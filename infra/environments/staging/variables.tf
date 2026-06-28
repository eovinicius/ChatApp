variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "aws_account_id" {
  type = string
}

variable "ecr_image_tag" {
  type    = string
  default = "latest"
}

variable "jwt_secret_key" {
  type      = string
  sensitive = true
}
