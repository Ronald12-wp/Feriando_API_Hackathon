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

## Implementación Despliegue con Azure 
Como plataforma se utiliza Microsoft Azure, con Azure Resource Group, Azure App Service y Azure SQL Database. Si el proyecto utiliza otro motor de base de datos o un backend diferente, se deben adaptar los pasos.

### 2. Tecnologías y componentes

| Componente | Tecnología propuesta | Función |
| --- | --- | --- |
| Nube | Microsoft Azure | Aloja y administra los recursos. |
| Organización de recursos | Azure Resource Group | Agrupa recursos relacionados y facilita su administración. |
| Backend y API | Azure App Service | Publica la API por HTTPS sin administrar directamente el servidor. |
| Base de datos | Azure SQL Database | Almacena la información relacional de Feriando. |
| Aplicación móvil | APK de Android | Paquete instalable para dispositivos Android. |
| Página de descarga | HTML, CSS  o un sitio existente | Presenta información y enlaza la descarga del APK. |
| Archivo descargable | Azure Blob Storage o alojamiento web | Guarda y entrega el APK. |
| Código fuente | Git y GitHub | Permite controlar versiones y colaborar. |
| Configuración sensible | App Service settings o Azure Key Vault | Mantiene secretos fuera del código fuente. |
| Pruebas | Swagger/OpenAPI, Postman o pruebas automatizadas | Verifica rutas, respuestas y comportamiento de la API. |

### 3. Preparación previa

Antes de crear recursos:

- Confirma el acceso a una suscripción de Azure y revisa presupuesto, cuotas y disponibilidad regional.
- Identifica el framework y la versión de la API, por ejemplo ASP.NET Core o Node.js.
- Confirma el motor de base de datos actual, su esquema, el usuario de conexión y el método de migración.
- Verifica que la aplicación Android se ejecute localmente y conoce su método de compilación, por ejemplo Flutter/Gradle o Android nativo.
- Actualiza el repositorio Git y excluye archivos de compilación, secretos y configuraciones locales.
- Define el dominio, el nombre del servicio, la región y la política de respaldos.
- Prueba la API localmente y documenta sus rutas principales.

Los nombres de los menús pueden variar cuando se actualiza Azure Portal. Elige la región según la latencia, la disponibilidad de los servicios, los requisitos de residencia de datos y el costo.

### 4. Crear el grupo de recursos

El grupo de recursos es el contenedor lógico de los servicios que pertenecen al despliegue de Feriando.

1. Entra al portal de Azure con tu cuenta personal o estudiantil.
2. Busca **Resource groups** o **Grupos de recursos**.
3. Selecciona **Create** o **Crear**.
4. Elige la suscripción correspondiente.
5. Asigna un nombre, por ejemplo `rg-feriando-prod`.
6. Selecciona la región prevista para la mayoría de los recursos.
7. Revisa los datos y selecciona **Review + create** y después **Create**.
8. Abre el grupo creado y úsalo como destino al crear App Service y la base de datos.

Convenciones de nombres sugeridas:

- `rg-feriando-prod`: grupo de recursos de producción.
- `app-feriando-api-prod`: nombre lógico de la aplicación o API. El nombre público de App Service debe ser único globalmente.
- `sql-feriando-prod`: servidor lógico de Azure SQL, si se utiliza Azure SQL Database.
- `sqldb-feriando-prod`: base de datos.

### 5. Crear y desplegar la API en Azure App Service

1. En Azure Portal, selecciona **Create a resource** y busca **Web App** o **App Service**.
2. Selecciona la suscripción y el grupo `rg-feriando-prod`.
3. Define un nombre único para la aplicación.
4. Selecciona el sistema operativo y el runtime que correspondan al backend. Confirma el framework de la API antes de elegir el runtime.
5. Elige un plan de App Service adecuado para pruebas o producción, considerando costo, escalado y disponibilidad.
6. Revisa y crea el recurso.
7. En **Overview**, copia la URL HTTPS asignada, por ejemplo `https://nombre-api.azurewebsites.net`.

Antes de publicar, comprueba lo siguiente:

- La API compila en modo Release y arranca sin depender de rutas locales del equipo de desarrollo.
- Las variables de entorno y cadenas de conexión se leen desde una configuración externa.
- Los endpoints necesarios están documentados y manejan errores.
- CORS está configurado solo para los orígenes web que lo necesitan. Una aplicación Android nativa no se configura igual que un navegador.
- Los endpoints de diagnóstico no exponen contraseñas, cadenas de conexión ni información personal.

### 6. Método de publicación

La API se puede desplegar desde GitHub Actions, Visual Studio, Azure DevOps o un paquete ZIP. Automatizar el despliegue desde el repositorio ayuda a repetir el proceso en cada versión.

Flujo recomendado:

1. Sube el código probado a una rama de despliegue en GitHub.
2. Configura la integración de despliegue de App Service con el repositorio.
3. Define los pasos para restaurar dependencias, compilar y publicar.
4. Guarda los secretos en la configuración de Azure o en Azure Key Vault; nunca los agregues al repositorio.
5. Ejecuta el despliegue y revisa el resultado de la acción o del registro.
6. Abre la URL HTTPS y comprueba un endpoint de salud o una ruta pública de prueba.

