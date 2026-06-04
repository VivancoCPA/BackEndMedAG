# Documentación de Endpoints de Centros Médicos (Slice: MedicalCenters)

Esta documentación detalla de forma exhaustiva únicamente los endpoints de **Gestión de Centros Médicos** (`WithTags("MedicalCenters")`) que pertenecen al Slice Vertical de Centros Médicos (`MedicalCenters`) de la aplicación.

---

## 📌 Tabla de Contenidos

- [Obtener Centro Médico por ID (`GET /api/medical-centers/{id}`)](#obtener-centro-médico-por-id-get-apimedical-centersid)
- [Listar Todos los Centros Médicos (`GET /api/medical-centers`)](#listar-todos-los-centros-médicos-get-apimedical-centers)
- [Listar Centros Médicos Paginados (`GET /api/medical-centers/paged`)](#listar-centros-médicos-paginados-get-apimedical-centerspaged)
- [Lookup / Búsqueda Rápida de Centros Médicos Activos (`GET /api/medical-centers/lookup`)](#lookup--búsqueda-rápida-de-centros-médicos-activos-get-apimedical-centerslookup)
- [Obtener Resumen Estadístico de Centros Médicos (`GET /api/medical-centers/summary`)](#obtener-resumen-estadístico-de-centros-médicos-get-apimedical-centerssummary)
- [Crear Centro Médico (`POST /api/medical-centers`)](#crear-centro-médico-post-apimedical-centers)
- [Actualizar Centro Médico (`PUT /api/medical-centers/{id}`)](#actualizar-centro-médico-put-apimedical-centersid)
- [Activar o Inactivar Centro Médico (`PATCH /api/medical-centers/{id}/toggle-status`)](#activar-o-inactivar-centro-médico-patch-apimedical-centersidtoggle-status)

---

### Obtener Centro Médico por ID (`GET /api/medical-centers/{id}`)

*   **Ruta:** `GET /api/medical-centers/{id}`
*   **Nombre de Acción:** `GetMedicalCenter`
*   **Autorización:** Requerido (`.RequireAuthorization()`)
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único (UUID v7) del centro médico a consultar.

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `GetMedicalCenterResponse` con los detalles del centro médico y su tipo asociado:
```json
{
  "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "name": "Clínica San Felipe - Sede Jesús María",
  "typeId": 1,
  "typeName": "Clínica",
  "address": "Av. Gregorio Escobedo 650, Jesús María",
  "phone": "+5112190000",
  "isActive": true,
  "latitude": -12.07894,
  "longitude": -77.05123,
  "createdAt": "2026-06-03T10:00:00Z",
  "updatedAt": "2026-06-03T10:00:00Z"
}
```

#### Otras Respuestas
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales de autenticación válidas o falta el token JWT.
*   **`404 Not Found`**: No se encuentra un centro médico con el `id` provisto.

---

### Listar Todos los Centros Médicos (`GET /api/medical-centers`)

*   **Ruta:** `GET /api/medical-centers`
*   **Nombre de Acción:** `ListMedicalCenters`
*   **Autorización:** Pública (enrutamiento de acceso libre).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<ListMedicalCentersResponse>` ordenada alfabéticamente por el nombre del centro:
```json
[
  {
    "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
    "name": "Clínica San Felipe - Sede Jesús María",
    "typeId": 1,
    "typeName": "Clínica",
    "address": "Av. Gregorio Escobedo 650, Jesús María",
    "phone": "+5112190000",
    "isActive": true,
    "latitude": -12.07894,
    "longitude": -77.05123,
    "createdAt": "2026-06-03T10:00:00Z",
    "updatedAt": "2026-06-03T10:00:00Z"
  },
  {
    "id": "018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8",
    "name": "Hospital Rebagliati",
    "typeId": 2,
    "typeName": "Hospital",
    "address": "Av. Salaverry 1400, Jesús María",
    "phone": "+5112654900",
    "isActive": true,
    "latitude": -12.07123,
    "longitude": -77.04234,
    "createdAt": "2026-06-03T10:15:00Z",
    "updatedAt": "2026-06-03T10:15:00Z"
  }
]
```

---

### Listar Centros Médicos Paginados (`GET /api/medical-centers/paged`)

*   **Ruta:** `GET /api/medical-centers/paged`
*   **Nombre de Acción:** `PagedMedicalCenters`
*   **Autorización:** Pública.
*   **Parámetros de Consulta (Query Params):**
    *   `page` (int, Opcional, por defecto `1`): Número de página. Debe ser mayor o igual a 1.
    *   `pageSize` (int, Opcional, por defecto `10`): Tamaño de la página. Debe estar entre 1 y 100.
    *   `search` (string, Opcional): Filtro de búsqueda insensible a mayúsculas/minúsculas sobre `mc.name`, `mc.address` y `ct.name` (`ILIKE`).
    *   `sortBy` (string, Opcional, por defecto `created_at`): Campo por el cual ordenar. Columnas permitidas: `name` (`mc.name`), `type` (`ct.name`), `address` (`mc.address`), `isActive` (o `isactive`), `created_at` (`mc.created_at`), `updated_at` (`mc.updated_at`).
    *   `sortDesc` (bool, Opcional, por defecto `false`): Determina si la ordenación es descendente (`true`) o ascendente (`false`).

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `PaginatedResult<PagedMedicalCenterItem>` que envuelve la lista y los metadatos correspondientes:
```json
{
  "items": [
    {
      "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
      "name": "Clínica San Felipe - Sede Jesús María",
      "typeId": 1,
      "typeName": "Clínica",
      "address": "Av. Gregorio Escobedo 650, Jesús María",
      "phone": "+5112190000",
      "isActive": true,
      "latitude": -12.07894,
      "longitude": -77.05123,
      "createdAt": "2026-06-03T10:00:00Z",
      "updatedAt": "2026-06-03T10:00:00Z"
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
*   **`400 Bad Request`**: Parámetros de paginación fuera de rango (`page < 1` o `pageSize` fuera de `1-100`).

---

### Lookup / Búsqueda Rápida de Centros Médicos Activos (`GET /api/medical-centers/lookup`)

*   **Ruta:** `GET /api/medical-centers/lookup`
*   **Nombre de Acción:** `LookupMedicalCenters`
*   **Autorización:** Pública (usado para autocompletar desplegables de centros médicos en el frontend).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista simplificada `IEnumerable<LookupItemGuid>` que contiene únicamente los centros médicos **activos** (`is_active = true`), ordenados alfabéticamente:
```json
[
  {
    "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
    "name": "Clínica San Felipe - Sede Jesús María"
  },
  {
    "id": "018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8",
    "name": "Hospital Rebagliati"
  }
]
```

---

### Obtener Resumen Estadístico de Centros Médicos (`GET /api/medical-centers/summary`)

*   **Ruta:** `GET /api/medical-centers/summary`
*   **Nombre de Acción:** `SummaryMedicalCenters`
*   **Autorización:** Pública.

#### Respuesta Exitosa (`200 OK`)
Retorna un conteo de alto rendimiento de los centros médicos clasificados por su estado lógico (`isActive`):
```json
{
  "total": 42,
  "active": 38,
  "inactive": 4
}
```

---

### Crear Centro Médico (`POST /api/medical-centers`)

*   **Ruta:** `POST /api/medical-centers`
*   **Nombre de Acción:** `CreateMedicalCenter`
*   **Autorización:** Pública.

#### Cuerpo de la Solicitud (JSON)
*   `name` (string, Requerido): Nombre del centro médico. Máx. 200 caracteres.
*   `typeId` (int, Opcional): ID del tipo de centro (`centers_type.id`). Debe ser mayor a 0 si se proporciona.
*   `address` (string, Opcional): Dirección del centro. Máx. 500 caracteres.
*   `phone` (string, Opcional): Teléfono de contacto. Máx. 30 caracteres. Debe contener solo números, espacios y caracteres especiales de teléfono (`+`, `-`, `()`).
*   `isActive` (bool, Requerido): Estado activo inicial.
*   `latitude` (double, Opcional): Latitud geográfica. Rango `[-90, 90]`.
*   `longitude` (double, Opcional): Longitud geográfica. Rango `[-180, 180]`.

##### Ejemplo de Payload:
```json
{
  "name": "Clínica Delgado",
  "typeId": 1,
  "address": "Av. Angamos Oeste 401, Miraflores",
  "phone": "+5113777000",
  "isActive": true,
  "latitude": -12.11124,
  "longitude": -77.03154
}
```

#### Respuesta Exitosa (`201 Created`)
Retorna el centro médico creado con su ID UUID v7 y cabecera `Location` correspondiente:
```json
{
  "id": "018fdf9c-703c-7b0b-8d76-7ff39c9431f9",
  "name": "Clínica Delgado",
  "typeId": 1,
  "typeName": "Clínica",
  "address": "Av. Angamos Oeste 401, Miraflores",
  "phone": "+5113777000",
  "isActive": true,
  "latitude": -12.11124,
  "longitude": -77.03154,
  "createdAt": "2026-06-04T21:00:00Z",
  "updatedAt": "2026-06-04T21:00:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Error de validación en los campos del cuerpo (FluentValidation).

---

### Actualizar Centro Médico (`PUT /api/medical-centers/{id}`)

*   **Ruta:** `PUT /api/medical-centers/{id}`
*   **Nombre de Acción:** `UpdateMedicalCenter`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único del centro médico a actualizar.

#### Cuerpo de la Solicitud (JSON)
*   `name` (string, Requerido): Nombre del centro médico. Máx. 200 caracteres.
*   `typeId` (int, Opcional): ID del tipo de centro.
*   `address` (string, Opcional): Dirección del centro. Máx. 500 caracteres.
*   `phone` (string, Opcional): Teléfono de contacto. Máx. 30 caracteres.
*   `isActive` (bool, Requerido): Estado activo/inactivo.
*   `latitude` (double, Opcional): Latitud geográfica. Rango `[-90, 90]`.
*   `longitude` (double, Opcional): Longitud geográfica. Rango `[-180, 180]`.

##### Ejemplo de Payload:
```json
{
  "name": "Clínica Delgado - Sede Miraflores",
  "typeId": 1,
  "address": "Av. Angamos Oeste 401, Miraflores",
  "phone": "+5113777001",
  "isActive": true,
  "latitude": -12.11124,
  "longitude": -77.03154
}
```

#### Respuesta Exitosa (`200 OK`)
Retorna los datos del centro médico actualizado:
```json
{
  "id": "018fdf9c-703c-7b0b-8d76-7ff39c9431f9",
  "name": "Clínica Delgado - Sede Miraflores",
  "typeId": 1,
  "typeName": "Clínica",
  "address": "Av. Angamos Oeste 401, Miraflores",
  "phone": "+5113777001",
  "isActive": true,
  "latitude": -12.11124,
  "longitude": -77.03154,
  "createdAt": "2026-06-04T21:00:00Z",
  "updatedAt": "2026-06-04T21:10:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos o errores de validación.
*   **`404 Not Found`**: No se encuentra un centro médico con el `id` provisto.

---

### Activar o Inactivar Centro Médico (`PATCH /api/medical-centers/{id}/toggle-status`)

*   **Ruta:** `PATCH /api/medical-centers/{id}/toggle-status`
*   **Nombre de Acción:** `ToggleMedicalCenterStatus`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único del centro médico a alternar su estado.

#### Respuesta Exitosa (`200 OK`)
Retorna el nuevo estado lógico tras alternar su valor `isActive`:
```json
{
  "id": "018fdf9c-703c-7b0b-8d76-7ff39c9431f9",
  "name": "Clínica Delgado - Sede Miraflores",
  "isActive": false,
  "status": "Inactivado"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: No se encuentra el centro médico con el `id` provisto.
