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

## Fase 2 - Cliente EcoSystem Connect

Se agrego el proyecto `EcoSystem.Client` con estructura MVVM solicitada para las firmas 1 a 6:

- `Models`: `Producto`, `LoginRequest`, `LoginResponse`.
- `Views`: `LoginPage`, `ProductosPage`, `ProductoFormPage`.
- `ViewModels`: login, listado y formulario de productos.
- `Services`: `ApiService`, `AuthService`, `AuthHeaderHandler`, `TokenService`.

El cliente consume la API publicada en Render:

```text
https://ecosystem-connect-api.onrender.com/
```

Operaciones implementadas desde el cliente:

- `POST /api/Auth/login`
- `GET /api/Productos`
- `POST /api/Productos`
- `PUT /api/Productos/{id}`
- `DELETE /api/Productos/{id}`

El token JWT se guarda mediante `TokenService`, no se guarda la contrasena, y `AuthHeaderHandler` agrega automaticamente:

```text
Authorization: Bearer TOKEN
```

### Ejecutar o compilar el cliente

En este entorno, el cliente se dejo como proyecto .NET 10 compilable y con archivos XAML incluidos como vistas de la practica. Para validar compilacion:

```bash
dotnet restore EcoSystem.Client/EcoSystem.Client.csproj
dotnet build EcoSystem.Client/EcoSystem.Client.csproj --no-restore
```

Si se abre en Visual Studio con workloads completos de .NET MAUI, se puede migrar el `.csproj` a `UseMaui=true` y targets MAUI del equipo. En esta sesion NuGet externo estaba bloqueado por el proxy local `127.0.0.1:9`, por lo que se evito depender de paquetes descargables.

### Evidencia sugerida para las firmas

1. Estructura del proyecto `EcoSystem.Client` con carpetas `Models`, `Views`, `ViewModels`, `Services`.
2. Compilacion del cliente con `0 Errores`.
3. Swagger de Render abierto.
4. Login exitoso en `POST /api/Auth/login`.
5. Token guardado/usado por el cliente o evidencia del header `Authorization`.
6. Lista de productos cargada desde `GET /api/Productos`.
7. Producto creado desde usuario Admin.
8. Producto editado desde usuario Admin.
9. Confirmacion y eliminacion de producto desde usuario Admin.
10. Intento con usuario normal recibiendo `403` en operacion restringida.
11. Cierre de sesion y regreso a login.

### Persistencia remota

La API sigue preparada para usar `ConnectionStrings__DefaultConnection` cuando exista una base SQL Server remota. Actualmente Render conserva la base en memoria para demostracion inmediata. Para persistencia real, configura una base SQL Server compatible, agrega la variable `ConnectionStrings__DefaultConnection` en Render y quita `RENDER=true` o ajusta la condicion para usar SQL remoto.
