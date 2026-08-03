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

## Autenticacion JWT

La API usa JWT Bearer para proteger endpoints.

### Variables necesarias

No guardes claves reales en `appsettings.json`. Configura estos valores como variables de entorno o User Secrets:

```text
JwtSettings__SecretKey=<clave-real-minimo-32-caracteres>
JwtSettings__Issuer=EcoSystem.API
JwtSettings__Audience=EcoSystem.Client
JwtSettings__ExpirationMinutes=60
SeedUsers__AdminPassword=<password-admin-seguro>
SeedUsers__UserPassword=<password-user-seguro>
```

`SeedUsers__AdminPassword` y `SeedUsers__UserPassword` solo se usan para crear usuarios de prueba si no existen. Las contrasenas se almacenan como hash usando `PasswordHasher<User>`.

### Endpoints de autenticacion

```text
POST /api/Auth/login
```

Cuerpo de ejemplo:

```json
{
  "username": "admin",
  "password": "tu-password-configurado-en-variable"
}
```

Respuestas esperadas:

- `400` si faltan datos.
- `401` si las credenciales son incorrectas.
- `200` si las credenciales son correctas. La respuesta incluye `token`, `expiresAt`, `username` y `role`.

### Uso del token en Swagger

1. Abre `/swagger`.
2. Ejecuta `POST /api/Auth/login` con el usuario de prueba.
3. Copia el valor de `token`.
4. Haz clic en **Authorize**.
5. Escribe:

```text
Bearer TU_TOKEN
```

6. Ejecuta endpoints protegidos.

### Endpoints protegidos

- `GET /api/Productos`: requiere cualquier usuario autenticado.
- `POST /api/Productos`: requiere rol `Admin`.
- `PUT /api/Productos/{id}`: requiere rol `Admin`.
- `DELETE /api/Productos/{id}`: requiere rol `Admin`.

### Migracion local

Se agrego la migracion `AddUsers` para crear la tabla `Users` en SQL Server local.

Comando:

```bash
dotnet ef database update --project EcoSystem.API/EcoSystem.API.csproj --startup-project EcoSystem.API/EcoSystem.API.csproj --context ApplicationDbContext
```

En Render no se aplican migraciones porque el despliegue de practica usa Entity Framework InMemory.
