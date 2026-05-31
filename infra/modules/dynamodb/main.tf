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

  attribute {
    name = "user_name"
    type = "S"
  }

  # Lookup users by username for the create-or-return flow.
  global_secondary_index {
    name            = "user_name-index"
    hash_key        = "user_name"
    projection_type = "ALL"
  }
}
