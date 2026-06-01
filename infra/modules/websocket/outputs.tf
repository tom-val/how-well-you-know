# Browser-facing wss:// URL (includes the stage), e.g. wss://abc.execute-api.<region>.amazonaws.com/prod
output "ws_connect_url" {
  value = aws_apigatewayv2_stage.default.invoke_url
}

# https:// endpoint the API Lambda uses to PostToConnection / DeleteConnection.
output "management_endpoint" {
  value = replace(aws_apigatewayv2_stage.default.invoke_url, "wss://", "https://")
}

# IAM resource ARN scope for execute-api:ManageConnections on this API/stage.
output "manage_connections_arn" {
  value = "${aws_apigatewayv2_api.ws.execution_arn}/${aws_apigatewayv2_stage.default.name}/POST/@connections/*"
}

output "connections_function_name" {
  value = aws_lambda_function.connections.function_name
}
