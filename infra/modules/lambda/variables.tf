variable "project_name" {
  type = string
}

variable "environment" {
  type = string
}

variable "api_gateway_execution_arn" {
  type = string
}

variable "games_table_name" {
  type = string
}

variable "games_table_arn" {
  type = string
}

variable "users_table_name" {
  type = string
}

variable "users_table_arn" {
  type = string
}

variable "memberships_table_name" {
  type = string
}

variable "memberships_table_arn" {
  type = string
}

variable "connections_table_name" {
  type = string
}

variable "connections_table_arn" {
  type = string
}

# Whether real-time WebSocket broadcasting is wired up. Known at plan time so it can gate
# the ManageConnections policy (the ARN itself is only known after apply).
variable "ws_enabled" {
  type    = bool
  default = false
}

# https:// endpoint used to push messages to WebSocket connections. Empty disables broadcasting.
variable "ws_management_endpoint" {
  type    = string
  default = ""
}

# IAM ARN scope for execute-api:ManageConnections.
variable "ws_manage_connections_arn" {
  type    = string
  default = ""
}

variable "cors_allowed_origins" {
  type    = list(string)
  default = []
}

variable "openai_api_key" {
  type      = string
  sensitive = true
  default   = ""
}
