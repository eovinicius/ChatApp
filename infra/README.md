# Infraestrutura — ChatApp (Terraform + AWS)

## Estrutura

```
infra/
├── modules/          # Módulos reutilizáveis
│   ├── networking/   # VPC, subnets, SGs, ALB
│   ├── compute/      # ECS Fargate + IAM
│   ├── database/     # RDS PostgreSQL + Secrets Manager
│   └── storage/      # S3 + ECR + JWT secret
└── environments/
    ├── dev/          # Sem NAT gateway, tasks em subnet pública
    ├── staging/      # Tasks em subnet privada, RDS t3.small
    └── prod/         # Multi-AZ, RDS t3.medium, 2 tasks ECS
```

## Pré-requisitos

1. [Terraform >= 1.6](https://developer.hashicorp.com/terraform/install)
2. AWS CLI configurado (`aws configure`)
3. Bucket S3 para o remote state criado manualmente:

```bash
aws s3 mb s3://chatapp-terraform-state --region us-east-1
aws s3api put-bucket-versioning \
  --bucket chatapp-terraform-state \
  --versioning-configuration Status=Enabled
```

## Como usar

```bash
# Substitua <env> por dev, staging ou prod
cd infra/environments/<env>

# Edite terraform.tfvars com seu AWS account ID
# Nunca commite jwt_secret_key — passe via env var:
export TF_VAR_jwt_secret_key="sua-chave-jwt-aqui"

terraform init
terraform validate
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"
```

## Deploy de nova imagem

```bash
# Build e push para ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin \
  <account-id>.dkr.ecr.us-east-1.amazonaws.com

docker build -t chatapp-dev ./app/src/ChatApp.Api
docker tag chatapp-dev:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/chatapp-dev:<tag>
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/chatapp-dev:<tag>

# Força novo deploy no ECS (sem mudança de infra)
aws ecs update-service \
  --cluster chatapp-dev \
  --service chatapp-dev \
  --force-new-deployment
```

## Diferenças por ambiente

| Configuração        | dev         | staging      | prod         |
|---------------------|-------------|--------------|--------------|
| NAT Gateway         | Não         | Sim          | Sim          |
| ECS subnet          | pública     | privada      | privada      |
| ECS desired count   | 1           | 1            | 2            |
| ECS CPU/Memória     | 256/512     | 512/1024     | 1024/2048    |
| RDS instance        | db.t3.micro | db.t3.small  | db.t3.medium |
| RDS Multi-AZ        | Não         | Não          | Sim          |
| Deletion protection | Não         | Não          | Sim          |
