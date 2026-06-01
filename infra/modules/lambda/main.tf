# Lambda module — .NET 10 API function, IAM role, DynamoDB access, and log group.

data "archive_file" "dummy" {
  type        = "zip"
  output_path = "${path.module}/dummy.zip"

  source {
    content  = "placeholder"
    filename = "placeholder.txt"
  }
}

resource "aws_iam_role" "lambda_execution" {
  name = "${var.project_name}-${var.environment}-lambda-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "lambda.amazonaws.com" }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.lambda_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# Least-privilege access to the two tables and the users GSI.
resource "aws_iam_role_policy" "dynamodb" {
  name = "${var.project_name}-${var.environment}-lambda-dynamodb"
  role = aws_iam_role.lambda_execution.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Action = [
        "dynamodb:GetItem",
        "dynamodb:PutItem",
        "dynamodb:Query",
        "dynamodb:UpdateItem",
        "dynamodb:DeleteItem",
      ]
      Resource = [
        var.games_table_arn,
        var.users_table_arn,
        var.memberships_table_arn,
        var.connections_table_arn,
        "${var.connections_table_arn}/index/*",
      ]
    }]
  })
}

# Allow the API to push "game-changed" signals to live WebSocket connections.
resource "aws_iam_role_policy" "manage_connections" {
  count = var.ws_manage_connections_arn == "" ? 0 : 1

  name = "${var.project_name}-${var.environment}-lambda-ws"
  role = aws_iam_role.lambda_execution.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = "execute-api:ManageConnections"
      Resource = var.ws_manage_connections_arn
    }]
  })
}

resource "aws_cloudwatch_log_group" "lambda" {
  name              = "/aws/lambda/${var.project_name}-${var.environment}"
  retention_in_days = 14
}

resource "aws_lambda_function" "api" {
  function_name    = "${var.project_name}-${var.environment}"
  role             = aws_iam_role.lambda_execution.arn
  runtime          = "dotnet10"
  handler          = "KnowMe.API"
  architectures    = ["arm64"]
  memory_size      = 512
  timeout          = 30
  filename         = data.archive_file.dummy.output_path
  source_code_hash = data.archive_file.dummy.output_base64sha256

  environment {
    variables = merge(
      {
        ASPNETCORE_ENVIRONMENT         = "Production"
        DynamoDb__GamesTableName       = var.games_table_name
        DynamoDb__UsersTableName       = var.users_table_name
        DynamoDb__MembershipsTableName = var.memberships_table_name
        DynamoDb__ConnectionsTableName = var.connections_table_name
        WebSocket__ManagementEndpoint  = var.ws_management_endpoint
        OpenAi__ApiKey                 = var.openai_api_key
      },
      { for i, origin in var.cors_allowed_origins : "Cors__AllowedOrigins__${i}" => origin }
    )
  }

  depends_on = [
    aws_iam_role_policy_attachment.lambda_basic_execution,
    aws_cloudwatch_log_group.lambda,
  ]

  # Function code is deployed by the CI pipeline, not Terraform.
  lifecycle {
    ignore_changes = [filename, source_code_hash]
  }
}

resource "aws_lambda_permission" "api_gateway" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.api.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${var.api_gateway_execution_arn}/*/*"
}
