# 🚀 GUÍA DE DESPLIEGUE

## 📋 Tabla de Contenidos
1. [Requisitos Previos](#requisitos-previos)
2. [Configuración Local](#configuración-local)
3. [Compilación y Ejecución](#compilación-y-ejecución)
4. [Despliegue en Producción](#despliegue-en-producción)
5. [Solución de Problemas](#solución-de-problemas)

---

## 📌 Requisitos Previos

### Software Necesario
```
✅ .NET SDK 9.0 o superior
✅ PostgreSQL 14+ (o Render Cloud)
✅ Visual Studio 2022 / VS Code / Rider
✅ Git
✅ npm (opcional, para actualizaciones futuras)
```

### Verificar Instalación
```powershell
# Verificar .NET
dotnet --version

# Verificar PostgreSQL
psql --version

# Verificar Git
git --version
```

---

## ⚙️ Configuración Local

### 1. Clonar o Descargar el Proyecto
```bash
cd /ruta/del/proyecto
git clone <repo-url>  # Si está en repo
```

### 2. Verificar Archivo de Configuración
```
Archivo: appsettings.json
Ubicación: Raíz del proyecto
```

**Contenido esperado:**
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=dpg-xxx.postgres.render.com;Port=5432;Database=xxx;Username=xxx;Password=xxx;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Variables de Entorno (Opcional pero Recomendado)
```powershell
# En Windows (PowerShell)
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "https://localhost:7001"

# En Linux/Mac
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=https://localhost:7001
```

---

## 🔨 Compilación y Ejecución

### 1. Restaurar Dependencias
```bash
dotnet restore
```

### 2. Compilar Proyecto
```bash
# Compilación Debug
dotnet build

# Compilación Release
dotnet build -c Release
```

### 3. Aplicar Migraciones de BD
```bash
# Ver migraciones pendientes
dotnet ef migrations list

# Aplicar migraciones
dotnet ef database update

# Si necesitas crear migraciones nuevas
dotnet ef migrations add NombreMigracion
```

### 4. Ejecutar Localmente
```bash
# Desarrollo
dotnet run

# Con modo hot-reload
dotnet watch run

# Release
dotnet run -c Release
```

**URLs de Acceso Local:**
```
HTTP:   http://localhost:5000
HTTPS:  https://localhost:7001
```

### 5. Crear Primer Usuario Admin (Importante)
```
1. Inicia la aplicación
2. Accede a /Usuarios/Crear
3. Crea un usuario con rol "Admin"
4. Usa ese usuario para crear otros usuarios
```

---

## 🌐 Despliegue en Producción

### Opción 1: Render (Recomendado)

#### Paso 1: Preparar GitHub
```bash
git add .
git commit -m "Rediseño visual - Sistema listo para producción"
git push origin main
```

#### Paso 2: Crear Servicio en Render
1. Ir a https://render.com
2. Conectar repositorio de GitHub
3. Crear nuevo "Web Service"
4. Configurar:
   ```
   Build Command: dotnet publish -c Release -o /etc/render/out
   Start Command: cd /etc/render/out && dotnet Inventarios.dll
   ```

#### Paso 3: Variables de Entorno en Render
```
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://0.0.0.0:10000
ConnectionStrings__Postgres = [tu-connection-string]
```

#### Paso 4: Desplegar
- Render automáticamente desplegará desde cada push a main

### Opción 2: Azure App Service

#### Paso 1: Crear App Service
```bash
# Crear grupo de recursos
az group create --name myResourceGroup --location "eastus"

# Crear App Service Plan
az appservice plan create --name myAppServicePlan --resource-group myResourceGroup --sku B1

# Crear Web App
az webapp create --resource-group myResourceGroup --plan myAppServicePlan --name myAppName
```

#### Paso 2: Desplegar con Git
```bash
# Configurar deployment credentials
az webapp deployment user set --user-name <username> --password <password>

# Git remote
git remote add azure <git-url>
git push azure main
```

### Opción 3: Servidor Linux/Docker

#### Crear Dockerfile
```dockerfile
# Crear archivo: Dockerfile (raíz del proyecto)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Inventarios.csproj", "."]
RUN dotnet restore "Inventarios.csproj"
COPY . .
RUN dotnet build "Inventarios.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 80
ENTRYPOINT ["dotnet", "Inventarios.dll"]
```

#### Construir y ejecutar
```bash
# Construir imagen
docker build -t inventarios:latest .

# Ejecutar contenedor
docker run -d -p 8080:80 \
  -e ConnectionStrings__Postgres="..." \
  -e ASPNETCORE_ENVIRONMENT="Production" \
  inventarios:latest
```

---

## 🔒 Seguridad en Producción

### 1. Configuración HTTPS
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:80"
      },
      "HttpsDefault": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "/etc/ssl/certs/server.pem",
          "Password": "your-password"
        }
      }
    }
  }
}
```

### 2. Credenciales Seguras
```bash
# NO poner contraseñas en appsettings.json
# Usar User Secrets en desarrollo
dotnet user-secrets set "ConnectionStrings:Postgres" "..."

