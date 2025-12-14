# Étape 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copier le fichier csproj et restaurer les dépendances
COPY test1_Todo.csproj .
RUN dotnet restore

# Copier tout le code source et builder l'application
COPY . .
RUN dotnet build -c Release -o /app/build

# Étape 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Étape 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copier les fichiers publiés depuis l'étape publish
COPY --from=publish /app/publish .

# Créer le dossier Logs pour le logging
RUN mkdir -p /app/Logs

# Exposer le port 8080 (port par défaut pour ASP.NET Core)
EXPOSE 8080

# Variables d'environnement
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Point d'entrée de l'application
ENTRYPOINT ["dotnet", "test1_Todo.dll"]
