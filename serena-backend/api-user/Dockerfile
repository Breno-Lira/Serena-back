# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os .csproj primeiro para aproveitar o cache de restore
COPY ["api-user/api-user.csproj", "api-user/"]
COPY ["ServiceUser/ServiceUser.csproj", "ServiceUser/"]
COPY ["InfrastructureUser/InfrastructureUser.csproj", "InfrastructureUser/"]
COPY ["Dominio/DominioUser.csproj", "Dominio/"]
RUN dotnet restore "api-user/api-user.csproj"

# Copia o restante e publica
COPY . .
WORKDIR "/src/api-user"
RUN dotnet publish "api-user.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "api-user.dll"]
