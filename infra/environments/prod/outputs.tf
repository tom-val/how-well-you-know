output "lambda_function_name" {
  value = module.lambda.function_name
}

output "api_endpoint" {
  value = module.api_gateway.api_endpoint
}

output "games_table_name" {
  value = module.dynamodb.games_table_name
}

output "users_table_name" {
  value = module.dynamodb.users_table_name
}

output "s3_frontend_bucket" {
  value = module.s3_frontend.bucket_id
}

output "cloudfront_distribution_id" {
  value = module.cloudfront.distribution_id
}

output "cloudfront_domain_name" {
  value = module.cloudfront.distribution_domain_name
}
