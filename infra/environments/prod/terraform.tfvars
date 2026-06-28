aws_region     = "us-east-1"
aws_account_id = "YOUR_AWS_ACCOUNT_ID"
# ecr_image_tag: sempre passe via CLI ou CI/CD com a tag da imagem do release
# terraform apply -var="ecr_image_tag=abc1234"

# jwt_secret_key: não commitar aqui. Use a env var:
#   export TF_VAR_jwt_secret_key="sua-chave-secreta"
