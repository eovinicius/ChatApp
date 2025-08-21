FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ChatApp.sln", "./"]
COPY ["src/ChatApp.Api/ChatApp.Api.csproj", "src/ChatApp.Api/"]
COPY ["src/ChatApp.Application/ChatApp.Application.csproj", "src/ChatApp.Application/"]
COPY ["src/ChatApp.Domain/ChatApp.Domain.csproj", "src/ChatApp.Domain/"]
COPY ["src/ChatApp.Infrastructure/ChatApp.Infrastructure.csproj", "src/ChatApp.Infrastructure/"]
COPY ["test/ChatApp.UnitTests/ChatApp.UnitTests.csproj", "test/ChatApp.UnitTests/"]

RUN dotnet restore "ChatApp.sln"

COPY . .
WORKDIR /src/src/ChatApp.Api
RUN dotnet publish "ChatApp.Api.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Porta padrão
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "ChatApp.Api.dll"]
