# --- Étape 1: La base de l'image finale ---
# On utilise l'image ASP.NET Core Runtime de .NET 9. C'est léger et optimisé pour l'exécution.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Expose les ports pour HTTP et HTTPS. Change-les si ton app écoute sur d'autres ports.
EXPOSE 8180
EXPOSE 8181

# --- Étape 2: L'atelier de construction ---
# On utilise l'image du SDK .NET 9 complet, qui contient tous les outils pour construire le projet.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copie le fichier .csproj et restaure les dépendances NuGet.
# On fait ça en premier pour profiter du cache de Docker. Si les dépendances ne changent pas,
# cette étape (qui peut être longue) ne sera pas refaite lors des prochains builds.
# Le chemin "YooVisitUserAPI.csproj" est correct car le Dockerfile est à côté du .csproj.
COPY ["YooVisitUserAPI.csproj", "."]
RUN dotnet restore "./YooVisitUserAPI.csproj"

# Copie tout le reste du code source.
COPY . .
WORKDIR "/src/."

# Construit le projet en mode Release.
RUN dotnet build "YooVisitUserAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# --- Étape 3: La publication ---
# On prend le résultat du build et on le "publie", ce qui crée un package optimisé pour le déploiement.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "YooVisitUserAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# --- Étape 4: L'image finale ---
# On repart de l'image de base légère.
FROM base AS final
WORKDIR /app
# On copie SEULEMENT le résultat de la publication depuis l'étape "publish".
# C'est ce qui rend l'image finale petite et sécurisée (pas de code source, pas de SDK).
COPY --from=publish /app/publish .

# C'est le point d'entrée, la commande qui sera lancée au démarrage du conteneur.
ENTRYPOINT ["dotnet", "YooVisitUserAPI.dll"]