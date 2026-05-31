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
