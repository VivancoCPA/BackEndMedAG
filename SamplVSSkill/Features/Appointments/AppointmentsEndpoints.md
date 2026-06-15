# Documentación de Endpoints de Citas (Slice: Appointments)

Esta documentación detalla los endpoints de gestión de consultas y citas médicas para el usuario autenticado.

---

## 📌 Tabla de Contenidos

- [Listar Todas las Citas (`GET /api/appointments`)](#listar-todas-las-citas-get-apiappointments)
- [Listar Citas Paginadas (`GET /api/appointments/paged`)](#listar-citas-paginadas-get-apiappointmentspaged)
- [Crear una Cita (`POST /api/appointments`)](#crear-una-cita-post-apiappointments)
- [Actualizar una Cita (`PUT /api/appointments/{id}`)](#actualizar-una-cita-put-apiappointmentsid)

---

## Listar Todas las Citas (`GET /api/appointments`)

*   **Ruta:** `GET /api/appointments`
*   **Nombre de Acción:** `ListAppointments`
*   **Autorización:** Requerido (`.RequireAuthorization()`). Retorna las citas pertenecientes al usuario autenticado.
*   **Parámetros de Consulta (Query Params):**
    *   `statusId` (string, opcional, por defecto `null`): Filtra por el identificador de estado de la cita (ej. `"PENDIENTE"`, `"CONFIRMADA"`).
    *   `date` (DateTime, opcional, por defecto `null`): Filtra las citas de un día específico (compara solo la parte de la fecha).

#### Respuesta Exitosa (`200 OK`)
Devuelve un listado `IEnumerable<ListAppointmentsResponse>` con los detalles de las citas del usuario:

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "d748f65e-2b1a-42c3-98fe-d27e7fcd61a2",
    "appointmentDate": "2026-06-20T14:30:00Z",
    "centerId": "a82fca21-5a41-477f-8c38-8c10be141123",
    "doctorId": "8c38a123-5a41-477f-a82f-8c10be1411f5",
    "specialtieId": 1,
    "insurerId": "b3fc2c96-5717-4562-3fa8-5f643f66afa6",
    "description": "Consulta de control rutinario anual",
    "statusId": "CONFIRMADA",
    "createdAt": "2026-06-15T10:00:00Z",
    "specialtyName": "Cardiología",
    "doctorName": "Alejandro Vivanco",
    "doctorPhotoUrl": "/uploads/profiles/doctor_avatar.jpg",
    "centerName": "Centro Médico Las Lomas",
    "centerAddress": "Av. Principal 123, Lima",
    "centerLatitude": -12.046374,
    "centerLongitude": -77.042793
  }
]
```

#### Otras Respuestas
*   **`401 Unauthorized`**: Si el usuario no ha iniciado sesión o el token no es válido.

---

## Listar Citas Paginadas (`GET /api/appointments/paged`)

*   **Ruta:** `GET /api/appointments/paged`
*   **Nombre de Acción:** `PagedAppointments`
*   **Autorización:** Requerido (`.RequireAuthorization()`). Retorna las citas paginadas correspondientes al usuario autenticado.
*   **Parámetros de Consulta (Query Params):**
    *   `page` (int, opcional, por defecto `1`): Número de página a consultar.
    *   `pageSize` (int, opcional, por defecto `10`): Cantidad máxima de registros por página (Min: 1, Max: 100).
    *   `statusId` (string, opcional, por defecto `null`): Filtra por el identificador del estado de la cita.
    *   `date` (DateTime, opcional, por defecto `null`): Filtra por una fecha específica (compara solo el día).

#### Respuesta Exitosa (`200 OK`)
Devuelve un objeto `PaginatedResult<PagedAppointmentItem>` con los metadatos de paginación y la lista de citas:

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "d748f65e-2b1a-42c3-98fe-d27e7fcd61a2",
      "appointmentDate": "2026-06-20T14:30:00Z",
      "centerId": "a82fca21-5a41-477f-8c38-8c10be141123",
      "doctorId": "8c38a123-5a41-477f-a82f-8c10be1411f5",
      "specialtieId": 1,
      "insurerId": "b3fc2c96-5717-4562-3fa8-5f643f66afa6",
      "description": "Consulta de control rutinario anual",
      "statusId": "CONFIRMADA",
      "createdAt": "2026-06-15T10:00:00Z",
      "specialtyName": "Cardiología",
      "doctorName": "Alejandro Vivanco",
      "doctorPhotoUrl": "/uploads/profiles/doctor_avatar.jpg",
      "centerName": "Centro Médico Las Lomas",
      "centerAddress": "Av. Principal 123, Lima",
      "centerLatitude": -12.046374,
      "centerLongitude": -77.042793
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

#### Otras Respuestas
*   **`401 Unauthorized`**: Si el usuario no ha iniciado sesión o el token no es válido.

---

## Crear una Cita (`POST /api/appointments`)

*   **Ruta:** `POST /api/appointments`
*   **Nombre de Acción:** `CreateAppointment`
*   **Autorización:** Requerido (`.RequireAuthorization()`). Crea una cita asignada al usuario autenticado.
*   **Cuerpo de la Solicitud (Body):**
    *   `centerId` (Guid?, opcional): Identificador del centro médico.
    *   `doctorId` (Guid?, opcional): Identificador del médico.
    *   `specialtieId` (int?, opcional): Identificador de la especialidad.
    *   `insurerId` (Guid?, opcional): Identificador de la aseguradora.
    *   `description` (string, requerido): Descripción o motivo de la consulta.
    *   `appointmentDate` (DateTime?, opcional): Fecha y hora programadas para la cita.
    *   `statusId` (string, opcional, por defecto `"PENDIENTE"`): Estado inicial de la cita.

#### Reglas de Validación
*   `description` no puede estar vacío y tiene un máximo de 1000 caracteres.
*   `statusId` debe ser uno de los siguientes valores permitidos (sin importar mayúsculas/minúsculas): `"PENDIENTE"`, `"CONFIRMADA"`, `"INASISTENCIA"`, `"CANCELADA"`, `"REPROGRAMADA"`, `"ENCONSULTA"`, `"FINALIZADA"`.
*   Si se especifican `centerId`, `doctorId`, `specialtieId` o `insurerId`, se valida que existan en sus respectivas tablas.

#### Respuesta Exitosa (`201 Created`)
Devuelve un objeto `CreateAppointmentResponse` con los detalles de la cita creada:

```json
{
  "id": "018fcc12-3456-789a-bcde-f0123456789a",
  "userId": "d748f65e-2b1a-42c3-98fe-d27e7fcd61a2",
  "centerId": "a82fca21-5a41-477f-8c38-8c10be141123",
  "doctorId": "8c38a123-5a41-477f-a82f-8c10be1411f5",
  "specialtieId": 1,
  "insurerId": "b3fc2c96-5717-4562-3fa8-5f643f66afa6",
  "description": "Consulta por dolor lumbar",
  "appointmentDate": "2026-06-20T14:30:00Z",
  "statusId": "PENDIENTE",
  "createdAt": "2026-06-15T10:12:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Si los datos de validación no se cumplen o si las entidades relacionadas no existen.
*   **`401 Unauthorized`**: Si el usuario no está autenticado.

---

## Actualizar una Cita (`PUT /api/appointments/{id}`)

*   **Ruta:** `PUT /api/appointments/{id}`
*   **Nombre de Acción:** `UpdateAppointment`
*   **Autorización:** Requerido (`.RequireAuthorization()`).
    *   Un usuario estándar solo puede actualizar sus propias citas.
    *   Un usuario con rol `Admin` o `SuperAdmin` puede actualizar cualquier cita.
*   **Parámetros de Ruta (Path Params):**
    *   `id` (Guid, requerido): Identificador único de la cita a actualizar.
*   **Cuerpo de la Solicitud (Body):**
    *   `centerId` (Guid?, opcional): Identificador del centro médico.
    *   `doctorId` (Guid?, opcional): Identificador del médico.
    *   `specialtieId` (int?, opcional): Identificador de la especialidad.
    *   `insurerId` (Guid?, opcional): Identificador de la aseguradora.
    *   `description` (string, requerido): Descripción o motivo de la consulta.
    *   `appointmentDate` (DateTime?, opcional): Fecha y hora programadas para la cita.
    *   `statusId` (string, requerido): Estado de la cita.

#### Reglas de Validación
*   `description` no puede estar vacío y tiene un máximo de 1000 caracteres.
*   `statusId` no puede estar vacío y debe ser uno de los siguientes valores permitidos (sin importar mayúsculas/minúsculas): `"PENDIENTE"`, `"CONFIRMADA"`, `"INASISTENCIA"`, `"CANCELADA"`, `"REPROGRAMADA"`, `"ENCONSULTA"`, `"FINALIZADA"`.
*   Si se especifican `centerId`, `doctorId`, `specialtieId` o `insurerId`, se valida que existan en sus respectivas tablas.

#### Respuesta Exitosa (`200 OK`)
Devuelve un objeto `UpdateAppointmentResponse` con los detalles de la cita modificada:

```json
{
  "id": "018fcc12-3456-789a-bcde-f0123456789a",
  "userId": "d748f65e-2b1a-42c3-98fe-d27e7fcd61a2",
  "centerId": "a82fca21-5a41-477f-8c38-8c10be141123",
  "doctorId": "8c38a123-5a41-477f-a82f-8c10be1411f5",
  "specialtieId": 1,
  "insurerId": "b3fc2c96-5717-4562-3fa8-5f643f66afa6",
  "description": "Consulta por dolor lumbar modificada",
  "appointmentDate": "2026-06-21T09:00:00Z",
  "statusId": "CONFIRMADA",
  "createdAt": "2026-06-15T10:12:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Si los datos de validación no se cumplen o si las entidades relacionadas no existen.
*   **`401 Unauthorized`**: Si el usuario no está autenticado.
*   **`403 Forbidden`**: Si un usuario común intenta actualizar una cita que no le pertenece.
*   **`404 NotFound`**: Si la cita con el ID proporcionado no existe en la base de datos.
