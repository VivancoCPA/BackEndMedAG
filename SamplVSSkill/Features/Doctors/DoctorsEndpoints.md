# Documentación de Endpoints de Médicos (Slice: Doctors)

Esta documentación detalla de forma exhaustiva únicamente los endpoints de **Gestión de Médicos** (`WithTags("Doctors")`) que pertenecen al Slice Vertical de Médicos (`Doctors`) de la aplicación.

---

## 📌 Tabla de Contenidos

- [Obtener Médico por ID (`GET /api/doctors/{id}`)](#obtener-médico-por-id-get-apidoctorsid)
- [Listar Todos los Médicos (`GET /api/doctors`)](#listar-todos-los-médicos-get-apidoctors)
- [Listar Médicos Paginados (`GET /api/doctors/paged`)](#listar-médicos-paginados-get-apidoctorspaged)
- [Lookup / Búsqueda Rápida de Médicos Activos (`GET /api/doctors/lookup`)](#lookup--búsqueda-rápida-de-médicos-activos-get-apidoctorslookup)
- [Obtener Resumen Estadístico de Médicos (`GET /api/doctors/summary`)](#obtener-resumen-estadístico-de-médicos-get-apidoctorssummary)
- [Crear Médico (`POST /api/doctors`)](#crear-médico-post-apidoctors)
- [Actualizar Médico (`PUT /api/doctors/{id}`)](#actualizar-médico-put-apidoctorsid)
- [Activar o Inactivar Médico (`PATCH /api/doctors/{id}/toggle-status`)](#activar-o-inactivar-médico-patch-apidoctorsidtoggle-status)
- [Eliminar Médico (`DELETE /api/doctors/{id}`)](#eliminar-médico-delete-apidoctorsid)

---

### Obtener Médico por ID (`GET /api/doctors/{id}`)

*   **Ruta:** `GET /api/doctors/{id}`
*   **Nombre de Acción:** `GetDoctor`
*   **Autorización:** Requerido (`.RequireAuthorization()`)
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único (UUID v7) del médico a consultar.

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `GetDoctorResponse` con información resumida del médico y su especialidad:
```json
{
  "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "name": "Juan Perez",
  "specialty": "Pediatría",
  "isVet": false
}
```

#### Otras Respuestas
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales válidas.
*   **`404 Not Found`**: No se encuentra un médico con el `id` provisto.

---

### Listar Todos los Médicos (`GET /api/doctors`)

*   **Ruta:** `GET /api/doctors`
*   **Nombre de Acción:** `ListDoctors`
*   **Autorización:** Pública.

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<ListDoctorsResponse>` ordenada por nombre y apellido con sus respectivas afiliaciones de centros médicos:
```json
[
  {
    "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
    "name": "Juan",
    "lastName": "Perez",
    "specialty": "Pediatría",
    "isVet": false,
    "centers": [
      {
        "id": "018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8",
        "name": "Clínica San Felipe"
      }
    ]
  }
]
```

---

### Listar Médicos Paginados (`GET /api/doctors/paged`)

*   **Ruta:** `GET /api/doctors/paged`
*   **Nombre de Acción:** `PagedDoctors`
*   **Autorización:** Pública.
*   **Parámetros de Consulta (Query Params):**
    *   `page` (int, Opcional, por defecto `1`): Número de página. Mayor o igual a 1.
    *   `pageSize` (int, Opcional, por defecto `10`): Tamaño de la página. Rango `1-100`.
    *   `search` (string, Opcional): Búsqueda insensible a mayúsculas/minúsculas sobre `name`, `last_name`, `email`, `register` y el nombre de la especialidad (`ILIKE`).
    *   `sortBy` (string, Opcional, por defecto `created_at`): Columnas permitidas: `name`, `lastname`, `specialtyname`, `email`, `isactive`, `created_at`, `updated_at`.
    *   `sortDesc` (bool, Opcional, por defecto `false`): Determina orden descendente (`true`) o ascendente (`false`).

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `PaginatedResult<PagedDoctorItem>` con las afiliaciones y metadatos detallados:
```json
{
  "items": [
    {
      "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
      "name": "Juan",
      "lastName": "Perez",
      "specialtyId": 1,
      "specialtyName": "Pediatría",
      "register": "CMP12345",
      "phone": "+51999888777",
      "email": "juan.perez@medical.com",
      "photoUrl": "/uploads/doctors/018fdf9c6a7b7b0b8d765fa42c9431f2.jpg",
      "isVet": false,
      "isActive": true,
      "createdAt": "2026-06-03T10:00:00Z",
      "updatedAt": "2026-06-03T10:00:00Z",
      "centers": [
        {
          "id": "018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8",
          "name": "Clínica San Felipe",
          "officeNumber": "Consultorio 302",
          "workSchedule": "Lun-Vie 9am-1pm"
        }
      ]
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

---

### Lookup / Búsqueda Rápida de Médicos Activos (`GET /api/doctors/lookup`)

*   **Ruta:** `GET /api/doctors/lookup`
*   **Nombre de Acción:** `LookupDoctors`
*   **Autorización:** Pública.

#### Respuesta Exitosa (`200 OK`)
Retorna una lista simplificada `IEnumerable<LookupItemGuid>` con el nombre completo concatenado (`name` + `last_name`), ordenada alfabéticamente:
```json
[
  {
    "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
    "name": "Juan Perez"
  }
]
```

---

### Obtener Resumen Estadístico de Médicos (`GET /api/doctors/summary`)

*   **Ruta:** `GET /api/doctors/summary`
*   **Nombre de Acción:** `SummaryDoctors`
*   **Autorización:** Pública.

#### Respuesta Exitosa (`200 OK`)
Retorna un conteo de alto rendimiento clasificado por estado de actividad:
```json
{
  "total": 150,
  "active": 140,
  "inactive": 10
}
```

---

### Crear Médico (`POST /api/doctors`)

*   **Ruta:** `POST /api/doctors`
*   **Nombre de Acción:** `CreateDoctor`
*   **Autorización:** Pública.

#### Cuerpo de la Solicitud (FormData / `multipart/form-data`)
La petición debe enviarse codificada como formulario (`multipart/form-data`) con los siguientes campos:

*   `name` (string, Requerido): Nombre del médico. Máx. 200 caracteres.
*   `lastName` (string, Requerido): Apellido del médico. Máx. 200 caracteres.
*   `specialtyId` (int, Opcional): ID de la especialidad asociada.
*   `register` (string, Opcional): Registro o colegiatura médica.
*   `phone` (string, Opcional): Teléfono de contacto.
*   `email` (string, Opcional): Correo electrónico (formato válido).
*   `photo` (file / IFormFile, Opcional): Archivo de foto de perfil del médico.
*   `isVet` (bool, Requerido): Determina si atiende veterinaria.
*   `centers` (string, Opcional): Cadena JSON que representa un arreglo de afiliaciones (`DoctorAffiliationRequest`). Ejemplo de valor: `[{"id":"018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8","officeNumber":"Consultorio 302","workSchedule":"Lun-Vie 9am-1pm"}]`.

> [!IMPORTANT]
> **Recomendación para el FrontEnd (React / JS / TS)**:
> Al invocar este endpoint utilizando un objeto `FormData`, **NO debes definir manualmente la cabecera `'Content-Type'`** en las cabeceras (`headers`) de tu cliente HTTP. Deja que el navegador configure automáticamente `multipart/form-data` con sus separadores dinámicos.

#### Comportamiento Transaccional y Atomicidad
1. Se abre una transacción de base de datos (`BeginTransactionAsync`).
2. Se registra el médico y sus afiliaciones en PostgreSQL.
3. Se escribe físicamente el archivo en `/uploads/doctors/` del servidor bajo un nombre de archivo seguro autogenerado (GUID).
4. Si la escritura física es exitosa, se hace el `Commit` de la base de datos.
5. **Rollback**: Si el guardado del archivo físico falla, se realiza un rollback automático de la base de datos y se elimina cualquier remanente del disco local, garantizando consistencia absoluta (sin registros huérfanos ni archivos basura).

#### Respuesta Exitosa (`201 Created`)
Retorna el médico registrado y su ruta relativa del archivo de imagen:
```json
{
  "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "name": "Juan",
  "lastName": "Perez",
  "specialtyId": 1,
  "register": "CMP12345",
  "phone": "+51999888777",
  "email": "juan.perez@medical.com",
  "photoUrl": "/uploads/doctors/018fdf9c6a7b7b0b8d765fa42c9431f2.jpg",
  "isVet": false,
  "isActive": true,
  "createdAt": "2026-06-05T00:00:00Z",
  "updatedAt": "2026-06-05T00:00:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos en los campos del cuerpo.
*   **`500 Internal Server Error`**: Error físico al escribir el archivo de foto (activa rollback automático).

---

### Actualizar Médico (`PUT /api/doctors/{id}`)

*   **Ruta:** `PUT /api/doctors/{id}`
*   **Nombre de Acción:** `UpdateDoctor`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único del médico a actualizar.

#### Cuerpo de la Solicitud (FormData / `multipart/form-data`)
La petición debe enviarse codificada como formulario (`multipart/form-data`) con los siguientes campos:

*   `name` (string, Requerido): Nombre del médico. Máx. 200 caracteres.
*   `lastName` (string, Requerido): Apellido. Máx. 200 caracteres.
*   `specialtyId` (int, Opcional): ID de especialidad.
*   `register` (string, Opcional): Colegiatura médica.
*   `phone` (string, Opcional): Teléfono.
*   `email` (string, Opcional): Email.
*   `photo` (file / IFormFile, Opcional): Nuevo archivo de foto. Si se omite, se conserva la foto actual.
*   `isVet` (bool, Requerido): Determina si atiende veterinaria.
*   `isActive` (bool, Requerido): Estado activo/inactivo.
*   `centers` (string, Opcional): Cadena JSON que representa la lista actualizada de afiliaciones (`DoctorAffiliationRequest`). Las afiliaciones antiguas que no se incluyan en este JSON serán desvinculadas automáticamente de la base de datos. Ejemplo de valor: `[{"id":"018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8","officeNumber":"Consultorio 302 Modificado","workSchedule":"Lun-Vie 9am-1pm"}]`.

#### Comportamiento Transaccional y Atomicidad
*   Opera dentro de una transacción.
*   Si se proporciona una nueva foto en `photo`:
    *   Se actualizan los campos y el nuevo path en base de datos.
    *   Se escribe el archivo de imagen físicamente en `/uploads/doctors/`.
    *   Si se guarda correctamente, se elimina el archivo de imagen anterior del médico para evitar archivos basura.
    *   **Rollback**: Si la escritura física del nuevo archivo falla, se realiza un rollback automático de la base de datos y se elimina el archivo temporal creado en disco, retornando `500 Internal Server Error`.

#### Respuesta Exitosa (`200 OK`)
Retorna los datos del médico actualizados:
```json
{
  "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "name": "Juan Modificado",
  "lastName": "Perez",
  "specialtyId": 1,
  "register": "CMP12345",
  "phone": "+51999888777",
  "email": "juan.perez@medical.com",
  "photoUrl": "/uploads/doctors/018fdf9c6a7b7b0b8d765fa42c9431f2_new.jpg",
  "isVet": false,
  "isActive": true,
  "createdAt": "2026-06-05T00:00:00Z",
  "updatedAt": "2026-06-05T00:10:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos en los campos de formulario.
*   **`404 Not Found`**: No se encuentra un médico con el `id` provisto.
*   **`500 Internal Server Error`**: Ocurrió un error en el almacenamiento del archivo (activa rollback automático).

---

### Activar o Inactivar Médico (`PATCH /api/doctors/{id}/toggle-status`)

*   **Ruta:** `PATCH /api/doctors/{id}/toggle-status`
*   **Nombre de Acción:** `ToggleDoctorStatus`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único del médico a alternar su estado.

#### Respuesta Exitosa (`200 OK`)
Retorna el nuevo estado lógico tras alternar su valor `isActive`:
```json
{
  "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "name": "Juan",
  "lastName": "Perez",
  "isActive": false,
  "status": "Inactivado"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: No se encuentra el médico con el `id` provisto.

---

### Eliminar Médico (`DELETE /api/doctors/{id}`)

*   **Ruta:** `DELETE /api/doctors/{id}`
*   **Nombre de Acción:** `DeleteDoctor`
*   **Autorización:** Requerido (`.RequireAuthorization()`)
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único del médico a eliminar físicamente del sistema.

#### Respuesta Exitosa (`204 No Content`)
El recurso se eliminó correctamente de la base de datos y no se devuelve cuerpo en la respuesta.

#### Otras Respuestas
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales válidas.
*   **`404 Not Found`**: No se encuentra un médico con el `id` provisto.
