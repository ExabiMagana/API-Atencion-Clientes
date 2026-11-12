# API de atención al cliente en tiempo real

Las pruebas se realizan mediante archivos HTTP y 
una aplicación de consola que actúa como cliente de SignalR.

## Tecnologías

- .NET 10 y ASP.NET Core Web API.
- Entity Framework Core 10.
- SQL Server.
- ASP.NET Core SignalR.
- OpenAPI.
- Cliente de consola .NET para demostrar las notificaciones.

## Importante

El proyecto AtencionClientes.PruebasSignalR contiene el cliente de consola.

## Configuración de la base de datos

En appsettings.json, se configura ConnectionStrings:DefaultConnection.

Ejemplo para SQL Server Express con autenticación de Windows:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AtencionClientesDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Instalación

1. Clona el repositorio.
2. Abre la solución en Visual Studio.
3. Restaura los paquetes NuGet.
4. Configura la cadena de conexión.
5. Establece AtencionClientesAPI como proyecto de inicio.
6. Abre la Consola del Administrador de paquetes.
7. Selecciona AtencionClientesAPI como proyecto predeterminado.
8. Ejecuta:

```powershell
Update-Database
```

Las migraciones ya incluidas crearán el esquema.

## Ejecución de la API

Ejecuta AtencionClientesAPI desde Visual Studio.

Utiliza la URL HTTPS indicada por el perfil de ejecución.
En el entorno original se utiliza:

```text
https://localhost:7222
```

La dirección principal puede devolver 404 porque no existe una página de inicio.

Durante el desarrollo, el documento OpenAPI está disponible en:

```text
/openapi/v1.json
```

## Endpoints de clientes

| Método | Ruta | Descripción |
|---|---|---|
| POST | /api/clientes | Registrar cliente |
| GET | /api/clientes | Consultar clientes |
| GET | /api/clientes/{id} | Consultar cliente |
| PUT | /api/clientes/{id} | Actualizar cliente |

Ejemplo de registro:

```json
{
  "nombre": "Ana López",
  "telefono": "7000-1001",
  "correo": "ana@example.com"
}
```

## Endpoints de empleados

| Método | Ruta | Descripción |
|---|---|---|
| POST | /api/empleados | Registrar empleado |
| GET | /api/empleados/{id} | Consultar empleado |
| GET | /api/empleados/disponibles | Consultar empleados disponibles |

Ejemplo de registro:

```json
{
  "nombre": "Carlos Hernández"
}
```

Un empleado está disponible cuando está activo y no tiene un turno en atención.

## Endpoints de turnos

| Método | Ruta | Descripción |
|---|---|---|
| POST | /api/turnos | Crear turno |
| GET | /api/turnos/pendientes | Consultar fila pendiente |
| GET | /api/turnos/{id} | Consultar turno |
| GET | /api/turnos/{id}/posicion | Consultar posición |
| PUT | /api/turnos/{id}/asignacion | Asignar empleado e iniciar atención |
| PATCH | /api/turnos/{id}/estado | Cambiar estado |
| POST | /api/turnos/{id}/finalizar | Finalizar atención |

Crear turno:

```json
{
  "clienteId": 1
}
```

Asignar empleado:

```json
{
  "empleadoId": 1
}
```

Cancelar turno:

```json
{
  "estado": "Cancelado"
}
```

Marcar como atendido mediante el endpoint de estado:

```json
{
  "estado": "Atendido"
}
```


## SignalR

Dirección del Hub:

```text
https://localhost:7222/hubs/turnos
```

Se adapta el puerto a la configuración de la API.

Métodos disponibles:

| Método | Parámetros | Función |
|---|---|---|
| UnirseEmpleados | Ninguno | Suscribirse al grupo de empleados |
| UnirseCliente | clienteId entero | Suscribirse a un cliente existente |

Eventos:

| Evento | Destinatarios | Contenido |
|---|---|---|
| NuevoTurno | Empleados | Datos del turno creado |
| TurnoActualizado | Cliente correspondiente y empleados según la operación | Datos actuales del turno |
| PosicionActualizada | Clientes con turnos pendientes | turnoId, posicion y generadaEn |


## Ejecutar el cliente de consola

Desde la carpeta de la solución:

```powershell
dotnet run --project .\AtencionClientes.PruebasSignalR
```

Introduce:

1. Modo empleados o cliente.
2. Identificador del cliente, si corresponde.

Se pueden abrir varias terminales para representar distintos clientes.
El cliente intenta reconectarse automáticamente y vuelve a suscribirse
a su grupo al recuperar la conexión.

## Pruebas

AtencionClientesAPI.http contiene las solicitudes para demostrar:

- Registro, consulta y actualización de clientes.
- Registro y disponibilidad de empleados.
- Creación y consulta de turnos.
- Posiciones en la fila.
- Asignación e inicio de atención.
- Finalización y cancelación.
- Validaciones y conflictos.

Ejecuta las solicitudes individualmente, en el orden documentado.
Actualiza las variables con los identificadores recibidos.

Los escenarios de SignalR a verificar son:

1. Crear un turno y observar NuevoTurno en empleados.
2. Iniciar atención y observar el aviso en el cliente correspondiente.
3. Finalizar atención y observar las posiciones de quienes esperan.
4. Cancelar un turno y observar los avisos y posiciones actualizadas.

## Códigos HTTP

- 200: consulta u operación completada con respuesta.
- 201: registro creado.
- 204: actualización completada sin cuerpo.
- 400: solicitud inválida.
- 404: recurso inexistente.
- 409: conflicto con las reglas de negocio o modificación simultánea.