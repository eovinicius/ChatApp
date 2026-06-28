output "ecr_repository_url" {
  value = aws_ecr_repository.main.repository_url
}

output "s3_bucket_name" {
  value = aws_s3_bucket.files.bucket
}

output "jwt_secret_arn" {
  value = aws_secretsmanager_secret.jwt.arn
}
