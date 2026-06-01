## Description

Quiz where you create questions and try to guess how the other players will answer.
Each question, every player picks their own answer and guesses what everyone else picked;
points are scored for correct guesses.

## Architecture

The app is being rewritten around a pure domain layer with an AWS-serverless backend.

- **Domain (`KnowMe.API/KnowMe.API.Domain`)** — framework-free game model (game lifecycle,
  per-question answer/guess flow, review step, scoring) with a `Result<T>` validation pattern.
- **Persistence (`KnowMe.API/KnowMe.API.Persistence`)** — DynamoDB. Each game is stored as a
  single aggregate item (JSON document) guarded by a version attribute for optimistic
  concurrency; users live in a separate table. A memberships table holds an inverted index
  (one row per player per game), written in the same transaction as the aggregate so
  "list my games" works without scanning.
- **API (`KnowMe.API/KnowMe.API`)** — .NET 10 minimal API hosted on AWS Lambda behind an
  HTTP API Gateway v2. Feature-folder slices (Users, Games). Live state by polling
  `GET /v1/games/{id}`.
- **Auth (Cognito + Lambda authorizer)** — a Cognito user pool issues JWTs; a Node.js Lambda
  authorizer (`KnowMe.API/authorizer`) validates them on every request and injects the verified
  user id (the token `sub`) into the request context. The app trusts only that id, so callers
  cannot impersonate each other. Domain users are keyed by the Cognito subject. For local dev
  and tests there is no Cognito — an `X-User-Id` header stands in for the authorizer.
- **Frontend (`frontend/`)** — React 19 + Vite + TypeScript SPA (react-router, TanStack Query,
  axios, `amazon-cognito-identity-js`, i18next lt/en). Cognito sign-up/sign-in, an axios client
  that attaches the access token, and a lobby (create / join / list-my-games). Served from a
  private S3 bucket via CloudFront (OAC, SPA routing); the API's CORS is wired to the CloudFront
  URL automatically. Gameplay screens (question setup, answer/guess/review, results) are next.
- **Infrastructure (`infra/`)** — modular Terraform (DynamoDB, Lambda, API Gateway, S3 +
  CloudFront), composed in `infra/environments/prod` with an S3 state backend.
- **CI/CD (`.github/workflows/deploy-prod.yml`)** — on push to `main`: build & test, `terraform
  apply`, then in parallel publish the .NET 10 `linux-arm64` Lambda, the Node.js authorizer, and
  the frontend (build → S3 sync → CloudFront invalidation).

> The legacy combined API + React UI lives in `HowWellYouKnow.API/` and is being retired.

## Tech Stack

- .NET 10 (minimal API on AWS Lambda, ARM64)
- DynamoDB
- AWS Cognito + Node.js Lambda authorizer
- Terraform
- GitHub Actions
- xUnit + FluentAssertions

## Commands

```bash
# Build & test the backend solution
cd KnowMe.API && dotnet test

# Run the API locally (expects DynamoDB Local on http://localhost:8000)
cd KnowMe.API && dotnet run --project KnowMe.API

# Plan infrastructure
cd infra/environments/prod && terraform init && terraform plan

# Frontend (dev server proxies /v1 to the deployed API, so no CORS faff)
cd frontend && npm install && npm run dev
```

## Deployment

### 1. Bootstrap AWS resources

[`infra/bootstrap.sh`](infra/bootstrap.sh) creates everything the Terraform backend and
CI/CD need: the state S3 bucket, the DynamoDB lock table, the GitHub Actions OIDC provider,
and the IAM role GitHub Actions assumes to deploy. It is idempotent — safe to re-run.

The easiest way is **AWS CloudShell** (it already has the AWS CLI and admin credentials):

1. Open the [AWS Console](https://console.aws.amazon.com/), pick region **eu-central-1**,
   and launch **CloudShell** (the terminal icon in the top bar).
2. Upload the script (CloudShell **Actions → Upload file**) or paste its contents, then run:

   ```bash
   bash bootstrap.sh
   ```

3. Copy the `AWS_ROLE_ARN` it prints at the end.

It creates:

| Resource | Name |
| --- | --- |
| State bucket (versioned, encrypted, private) | `knowme-terraform-state` |
| Lock table | `knowme-terraform-locks` |
| OIDC provider | `token.actions.githubusercontent.com` |
| Deploy role | `knowme-github-actions` |

The deploy role is scoped to what `terraform apply` and the deploy steps manage: the
Terraform state, DynamoDB, Lambda, API Gateway, Cognito, CloudWatch logs, the frontend S3
bucket, CloudFront, and IAM roles named `knowme-*`.

### 2. Add the GitHub secret

In the repo: **Settings → Secrets and variables → Actions → New repository secret**

- `AWS_ROLE_ARN` — the role ARN printed by the script

### 3. Deploy

Push to `main` — [`.github/workflows/deploy-prod.yml`](.github/workflows/deploy-prod.yml) runs
build & test → `terraform apply` → publish the .NET 10 `linux-arm64` Lambda →
`aws lambda update-function-code`. The API comes up on the HTTP API Gateway URL shown in the
Terraform output `api_endpoint`.

To run Terraform locally instead of via CI (requires the bootstrap above):

```bash
cd infra/environments/prod
terraform init
terraform plan
terraform apply
```

### Custom domain (frontend)

The frontend is served at **https://quiz.valiunas.dev**. This is wired in `prod` via two
variables: `cloudfront_aliases = ["quiz.valiunas.dev"]` and `acm_certificate_arn` (a wildcard
`*.valiunas.dev` cert in **us-east-1** — CloudFront only accepts certs from that region). The
alias is attached to CloudFront and added to the API's allowed CORS origins automatically.

DNS lives outside AWS (Cloudflare / registrar), so after `terraform apply` add one record at
the DNS provider:

```
quiz.valiunas.dev   CNAME   <cloudfront_domain_name>   # Terraform output
```

To serve on the default `*.cloudfront.net` URL instead, apply with `-var='acm_certificate_arn='`.
For local frontend dev the Vite server proxies `/v1` to the API (no CORS), so no origin change
is needed; otherwise add one with `-var='extra_cors_allowed_origins=["http://localhost:5173"]'`.

## API endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/v1/users` | Register the current user with a display name |
| GET | `/v1/users/me` | Get the current user's profile |
| GET | `/v1/users/{id}` | Get a user (e.g. another player) |
| POST | `/v1/games` | Create a game (creator is the current user) |
| GET | `/v1/games/mine` | List games the current user created or joined |
| GET | `/v1/games/{id}` | Get game state |
| GET | `/v1/games/{id}/results` | Get the leaderboard and per-question results |
| POST | `/v1/games/{id}/join` | Join a game |
| POST | `/v1/games/{id}/questions` | Add a question |
| POST | `/v1/games/{id}/start` | Start the game |
| POST | `/v1/games/{id}/choices` | Record your own answer |
| POST | `/v1/games/{id}/guesses` | Guess another player's answer |
| POST | `/v1/games/{id}/advance` | Move past the review step to the next question |

All routes except `GET /health` require an `Authorization: Bearer <Cognito JWT>` header.
The identity is taken from the verified token — never from the client. (Locally, with no
Cognito, an `X-User-Id: <guid>` header stands in.)

Previously seen live here: https://quiz.valiunas.dev/
