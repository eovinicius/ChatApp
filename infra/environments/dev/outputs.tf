output "alb_url" {
  value = "http://${module.networking.alb_dns_name}"
}

output "ecr_repository_url" {
  value = module.storage.ecr_repository_url
}

output "s3_bucket_name" {
  value = module.storage.s3_bucket_name
}
