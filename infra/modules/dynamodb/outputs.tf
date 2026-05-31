output "games_table_name" {
  value = aws_dynamodb_table.games.name
}

output "games_table_arn" {
  value = aws_dynamodb_table.games.arn
}

output "users_table_name" {
  value = aws_dynamodb_table.users.name
}

output "users_table_arn" {
  value = aws_dynamodb_table.users.arn
}

output "users_index_name" {
  value = "user_name-index"
}
