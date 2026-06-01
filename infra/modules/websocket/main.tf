# WebSocket module — API Gateway WebSocket API plus the Node connections Lambda that
# handles $connect/$disconnect/subscribe and tracks open sockets in DynamoDB. The .NET
# API Lambda (elsewhere) pushes "game-changed" signals to these connections.

data "archive_file" "dummy" {
  type        = "zip"
  output_path = "${path.module}/dummy.zip"

  source {
    content  = "placeholder"
    filename = "placeholder.txt"
  }
}

# --- WebSocket API ---

resource "aws_apigatewayv2_api" "ws" {
  name                       = "${var.project_name}-${var.environment}-ws"
  protocol_type              = "WEBSOCKET"
  route_selection_expression = "$request.body.action"
}

resource "aws_apigatewayv2_integration" "connections" {
  api_id                    = aws_apigatewayv2_api.ws.id
  integration_type          = "AWS_PROXY"
  integration_uri           = aws_lambda_function.connections.invoke_arn
  integration_method        = "POST"
  content_handling_strategy = "CONVERT_TO_TEXT"
}

# $connect validates the Cognito token (query string) inside the Lambda and rejects
# with a non-2xx status; subscribe attaches a game_id to the connection row.
resource "aws_apigatewayv2_route" "connect" {
  api_id    = aws_apigatewayv2_api.ws.id
  route_key = "$connect"
  target    = "integrations/${aws_apigatewayv2_integration.connections.id}"
}

resource "aws_apigatewayv2_route" "disconnect" {
  api_id    = aws_apigatewayv2_api.ws.id
  route_key = "$disconnect"
  target    = "integrations/${aws_apigatewayv2_integration.connections.id}"
}

resource "aws_apigatewayv2_route" "subscribe" {
  api_id    = aws_apigatewayv2_api.ws.id
  route_key = "subscribe"
  target    = "integrations/${aws_apigatewayv2_integration.connections.id}"
}

resource "aws_apigatewayv2_route" "default" {
  api_id    = aws_apigatewayv2_api.ws.id
  route_key = "$default"
  target    = "integrations/${aws_apigatewayv2_integration.connections.id}"
}

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.ws.id
  name        = var.stage_name
  auto_deploy = true

  default_route_settings {
    throttling_rate_limit  = 100
    throttling_burst_limit = 50
  }
}

# --- Connections Lambda (Node) ---

resource "aws_iam_role" "connections" {
  name = "${var.project_name}-${var.environment}-ws-connections-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "lambda.amazonaws.com" }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "connections_basic_execution" {
  role       = aws_iam_role.connections.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_role_policy" "connections_dynamodb" {
  name = "${var.project_name}-${var.environment}-ws-connections-dynamodb"
  role = aws_iam_role.connections.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Action = [
        "dynamodb:PutItem",
        "dynamodb:UpdateItem",
        "dynamodb:DeleteItem",
      ]
      Resource = var.connections_table_arn
    }]
  })
}

resource "aws_cloudwatch_log_group" "connections" {
  name              = "/aws/lambda/${var.project_name}-${var.environment}-ws-connections"
  retention_in_days = 14
}

resource "aws_lambda_function" "connections" {
  function_name    = "${var.project_name}-${var.environment}-ws-connections"
  role             = aws_iam_role.connections.arn
  runtime          = "nodejs22.x"
  handler          = "index.handler"
  memory_size      = 128
  timeout          = 10
  filename         = data.archive_file.dummy.output_path
  source_code_hash = data.archive_file.dummy.output_base64sha256

  environment {
    variables = {
      COGNITO_USER_POOL_ID   = var.cognito_user_pool_id
      CONNECTIONS_TABLE_NAME = var.connections_table_name
    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.connections_basic_execution,
    aws_cloudwatch_log_group.connections,
  ]

  # Function code is deployed by the CI pipeline, not Terraform.
  lifecycle {
    ignore_changes = [filename, source_code_hash]
  }
}

resource "aws_lambda_permission" "ws_invoke" {
  statement_id  = "AllowWebSocketInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.connections.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.ws.execution_arn}/*"
}
