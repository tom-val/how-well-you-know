#!/usr/bin/env bash
#
# End-to-end smoke test against the deployed KnowMe API.
#
# Signs two existing Cognito users in (USER_PASSWORD_AUTH, no AWS creds needed),
# then plays a full 2-player game (create -> add questions -> join -> start ->
# answer/guess each question through the review step -> finish) and prints the
# final leaderboard.
#
# Requires: curl, jq. Credentials are taken from the environment so they are
# never committed:
#
#   P1_EMAIL=test@user.com  P1_PASSWORD=...  \
#   P2_EMAIL=test2@goog.com P2_PASSWORD=...  \
#   bash scripts/smoke-test.sh
#
set -euo pipefail

# --- Config (from terraform outputs; override via env) ----------------------
API="${API_ENDPOINT:-https://vv871irp29.execute-api.eu-central-1.amazonaws.com}"
POOL_ID="${COGNITO_USER_POOL_ID:-eu-central-1_zCRQTeb4F}"
CLIENT_ID="${COGNITO_CLIENT_ID:-2b0f625luhb9u4tu5mcu7t20st}"
REGION="${AWS_REGION:-${POOL_ID%%_*}}"

P1_EMAIL="${P1_EMAIL:?set P1_EMAIL}"
P1_PASSWORD="${P1_PASSWORD:?set P1_PASSWORD}"
P2_EMAIL="${P2_EMAIL:?set P2_EMAIL}"
P2_PASSWORD="${P2_PASSWORD:?set P2_PASSWORD}"

# If a user is in FORCE_CHANGE_PASSWORD state, the NEW_PASSWORD_REQUIRED challenge is
# answered with these (policy-compliant) permanent passwords. Re-run with P*_PASSWORD set
# to these afterwards.
P1_NEW_PASSWORD="${P1_NEW_PASSWORD:-SmokeTestA1}"
P2_NEW_PASSWORD="${P2_NEW_PASSWORD:-SmokeTestB2}"
# ---------------------------------------------------------------------------

API="${API%/}" # strip trailing slash
COGNITO_URL="https://cognito-idp.${REGION}.amazonaws.com/"
echo "API:  $API"
echo "Pool: $POOL_ID  (region $REGION)"
echo

for tool in curl jq; do
  command -v "$tool" >/dev/null 2>&1 || { echo "Missing required tool: $tool" >&2; exit 1; }
done

# --- Cognito sign-in (public USER_PASSWORD_AUTH flow) -----------------------

cognito() {
  local target="$1" payload="$2"
  curl -sS "$COGNITO_URL" \
    -H "X-Amz-Target: AWSCognitoIdentityProviderService.$target" \
    -H "Content-Type: application/x-amz-json-1.1" \
    -d "$payload"
}

login() {
  local email="$1" password="$2" newPassword="$3"

  local resp
  resp="$(cognito InitiateAuth \
    "{\"AuthFlow\":\"USER_PASSWORD_AUTH\",\"ClientId\":\"$CLIENT_ID\",\"AuthParameters\":{\"USERNAME\":\"$email\",\"PASSWORD\":\"$password\"}}")"

  local token
  token="$(jq -r '.AuthenticationResult.AccessToken // empty' <<<"$resp")"
  if [[ -n "$token" ]]; then
    printf '%s' "$token"
    return
  fi

  # First-login challenge: set a permanent, policy-compliant password and continue.
  if [[ "$(jq -r '.ChallengeName // empty' <<<"$resp")" == "NEW_PASSWORD_REQUIRED" ]]; then
    local session
    session="$(jq -r '.Session' <<<"$resp")"
    resp="$(cognito RespondToAuthChallenge \
      "{\"ChallengeName\":\"NEW_PASSWORD_REQUIRED\",\"ClientId\":\"$CLIENT_ID\",\"Session\":\"$session\",\"ChallengeResponses\":{\"USERNAME\":\"$email\",\"NEW_PASSWORD\":\"$newPassword\"}}")"
    token="$(jq -r '.AuthenticationResult.AccessToken // empty' <<<"$resp")"
    if [[ -n "$token" ]]; then
      echo "    NOTE: set permanent password for $email to '$newPassword'" >&2
      printf '%s' "$token"
      return
    fi
  fi

  echo "Sign-in failed for $email:" >&2
  jq '.' <<<"$resp" >&2 || echo "$resp" >&2
  exit 1
}