# En producción, usar variables de entorno
export ConnectionStrings__Postgres="..."
```

### 3. CORS (Si necesitas API)
```csharp
// En Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("https://tudominio.com")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

app.UseCors("AllowSpecificOrigin");
```

### 4. Headers de Seguridad
```csharp
// En Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

---

## 📊 Base de Datos

### Render PostgreSQL (Actual)
```
Host: dpg-d65tdr248b3s73ehe4vg-a.oregon-postgres.render.com
Base de datos: multiservicios_ayo
Usuario: multiservicios_ayo_user
```

### Usar Base de Datos Local (Desarrollo)
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=inventarios;Username=postgres;Password=tu-password"
  }
}
```

### Instalar PostgreSQL Local
```bash
# Windows (Chocolatey)
choco install postgresql

# Mac (Homebrew)
brew install postgresql@14

# Linux (Ubuntu)
sudo apt-get install postgresql postgresql-contrib
```

---

## 📈 Monitoreo y Logs

### Logs en Desarrollo
```csharp
// Ya configurado en appsettings.json
// Usa ILogger<T> en tus controllers

public HomeController(ILogger<HomeController> logger)
{
    _logger = logger;
}

_logger.LogInformation("Mensaje informativo");
_logger.LogWarning("Advertencia");
_logger.LogError("Error");
```

### Logs en Producción
```csharp
// Usar Application Insights (Azure)
// O Serilog para archivo

builder.Services.AddApplicationInsightsTelemetry();
```

### Ver Logs en Render
```
Dashboard Render → Logs
```

---

## ✅ Checklist Pre-Despliegue

- [ ] Proyecto compila sin errores
- [ ] Todas las vistas se cargan correctamente
- [ ] CSS y JS se cargan (revisar en DevTools)
- [ ] Migraciones de BD aplicadas
- [ ] Primer usuario Admin creado
- [ ] Login funciona correctamente
- [ ] Crud de herramientas funciona
- [ ] Crud de préstamos funciona
- [ ] Crud de usuarios funciona
- [ ] Búsqueda en tiempo real funciona
- [ ] Validación de formularios funciona
- [ ] Contraseños se encriptan correctamente
- [ ] Roles se aplican correctamente
- [ ] Responsive en móvil verificado
- [ ] No hay errores en consola navegador
- [ ] No hay errores en logs servidor

---

## 🐛 Solución de Problemas

### Error: "Connection refused"
```
Problema: No puede conectar a PostgreSQL

Solución:
1. Verifica que PostgreSQL está corriendo
2. Verifica credenciales en appsettings.json
3. Verifica que puerto 5432 está abierto
```

### Error: "CSS/JS no cargan"
```
Problema: Estilos y scripts no aparecen

Solución:
1. Limpia caché navegador (Ctrl+Shift+Del)
2. Hard refresh (Ctrl+F5)
3. Verifica que wwwroot/ existe
4. Comprueba rutas en _Layout.cshtml
```

### Error: "Migrations pending"
```
Problema: BD no sincronizada con modelo

Solución:
dotnet ef database update
```

### Error: "UserSecrets not found"
```
Problema: Faltan secrets en desarrollo

Solución:
dotnet user-secrets init
dotnet user-secrets set "key" "value"
```

### Error: "401 Unauthorized"
```
Problema: No autorizado para acceder recurso

Solución:
1. Verifica que estás logueado
2. Verifica roles requeridos
3. Revisa [Authorize(Roles="...")] en controller
```

---

## 🚀 Optimizaciones para Producción

### 1. Compilación Release
```bash
dotnet publish -c Release -o ./publish
```

### 2. Minificación CSS/JS
```csharp
// Ya incluida en Bootstrap bundling
// Agregar en _Layout.cshtml si necesario:
<link rel="stylesheet" href="~/css/site.min.css" asp-append-version="true" />
```

### 3. Caching HTTP
```csharp
app.UseResponseCaching();

[ResponseCache(Duration = 60)]
public IActionResult Index()
{
    return View();
}
```

### 4. Compression
```csharp
builder.Services.AddResponseCompression(opts =>
{
    opts.GzipCompressionLevel = CompressionLevel.Optimal;
});

app.UseResponseCompression();
```

---

## 📞 Contacto y Soporte

### Problemas Comunes
- Revisar **SISTEMA_ACTUALIZADO.md**
- Revisar **CAMBIOS_VISUALES.md**
- Revisar **README_REDISENO.md**

### Para Ayuda
1. Revisar consola navegador (F12)
2. Revisar logs del servidor
3. Crear issue en GitHub
4. Contactar administrador

---

## 📚 Referencias Útiles

- [Microsoft ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Docs](https://docs.microsoft.com/ef/core)
- [PostgreSQL Documentation](https://www.postgresql.org/docs)
- [Render Deployment Guide](https://render.com/docs)
- [Bootstrap Documentation](https://getbootstrap.com/docs)

---

**Guía de Despliegue Actualizada**: 2026-02-10  
**Versión**: 2.0  
**Estado**: ✅ Completa

