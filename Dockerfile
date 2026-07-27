# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore AIImageChat.Simple/AIImageChat.Simple.csproj

RUN dotnet publish AIImageChat.Simple/AIImageChat.Simple.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

# Apenas para documentação; o Railway usa a variável PORT
EXPOSE 8080

# Mostrar os arquivos publicados no log (remova depois se quiser)
RUN ls -la /app

ENTRYPOINT ["dotnet", "AIImageChat.Simple.dll"]