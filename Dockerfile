# =========================
# BUILD
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar csproj y restaurar primero (cache-friendly)
COPY Inventarios.csproj ./
RUN dotnet restore "Inventarios.csproj"

# Copiar el resto del código
COPY . ./

# Publicar en Release
RUN dotnet publish "Inventarios.csproj" -c Release -o /app/out

# =========================
# RUNTIME
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Puerto que usa Render (puedes configurarlo en el dashboard; 10000 es el default sugerido)
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Inventarios.dll"]
