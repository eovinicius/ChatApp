terraform {
  required_version = ">= 1.6"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }

  backend "s3" {
    bucket = "chatapp-terraform-state"
    key    = "staging/terraform.tfstate"
    region = "us-east-1"
  }
}

provider "aws" {
  region = var.aws_region
}

locals {
  app_name    = "chatapp"
  environment = "staging"
}

module "networking" {
  source = "../../modules/networking"

  app_name    = local.app_name
  environment = local.environment

  vpc_cidr             = "10.1.0.0/16"
  public_subnet_cidrs  = ["10.1.1.0/24", "10.1.2.0/24"]
  private_subnet_cidrs = ["10.1.11.0/24", "10.1.12.0/24"]
  availability_zones   = ["${var.aws_region}a", "${var.aws_region}b"]

  enable_nat_gateway = true
  app_port           = 8080
}

module "storage" {
  source = "../../modules/storage"

  app_name       = local.app_name
  environment    = local.environment
  aws_region     = var.aws_region
  jwt_secret_key = var.jwt_secret_key
}

module "database" {
  source = "../../modules/database"

  app_name    = local.app_name
  environment = local.environment

  instance_class          = "db.t3.small"
  allocated_storage       = 20
  multi_az                = false
  backup_retention_period = 3
  deletion_protection     = false

  private_subnet_ids = module.networking.private_subnet_ids
  rds_sg_id          = module.networking.rds_sg_id
}

module "compute" {
  source = "../../modules/compute"

  app_name    = local.app_name
  environment = local.environment
  aws_region  = var.aws_region

  ecr_image_uri = "${var.aws_account_id}.dkr.ecr.${var.aws_region}.amazonaws.com/${local.app_name}-${local.environment}:${var.ecr_image_tag}"
  app_port      = 8080
  cpu           = 512
  memory        = 1024
  desired_count = 1

  ecs_subnet_ids   = module.networking.private_subnet_ids
  assign_public_ip = false

  vpc_id           = module.networking.vpc_id
  ecs_sg_id        = module.networking.ecs_sg_id
  target_group_arn = module.networking.target_group_arn

  db_secret_arn  = module.database.db_secret_arn
  jwt_secret_arn = module.storage.jwt_secret_arn
  s3_bucket_name = module.storage.s3_bucket_name
  aws_account_id = var.aws_account_id
}
