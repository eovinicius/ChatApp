locals {
  tags = {
    App         = var.app_name
    Environment = var.environment
    ManagedBy   = "terraform"
  }
}

resource "random_password" "db" {
  length  = 32
  special = false
}

resource "aws_secretsmanager_secret" "db" {
  name                    = "${var.app_name}/${var.environment}/database"
  recovery_window_in_days = var.environment == "prod" ? 7 : 0
  tags                    = local.tags
}

resource "aws_secretsmanager_secret_version" "db" {
  secret_id = aws_secretsmanager_secret.db.id
  secret_string = jsonencode({
    username          = var.db_username
    password          = random_password.db.result
    host              = aws_db_instance.main.address
    port              = "5432"
    dbname            = var.db_name
    connection_string = "Host=${aws_db_instance.main.address};Port=5432;Database=${var.db_name};Username=${var.db_username};Password=${random_password.db.result}"
  })

  depends_on = [aws_db_instance.main]
}

resource "aws_db_subnet_group" "main" {
  name       = "${var.app_name}-${var.environment}-db-subnet-group"
  subnet_ids = var.private_subnet_ids
  tags       = merge(local.tags, { Name = "${var.app_name}-${var.environment}-db-subnet-group" })
}

resource "aws_db_instance" "main" {
  identifier        = "${var.app_name}-${var.environment}"
  engine            = "postgres"
  engine_version    = "16"
  instance_class    = var.instance_class
  allocated_storage = var.allocated_storage

  db_name  = var.db_name
  username = var.db_username
  password = random_password.db.result

  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [var.rds_sg_id]

  multi_az                = var.multi_az
  backup_retention_period = var.backup_retention_period
  deletion_protection     = var.deletion_protection
  skip_final_snapshot     = !var.deletion_protection
  storage_encrypted       = true

  tags = merge(local.tags, { Name = "${var.app_name}-${var.environment}-db" })
}
