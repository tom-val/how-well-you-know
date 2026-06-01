output "user_pool_id" {
  value = aws_cognito_user_pool.main.id
}

output "user_pool_client_id" {
  value = aws_cognito_user_pool_client.spa.id
}

output "user_pool_endpoint" {
  value = aws_cognito_user_pool.main.endpoint
}

# Hosted-UI host (e.g. knowme-prod.auth.eu-central-1.amazoncognito.com) for the SPA OAuth flow.
output "hosted_ui_domain" {
  value = "${aws_cognito_user_pool_domain.main.domain}.auth.${var.aws_region}.amazoncognito.com"
}
