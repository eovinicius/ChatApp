FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ChatApp.sln", "./"]
COPY ["app/src/ChatApp.Api/ChatApp.Api.csproj", "app/src/ChatApp.Api/"]
COPY ["app/src/ChatApp.Application/ChatApp.Application.csproj", "app/src/ChatApp.Application/"]
COPY ["app/src/ChatApp.Domain/ChatApp.Domain.csproj", "app/src/ChatApp.Domain/"]
COPY ["app/src/ChatApp.Infrastructure/ChatApp.Infrastructure.csproj", "app/src/ChatApp.Infrastructure/"]
COPY ["app/tests/ChatApp.UnitTests/ChatApp.UnitTests.csproj", "app/tests/ChatApp.UnitTests/"]

RUN dotnet restore "ChatApp.sln"

COPY . .
WORKDIR /src/app/src/ChatApp.Api
RUN dotnet publish "ChatApp.Api.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Porta padrão
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "ChatApp.Api.dll"]
