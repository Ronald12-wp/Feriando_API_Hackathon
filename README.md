# Feriando API (ASP.NET Core 10 / C#)

API RESTful para la plataforma **Feriando** (El Trueque), conectada a la base de datos `ElTruequeDB` en SQL Server y estructurada bajo una **Arquitectura en Capas**.

---

##  Arquitectura del Proyecto

El proyecto está dividido en 4 capas principales para garantizar modularidad y mantenibilidad:

**`Feriando.Api`**: Capa de presentación (Controladores REST, Swagger, JWT y manejo de archivos en `wwwroot`).
* **`Feriando.Business`**: Capa de lógica de negocio (Servicios como `AuthService`, `ProductoService`, `UsuarioService`, etc.).
* **`Feriando.Core`**: Entidades de dominio, modelos y DTOs (`AuthDTOs`, `ProductoDTOs`, `TruequeDTOs`).
* **`Feriando.DataAccess`**: Persistencia de datos y contexto de Entity Framework Core (`ElTruequeDbContext`).

---

##  Requisitos

- **.NET 10 SDK** (o versión compatible)
- **SQL Server** (Local, Express o Docker) con la base de datos `ElTruequeDB`
- **Visual Studio 2026** o **VS Code** con C# Dev Kit

---

##  Configuración

1. Abre el archivo `backend/appsettings.json` (o dentro de `Feriando.Api`) y ajusta tu cadena de conexión local:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=DESKTOP-K6NB7RO\\SQLEXPRESS;Database=ElTruequeDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }

# Restaurar dependencias
dotnet restore

# Aplicar migraciones a la base de datos
dotnet ef database update --project Feriando.DataAccess --startup-project backend

# Iniciar la API
dotnet run --project backend