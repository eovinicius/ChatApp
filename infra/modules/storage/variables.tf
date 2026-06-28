variable "app_name" {
  type = string
}

variable "environment" {
  type = string
}

variable "aws_region" {
  type = string
}

variable "jwt_secret_key" {
  type        = string
  sensitive   = true
  description = "JWT signing key. Pass via TF_VAR_jwt_secret_key env var — never commit this value."
}
