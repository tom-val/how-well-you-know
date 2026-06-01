variable "project_name" {
  type = string
}

variable "environment" {
  type = string
}

variable "stage_name" {
  type    = string
  default = "prod"
}

variable "cognito_user_pool_id" {
  type = string
}

variable "connections_table_name" {
  type = string
}

variable "connections_table_arn" {
  type = string
}
