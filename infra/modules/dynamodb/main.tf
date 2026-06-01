# DynamoDB module — games (single-item aggregate) and users tables.

resource "aws_dynamodb_table" "games" {
  name         = "${var.project_name}-${var.environment}-games"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "id"

  attribute {
    name = "id"
    type = "S"
  }
}

resource "aws_dynamodb_table" "users" {
  name         = "${var.project_name}-${var.environment}-users"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "id"

  attribute {
    name = "id"
    type = "S"
  }
}

# Inverted index: one row per (user, game) so a player's games can be listed.
resource "aws_dynamodb_table" "memberships" {
  name         = "${var.project_name}-${var.environment}-memberships"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "user_id"
  range_key    = "game_id"

  attribute {
    name = "user_id"
    type = "S"
  }

  attribute {
    name = "game_id"
    type = "S"
  }
}

# Live WebSocket connections. One row per open socket; the GSI lets the API broadcast
# a "game-changed" signal to every connection subscribed to a given game. A TTL on
# `ttl` reaps rows that survive an ungraceful disconnect.
resource "aws_dynamodb_table" "connections" {
  name         = "${var.project_name}-${var.environment}-connections"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "connection_id"

  attribute {
    name = "connection_id"
    type = "S"
  }

  attribute {
    name = "game_id"
    type = "S"
  }

  global_secondary_index {
    name            = "game_id-index"
    hash_key        = "game_id"
    projection_type = "KEYS_ONLY"
  }

  ttl {
    attribute_name = "ttl"
    enabled        = true
  }
}
