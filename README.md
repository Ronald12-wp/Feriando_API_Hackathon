# Feriando API

API REST de **Feriando**, una plataforma para publicar productos e intercambiarlos con otras personas. 
La API permite que la aplicación cliente consulte el catálogo, registre e identifique usuarios, administre publicaciones y coordine trueques
También ofrece valoraciones, notificaciones y conversaciones en tiempo real.

Este repositorio contiene el backend. La interfaz de usuario es un cliente separado que consume estas rutas HTTP y el hub de SignalR.

## Tecnologías

- C# y ASP.NET Core 10 (`net10.0`).
- SQL Server y Entity Framework Core 10 (proveedor SQL Server y migraciones).
- Autenticación con tokens JWT y contraseñas protegidas con BCrypt.
- SignalR para la comunicación en tiempo real del chat.
- Swagger / OpenAPI para explorar y probar la API en desarrollo.

## Estructura del repositorio

| Ruta | Responsabilidad |
| --- | --- |
| `backend/` | Punto de entrada de la API. Recibe peticiones HTTP en los controladores, configura autenticación y servicios, publica Swagger y el hub de SignalR, y sirve imágenes. |
| `Feriando.Business/` | Servicios que ejecutan las operaciones y reglas de negocio, por ejemplo, autenticación, productos y trueques. |
| `Feriando.Core/` | Entidades que representan los datos del dominio y DTOs usados para recibir o devolver información en la API. |
| `Feriando.DataAccess/` | Contexto de Entity Framework Core, que conecta el modelo con SQL Server, y migraciones que describen los cambios del esquema. |
| `database/` | Scripts SQL de referencia y diagnóstico del esquema. |
| `Feriando.sln` | Solución .NET que agrupa los proyectos. |

En una petición típica, el controlador de `backend/` valida y recibe la solicitud, un servicio de `Feriando.Business/` aplica la lógica y `Feriando.DataAccess/` consulta o actualiza SQL Server usando las entidades de `Feriando.Core/`.

## Requisitos

- .NET 10 SDK.
- Una instancia de SQL Server (por ejemplo, SQL Server Express) instalada y en ejecución, accesible desde el equipo.
- Herramienta de Entity Framework Core CLI (`dotnet-ef`) para aplicar migraciones.

La base de datos debe aceptar la conexión configurada. Con autenticación integrada de Windows, el usuario que ejecuta los comandos también necesita permiso para crear y modificar el esquema.

Comprueba la instalación del SDK con `dotnet --version`. Para instalar la herramienta de EF Core si aún no está disponible:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.11
```

## Configuración

1. Clona el repositorio y abre PowerShell en la carpeta raíz, donde está `Feriando.sln`.
2. Configura la conexión y los valores JWT en la sesión de PowerShell antes de ejecutar la API o las migraciones. Por ejemplo:

   ```powershell
   $env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=ElTruequeDB;Trusted_Connection=True;TrustServerCertificate=True;"
   $env:Jwt__Key = "REEMPLAZA_CON_UN_SECRETO_ALEATORIO_DE_AL_MENOS_32_CARACTERES"
   $env:Jwt__Issuer = "FeriandoApi"
   $env:Jwt__Audience = "FeriandoApp"
   ```

   Cambia el servidor según tu instalación de SQL Server. `Trusted_Connection=True` usa la identidad de Windows; si usas autenticación SQL, ajusta la cadena con el usuario y contraseña de tu instancia. `TrustServerCertificate=True` es útil en desarrollo local; revisa la configuración de certificados para un despliegue.

   Reemplaza el valor de `Jwt__Key` por un secreto aleatorio privado de al menos 32 caracteres. No publiques secretos ni cadenas con contraseñas en Git. `Jwt__Issuer` y `Jwt__Audience` identifican quién emite el token y para quién es válido; deben coincidir con la configuración del backend y del cliente que lo valida.

   ASP.NET Core convierte `__` en `:` al leer variables de entorno: por ejemplo, `Jwt__Key` configura `Jwt:Key`. Estas variables solo duran durante la sesión actual de PowerShell. Si abres otra terminal, vuelve a definirlas.

## Instalación y ejecución

Con SQL Server iniciado, las variables configuradas y la terminal en la raíz del repositorio, ejecuta los comandos en este orden:

```powershell
dotnet restore Feriando.sln
dotnet ef database update --project Feriando.DataAccess --startup-project backend
dotnet run --project backend
```

1. `dotnet restore` descarga las dependencias declaradas por los proyectos de la solución.
2. `dotnet ef database update` ejecuta las migraciones pendientes y crea o actualiza las tablas de `ElTruequeDB`. `--project` señala dónde están el contexto y las migraciones; `--startup-project` señala la API, que proporciona la configuración de conexión.
3. `dotnet run` compila e inicia la API. Mantén esta terminal abierta mientras la uses. La consola indica las direcciones locales HTTP/HTTPS en las que escucha.

En entorno de desarrollo, abre `/swagger` en una de esas direcciones (por ejemplo, `https://localhost:puerto/swagger`) para ver y probar los endpoints. Si el navegador advierte sobre el certificado HTTPS local, instala o confía el certificado de desarrollo de .NET según tu sistema.

Para detener el servidor, presiona `Ctrl+C` en la terminal donde se está ejecutando.

## Módulos y rutas principales

Los controladores se encuentran en `backend/Controllers/`. Las rutas principales incluyen:

- `api/auth`: registro e inicio de sesión (`registro`, `login`).
- `api/productos`: consulta y gestión de productos; las operaciones de escritura requieren autenticación.
- `api/usuarios`: operaciones de usuarios.
- `api/trueques`: solicitudes y gestión de intercambios.
- `api/valoraciones`: valoraciones.
- `api/notificaciones`: notificaciones.
- `api/catalogos`: datos de catálogo.
- `api/chat`: funciones del chat; el hub de SignalR está publicado en `/chatHub`.

Las rutas, parámetros, modelos y requisitos de autenticación completos se pueden consultar en Swagger durante el desarrollo. Para probar una ruta protegida, primero inicia sesión en `POST /api/auth/login`, copia el token devuelto y úsalo como `Authorization: Bearer <token>` en la petición. Swagger permite configurar este token con el botón **Authorize**.

## Base de datos

El modelo se configura en `Feriando.DataAccess/Data/ElTruequeDbContext.cs`; allí se declaran las tablas y algunas relaciones y restricciones. Las migraciones versionadas están en `Feriando.DataAccess/Migrations/` y registran cambios del modelo a lo largo del tiempo. También se incluyen `database/ElTruequeDB.sql` y `database/DiagnosticoEsquema.sql` como scripts de referencia y diagnóstico. Para preparar una base compatible con la versión actual del código, aplica las migraciones con el comando indicado arriba.

## Archivos cargados

Las imágenes de productos se guardan y sirven desde `backend/wwwroot/uploads/productos/`. En despliegues, configura almacenamiento persistente y copias de seguridad apropiadas para estos archivos; los archivos locales de una ejecución de desarrollo no sustituyen una estrategia de almacenamiento para producción.
