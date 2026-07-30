# El Trueque — API (ASP.NET Core / C#)

API REST para la aplicación "El Trueque", conectada a la base de datos
`ElTruequeDB` en SQL Server (script `database_el_trueque.sql`).

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local, Express o Docker) con `ElTruequeDB` ya creada
- Visual Studio Code + extensión **C# Dev Kit**

## Configuración

1. Copia `appsettings.json` y ajusta la cadena de conexión con tu usuario y
   contraseña reales de SQL Server:

   ```json
   "DefaultConnection": "Server=localhost;Database=ElTruequeDB;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;"
   ```

2. Cambia `Jwt:Key` por una llave secreta propia (mínimo 32 caracteres).

## Ejecutar en VS Code

```bash
cd backend
dotnet restore
dotnet run
```

La API queda disponible en `http://localhost:5080` y la documentación
interactiva (Swagger) en `http://localhost:5080/swagger`.

## Si prefieres que EF Core genere el esquema (en vez del script .sql)

El script `database_el_trueque.sql` ya crea todas las tablas. Si en algún
momento prefieres que Entity Framework las genere a partir del código C#,
usa:

```bash
dotnet ef migrations add InicialElTrueque
dotnet ef database update
```

(Requiere la herramienta `dotnet-ef`: `dotnet tool install --global dotnet-ef`.)

## Endpoints principales

| Método | Ruta                              | Descripción                                  | Auth |
|--------|-----------------------------------|-----------------------------------------------|------|
| POST   | /api/auth/registro                | Crea una cuenta de usuaria                    | No   |
| POST   | /api/auth/login                   | Inicia sesión y devuelve el JWT               | No   |
| GET    | /api/catalogos/comunidades        | Lista comunidades (para combos)               | No   |
| GET    | /api/catalogos/categorias         | Lista categorías de productos                 | No   |
| GET    | /api/productos                    | Catálogo público de productos (con filtros)   | No   |
| GET    | /api/productos/{id}               | Detalle de un producto                        | No   |
| POST   | /api/productos                    | Publica un nuevo producto                     | Sí   |
| PUT    | /api/productos/{id}                | Edita un producto propio                      | Sí   |
| DELETE | /api/productos/{id}                | Elimina un producto propio                    | Sí   |
| GET    | /api/productos/mios                | Productos publicados por la usuaria autenticada | Sí |
| POST   | /api/trueques                      | Solicita un trueque o compra                  | Sí   |
| PUT    | /api/trueques/{id}/responder       | Acepta o rechaza una solicitud                | Sí   |
| GET    | /api/trueques/mios                 | Solicitudes enviadas y recibidas              | Sí   |
| POST   | /api/valoraciones                  | Califica a la otra persona tras el intercambio | Sí  |
| GET    | /api/notificaciones                | Notificaciones de la usuaria                  | Sí   |

Todas las rutas marcadas con **Auth: Sí** requieren el header:
`Authorization: Bearer {token}` obtenido en `/api/auth/login`.
