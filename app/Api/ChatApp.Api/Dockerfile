FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o código-fonte dos módulos e restaura o projeto da API (com todo o seu grafo).
COPY app/ ./app/
RUN dotnet restore "app/Api/ChatApp.Api/ChatApp.Api.csproj"

WORKDIR "/src/app/Api/ChatApp.Api"
RUN dotnet publish "ChatApp.Api.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Porta padrão
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "ChatApp.Api.dll"]