### 7. Validar la API

- Abre Swagger/OpenAPI si está habilitado y protegido adecuadamente.
- Usa Postman o `curl` para probar una ruta GET que no modifique datos.
- En un entorno de pruebas, comprueba autenticación, validaciones, respuestas de error y operaciones que escriban datos.
- Si la API no inicia, revisa **App Service → Log stream / Logs**.

### 8. Crear y configurar la base de datos

Esta guía supone que Feriando utiliza una base de datos relacional compatible con Azure SQL Database. Si el proyecto utiliza MySQL, PostgreSQL, Firebase u otro servicio, elige el servicio equivalente y adapta el proveedor de conexión.

#### 8.1 Crear Azure SQL Database

1. En Azure Portal, crea un recurso **SQL Database**.
2. Selecciona o crea un servidor lógico SQL y asigna una región compatible con el diseño.
3. Asigna un nombre a la base de datos, por ejemplo `sqldb-feriando-prod`.
4. Elige un nivel de cómputo y almacenamiento adecuado al volumen de trabajo y al presupuesto.
5. Configura un método de autenticación seguro y guarda las credenciales en un gestor de secretos.
6. Crea el recurso y abre su página de configuración.

#### 8.2 Configurar red y permisos

- Configura las reglas de red para que la API pueda acceder a la base de datos.
- Evita habilitar el acceso público para cualquier IP. Si permites acceso público temporal para pruebas, limítalo a IP conocidas y retíralo después.
- Para producción, considera una red privada, Private Endpoint o una estrategia de red compatible con el plan de App Service.
- Concede a la identidad o al usuario de la API solo los permisos necesarios. Evita utilizar una cuenta administradora para las operaciones normales.

#### 8.3 Preparar el esquema y los datos

1. Crea tablas, relaciones, índices y restricciones mediante scripts versionados o migraciones del framework.
2. Si ya existe una base de datos, haz un respaldo antes de modificar el esquema.
3. No copies datos personales reales a pruebas sin autorización y medidas de protección.
4. Valida que existan las tablas y los datos mínimos requeridos antes de conectar la aplicación.

#### 8.4 Conectar la API con la base de datos

Configura la cadena de conexión en **App Service → Settings → Environment variables** (o **Configuration**, según la interfaz). No escribas contraseñas reales en el código ni en Git.

Ejemplo conceptual para una API .NET que utiliza Azure SQL. Reemplaza los valores y el nombre de la variable según el proyecto:

```text
ConnectionStrings__DefaultConnection = "Server=tcp:<servidor>.database.windows.net,1433;Initial Catalog=<base>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=..."
```

El método de autenticación debe coincidir con la configuración real. Se recomienda una identidad administrada cuando el proveedor y el diseño lo permitan. Si se utiliza usuario y contraseña, guárdalos en una configuración segura y rótalos periódicamente.

#### 8.5 Verificar la conectividad

1. Confirma que el proveedor de base de datos y la versión del controlador están incluidos en la API.
2. Verifica firewall, DNS, puerto y reglas de red.
3. Confirma que la API recibe la cadena de conexión desde la configuración del entorno.
4. Ejecuta las migraciones de forma controlada, idealmente como una tarea de despliegue separada.
5. Prueba una operación de lectura y otra de escritura con datos de prueba.
6. Consulta los registros ante errores de autenticación, timeout o esquema.

### 9. Generar y validar el APK de Android

El procedimiento depende de si Feriando está desarrollado en Flutter, Android nativo u otro framework. Los siguientes pasos son generales; el comando de Flutter aplica solo si ese es el framework del proyecto.

#### 9.1 Preparación

- Actualiza la URL base de la API para utilizar el endpoint HTTPS publicado.
- Elimina referencias a `localhost`, emuladores o direcciones de desarrollo.
- Comprueba permisos de Android, versión mínima soportada, nombre del paquete e iconos.
- Prueba inicio de sesión, consultas, formularios, carga de imágenes y manejo de errores de red.
- Configura la firma de release y protege el almacén de claves y sus contraseñas.

#### 9.2 Compilar con Flutter

```powershell
flutter pub get
flutter analyze
flutter build apk --release
```
En un proyecto Flutter estándar, el APK de release suele generarse en:
```text
build/app/outputs/flutter-apk/app-release.apk
```

Confirma la ruta que informa la compilación. En Android nativo, genera el APK desde Android Studio con **Build → Generate Signed Bundle / APK** y sigue el asistente de firma. Para distribuir por una tienda puede convenir un Android App Bundle (AAB); una web de descarga directa normalmente distribuye un APK.

#### 9.3 Probar antes de publicar

1. Instala el APK en un dispositivo Android de prueba.
2. Verifica que la aplicación accede a la API mediante HTTPS.
3. Prueba las funciones principales y distintos tamaños de pantalla.
4. Comprueba que el APK corresponde a la versión final y que está firmado correctamente.
5. Guarda una copia del archivo publicado y registra la versión, la fecha y la persona responsable.

### 10. Publicar el APK en una página web

La web puede ser una página sencilla de presentación con un enlace de descarga. El APK puede alojarse en el propio hosting web o en Azure Blob Storage. Para una solución más fácil de mantener, conviene separar el sitio informativo del almacenamiento del instalador.