# --- HTTP helper: api <token> <METHOD> <path> [json-body] -------------------

api() {
  local token="$1" method="$2" path="$3" body="${4:-}"
  local args=(-sS -X "$method" "$API$path"
    -H "Authorization: Bearer $token"
    -H "Content-Type: application/json")
  [[ -n "$body" ]] && args+=(-d "$body")
  curl "${args[@]}"
}

# --- 1. Sign in + register --------------------------------------------------

echo "==> Signing in both users"
P1_TOKEN="$(login "$P1_EMAIL" "$P1_PASSWORD" "$P1_NEW_PASSWORD")"
P2_TOKEN="$(login "$P2_EMAIL" "$P2_PASSWORD" "$P2_NEW_PASSWORD")"

echo "==> Registering both as players"
P1_ID="$(api "$P1_TOKEN" POST /v1/users '{"userName":"Alice"}' | jq -r '.id')"
P2_ID="$(api "$P2_TOKEN" POST /v1/users '{"userName":"Bob"}'   | jq -r '.id')"
echo "    Alice = $P1_ID"
echo "    Bob   = $P2_ID"

# --- 2. Game setup ----------------------------------------------------------

echo "==> Alice creates a game"
GAME_ID="$(api "$P1_TOKEN" POST /v1/games '{"name":"Smoke test game"}' | jq -r '.id')"
echo "    game = $GAME_ID"

echo "==> Alice adds two questions"
api "$P1_TOKEN" POST "/v1/games/$GAME_ID/questions" \
  '{"text":"Favourite colour?","multipleAnswers":false,"variants":{"A":"Red","B":"Green","C":"Blue"}}' >/dev/null
api "$P1_TOKEN" POST "/v1/games/$GAME_ID/questions" \
  '{"text":"Best season?","multipleAnswers":false,"variants":{"A":"Spring","B":"Summer","C":"Winter"}}' >/dev/null

echo "==> Bob joins"
api "$P2_TOKEN" POST "/v1/games/$GAME_ID/join" >/dev/null

echo "==> Alice starts the game"
api "$P1_TOKEN" POST "/v1/games/$GAME_ID/start" >/dev/null

# --- 3. Play through every question -----------------------------------------

echo "==> Playing through questions"
for _ in $(seq 1 10); do
  game="$(api "$P1_TOKEN" GET "/v1/games/$GAME_ID")"
  status="$(jq -r '.status' <<<"$game")"
  phase="$(jq -r '.currentQuestionPhase' <<<"$game")"

  if [[ "$status" == "Ended" ]]; then
    echo "    game ended"
    break
  fi

  if [[ "$phase" == "Answering" ]]; then
    qid="$(jq -r '.currentQuestionId' <<<"$game")"
    echo "    question $qid: both answer A, then guess each other"

    api "$P1_TOKEN" POST "/v1/games/$GAME_ID/choices" '{"variantNotations":["A"]}' >/dev/null
    api "$P2_TOKEN" POST "/v1/games/$GAME_ID/choices" '{"variantNotations":["A"]}' >/dev/null

    api "$P1_TOKEN" POST "/v1/games/$GAME_ID/guesses" \
      "{\"choiceUserId\":\"$P2_ID\",\"variantNotations\":[\"A\"]}" >/dev/null
    api "$P2_TOKEN" POST "/v1/games/$GAME_ID/guesses" \
      "{\"choiceUserId\":\"$P1_ID\",\"variantNotations\":[\"A\"]}" >/dev/null
  fi

  echo "    advancing past review"
  api "$P1_TOKEN" POST "/v1/games/$GAME_ID/advance" >/dev/null
done

# --- 4. Results -------------------------------------------------------------

echo
echo "==> Final results"
api "$P1_TOKEN" GET "/v1/games/$GAME_ID/results" | jq '.'

echo
echo "==> Alice's games (GET /v1/games/mine)"
api "$P1_TOKEN" GET "/v1/games/mine" | jq '.'

echo
echo "Smoke test complete."
