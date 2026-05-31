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
  concurrency; users live in a separate table with a username GSI.
- **API (`KnowMe.API/KnowMe.API`)** — .NET 10 minimal API hosted on AWS Lambda behind an
  HTTP API Gateway v2. Feature-folder slices (Users, Games). Simple username-based auth via
  the `X-User-Id` header; live state by polling `GET /v1/games/{id}`.
- **Frontend (planned)** — a private S3 bucket served by a CloudFront distribution (OAC, SPA
  routing) is already provisioned by Terraform, ready for the frontend app. The API's CORS
  policy is wired to the CloudFront URL automatically.
- **Infrastructure (`infra/`)** — modular Terraform (DynamoDB, Lambda, API Gateway, S3 +
  CloudFront), composed in `infra/environments/prod` with an S3 state backend.
- **CI/CD (`.github/workflows/deploy-prod.yml`)** — on push to `main`: build & test, `terraform
  apply`, then publish the .NET 10 `linux-arm64` Lambda and update the function code. A
  frontend build/sync/invalidate job will be added alongside the frontend app.

> The legacy combined API + React UI lives in `HowWellYouKnow.API/` and is being retired.

## Tech Stack

- .NET 10 (minimal API on AWS Lambda, ARM64)
- DynamoDB
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
Terraform state, DynamoDB, Lambda, API Gateway, CloudWatch logs, the frontend S3 bucket,
CloudFront, and IAM roles named `knowme-*`.

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

The frontend serves on the default `*.cloudfront.net` URL (Terraform output
`cloudfront_domain_name`) until a domain is wired up. To use a custom domain, create an ACM
certificate in **us-east-1** (CloudFront only accepts certs from that region), then pass it
along with the domain(s):

```bash
terraform apply \
  -var='acm_certificate_arn=arn:aws:acm:us-east-1:...:certificate/...' \
  -var='cloudfront_aliases=["knowme.valiunas.dev"]'
```

This enables the alias on CloudFront and adds it to the API's allowed CORS origins. Point the
domain's DNS at the CloudFront distribution afterwards. For local frontend development, allow
the dev origin without a deploy via `-var='extra_cors_allowed_origins=["http://localhost:5173"]'`.

## API endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/v1/users` | Create or return a user by username |
| GET | `/v1/users/{id}` | Get a user |
| POST | `/v1/games` | Create a game (creator from `X-User-Id`) |
| GET | `/v1/games/{id}` | Get game state |
| GET | `/v1/games/{id}/results` | Get the leaderboard and per-question results |
| POST | `/v1/games/{id}/join` | Join a game |
| POST | `/v1/games/{id}/questions` | Add a question |
| POST | `/v1/games/{id}/start` | Start the game |
| POST | `/v1/games/{id}/choices` | Record your own answer |
| POST | `/v1/games/{id}/guesses` | Guess another player's answer |
| POST | `/v1/games/{id}/advance` | Move past the review step to the next question |

All game routes (except user creation) require the `X-User-Id` header.

Previously seen live here: https://quiz.valiunas.dev/
