locals {
  project_name = "knowme"
  environment  = "prod"

  # The API only accepts cross-origin calls from the frontend's CloudFront URL,
  # any custom domains, plus any extra origins (e.g. local dev).
  cors_allowed_origins = concat(
    ["https://${module.cloudfront.distribution_domain_name}"],
    [for domain in var.cloudfront_aliases : "https://${domain}"],
    var.extra_cors_allowed_origins,
  )
}

# --- Auth (Cognito) ---

module "cognito" {
  source = "../../modules/cognito"

  project_name = local.project_name
  environment  = local.environment
}

# --- Persistence (DynamoDB) ---

module "dynamodb" {
  source = "../../modules/dynamodb"

  project_name = local.project_name
  environment  = local.environment
}

# --- Frontend (S3 + CloudFront) ---

module "s3_frontend" {
  source = "../../modules/s3-frontend"

  project_name = local.project_name
  environment  = local.environment
}

module "cloudfront" {
  source = "../../modules/cloudfront"

  project_name                   = local.project_name
  environment                    = local.environment
  s3_bucket_regional_domain_name = module.s3_frontend.bucket_regional_domain_name
  aliases                        = var.acm_certificate_arn != "" ? var.cloudfront_aliases : []
  acm_certificate_arn            = var.acm_certificate_arn
}

# S3 bucket policy granting CloudFront OAC read access.
resource "aws_s3_bucket_policy" "frontend" {
  bucket = module.s3_frontend.bucket_id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Sid       = "AllowCloudFrontOAC"
      Effect    = "Allow"
      Principal = { Service = "cloudfront.amazonaws.com" }
      Action    = "s3:GetObject"
      Resource  = "${module.s3_frontend.bucket_arn}/*"
      Condition = {
        StringEquals = {
          "AWS:SourceArn" = module.cloudfront.distribution_arn
        }
      }
    }]
  })
}

# --- Backend (API Gateway + Lambda) ---

module "lambda" {
  source = "../../modules/lambda"

  project_name              = local.project_name
  environment               = local.environment
  api_gateway_execution_arn = module.api_gateway.execution_arn
  games_table_name          = module.dynamodb.games_table_name
  games_table_arn           = module.dynamodb.games_table_arn
  users_table_name          = module.dynamodb.users_table_name
  users_table_arn           = module.dynamodb.users_table_arn
  users_index_name          = module.dynamodb.users_index_name
  cors_allowed_origins      = local.cors_allowed_origins
}

module "api_gateway" {
  source = "../../modules/api-gateway"

  project_name       = local.project_name
  environment        = local.environment
  lambda_invoke_arn  = module.lambda.invoke_arn
  cors_allow_origins = local.cors_allowed_origins
  authorizer_id      = module.lambda_authorizer.authorizer_id
}

module "lambda_authorizer" {
  source = "../../modules/lambda-authorizer"

  project_name          = local.project_name
  environment           = local.environment
  cognito_user_pool_id  = module.cognito.user_pool_id
  api_id                = module.api_gateway.api_id
  api_execution_arn     = module.api_gateway.execution_arn
  lambda_integration_id = module.api_gateway.lambda_integration_id
}
