# EcoSystem Connect API

API ASP.NET Core para la practica de EcoSystem Connect.

## Tecnologia

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server para ejecucion local
- Entity Framework InMemory para despliegue temporal en Render

## Ejecucion local

La configuracion local conserva SQL Server con SQLEXPRESS usando la cadena de conexion de `EcoSystem.API/appsettings.json`:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EcoSystemDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

Para ejecutar localmente:

```bash
dotnet restore EcoSystem.API/EcoSystem.API.csproj
dotnet build EcoSystem.API/EcoSystem.API.csproj
dotnet run --project EcoSystem.API/EcoSystem.API.csproj
```

Swagger local queda disponible en:

```text
http://localhost:5203/swagger
```

Si se usa otro puerto local, abre `/swagger` en el puerto que muestre la consola.

## Render

Render ejecuta la API con Docker usando el `Dockerfile` ubicado en la raiz del repositorio.

En Render la aplicacion escucha en `0.0.0.0` usando la variable `PORT`. Si `PORT` no existe, usa `10000`.

Para esta practica, cuando la variable `RENDER=true` esta configurada, la API usa una base de datos temporal en memoria con Entity Framework InMemory. Esto permite abrir Swagger y probar endpoints sin publicar secretos ni configurar una base SQL Server externa.

Swagger publico esperado:

```text
https://NOMBRE-DEL-SERVICIO.onrender.com/swagger
```

Endpoint de salud:

```text
GET /health
```

## Pasos basicos de despliegue

1. Crear un Web Service en Render desde este repositorio.
2. Seleccionar la rama `feature/fase2-frontend`.
3. Seleccionar runtime Docker.
4. Usar el `Dockerfile` de la raiz del repositorio.
5. Configurar variables de entorno:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `RENDER=true`
   - `PORT=10000`
6. Desplegar y revisar logs hasta que el servicio aparezca como Live.
7. Abrir `/swagger` en la URL publica del servicio.
8. Probar al menos tres endpoints desde Swagger y documentar metodo, ruta, codigo HTTP y observaciones.
