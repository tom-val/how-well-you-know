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

output "memberships_table_name" {
  value = aws_dynamodb_table.memberships.name
}

output "memberships_table_arn" {
  value = aws_dynamodb_table.memberships.arn
}

output "connections_table_name" {
  value = aws_dynamodb_table.connections.name
}

output "connections_table_arn" {
  value = aws_dynamodb_table.connections.arn
}
