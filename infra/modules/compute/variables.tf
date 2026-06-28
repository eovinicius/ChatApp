variable "app_name" {
  type = string
}

variable "environment" {
  type = string
}

variable "aws_region" {
  type = string
}

variable "ecr_image_uri" {
  type        = string
  description = "Full ECR image URI including tag, e.g. 123456789.dkr.ecr.us-east-1.amazonaws.com/chatapp:latest"
}

variable "app_port" {
  type    = number
  default = 8080
}

variable "cpu" {
  type    = number
  default = 256
}

variable "memory" {
  type    = number
  default = 512
}

variable "desired_count" {
  type    = number
  default = 1
}

variable "vpc_id" {
  type = string
}

variable "ecs_subnet_ids" {
  type        = list(string)
  description = "Subnets where ECS tasks run. Use public subnets with assign_public_ip=true for dev (no NAT cost); private subnets for staging/prod."
}

variable "assign_public_ip" {
  type    = bool
  default = false
}

variable "ecs_sg_id" {
  type = string
}

variable "target_group_arn" {
  type = string
}

variable "db_secret_arn" {
  type        = string
  description = "ARN of the Secrets Manager secret containing the DB connection string"
}

variable "jwt_secret_arn" {
  type        = string
  description = "ARN of the Secrets Manager secret containing the JWT signing key"
}

variable "s3_bucket_name" {
  type = string
}

variable "aws_account_id" {
  type = string
}
