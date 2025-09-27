dotnet ef migrations add creating_tables --project .\src\ChatApp.Infrastructure\ChatApp.Infrastructure.csproj --startup-project .\src\ChatApp.Api\

dotnet ef database update --project .\src\chatApp.Infrastructure\ --startup-project .\src\ChatApp.Api\

docker system prune -a --volumes

docker-compose up -d --build 'chatapp-db'

dotnet run --project .\src\ChatApp.Api\

dotnet run --project .\src\ChatApp.Api\ChatApp.Api.csproj

http://localhost:5110/swagger/index.html
