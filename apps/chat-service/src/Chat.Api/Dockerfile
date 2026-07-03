FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["apps/chat-service/Chat.sln", "apps/chat-service/"]
COPY ["apps/chat-service/src/Chat.Api/Chat.Api.csproj", "apps/chat-service/src/Chat.Api/"]
COPY ["apps/chat-service/src/Chat.Application/Chat.Application.csproj", "apps/chat-service/src/Chat.Application/"]
COPY ["apps/chat-service/src/Chat.Domain/Chat.Domain.csproj", "apps/chat-service/src/Chat.Domain/"]
COPY ["apps/chat-service/src/Chat.Infrastructure/Chat.Infrastructure.csproj", "apps/chat-service/src/Chat.Infrastructure/"]
COPY ["apps/chat-service/tests/Chat.UnitTests/Chat.UnitTests.csproj", "apps/chat-service/tests/Chat.UnitTests/"]

RUN dotnet restore "apps/chat-service/Chat.sln"

COPY . .
WORKDIR /src/apps/chat-service/src/Chat.Api
RUN dotnet publish "Chat.Api.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Porta padrão
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Chat.Api.dll"]
