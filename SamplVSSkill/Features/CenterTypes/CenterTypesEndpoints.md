# Documentación de Endpoints de Tipos de Centros Médicos (Slice: CenterTypes)

Esta documentación detalla de forma exhaustiva únicamente los endpoints de **Gestión de Tipos de Centros Médicos** (`WithTags("Center Types")`) que pertenecen al Slice Vertical de Tipos de Centros (`CenterTypes`) de la aplicación.

---

## 📌 Tabla de Contenidos

- [Obtener Tipo de Centro por ID (`GET /api/center-types/{id}`)](#obtener-tipo-de-centro-por-id-get-apicenter-typesid)
- [Listar Todos los Tipos de Centro (`GET /api/center-types`)](#listar-todos-los-tipos-de-centro-get-apicenter-types)
- [Listar Tipos de Centro Paginados (`GET /api/center-types/paged`)](#listar-tipos-de-centro-paginados-get-apicenter-typespaged)
- [Lookup / Búsqueda Rápida de Tipos de Centro Activos (`GET /api/center-types/lookup`)](#lookup--búsqueda-rápida-de-tipos-de-centro-activos-get-apicenter-typeslookup)
- [Crear Tipo de Centro (`POST /api/center-types`)](#crear-tipo-de-centro-post-apicenter-types)
- [Actualizar Tipo de Centro (`PUT /api/center-types/{id}`)](#actualizar-tipo-de-centro-put-apicenter-typesid)
- [Activar o Inactivar Tipo de Centro (`PATCH /api/center-types/{id}/toggle-status`)](#activar-o-inactivar-tipo-de-centro-patch-apicenter-typesidtoggle-status)

---

### Obtener Tipo de Centro por ID (`GET /api/center-types/{id}`)

*   **Ruta:** `GET /api/center-types/{id}`
*   **Nombre de Acción:** `GetCenterType`
*   **Autorización:** Pública (Acceso libre en el enrutamiento).
*   **Parámetros de Ruta:**
    *   `id` (int, Requerido): ID único del tipo de centro a consultar.

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `GetCenterTypeResponse` con los detalles del tipo de centro:
```json
{
  "id": 1,
  "name": "Clínica",
  "isActive": true,
  "createdAt": "2026-06-03T10:00:00Z",
  "updatedAt": "2026-06-03T10:00:00Z"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: No se encuentra un tipo de centro con el `id` provisto.

---

### Listar Todos los Tipos de Centro (`GET /api/center-types`)

*   **Ruta:** `GET /api/center-types`
*   **Nombre de Acción:** `ListCenterTypes`
*   **Autorización:** Pública (Acceso libre, enrutamiento público).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<ListCenterTypesResponse>` de todos los tipos de centro registrados en el sistema, ordenados por fecha de creación:
```json
[
  {
    "id": 1,
    "name": "Clínica",
    "isActive": true,
    "createdAt": "2026-06-03T10:00:00Z",
    "updatedAt": "2026-06-03T10:00:00Z"
  },
  {
    "id": 2,
    "name": "Hospital",
    "isActive": true,
    "createdAt": "2026-06-03T10:15:00Z",
    "updatedAt": "2026-06-03T10:15:00Z"
  }
]
```

---

### Listar Tipos de Centro Paginados (`GET /api/center-types/paged`)

*   **Ruta:** `GET /api/center-types/paged`
*   **Nombre de Acción:** `PagedCenterTypes`
*   **Autorización:** Pública.
*   **Parámetros de Consulta (Query Params):**
    *   `page` (int, Opcional, por defecto `1`): Número de página. Debe ser mayor o igual a 1.
    *   `pageSize` (int, Opcional, por defecto `10`): Tamaño de la página. Debe estar entre 1 y 100.
    *   `search` (string, Opcional): Filtro de búsqueda insensible a mayúsculas/minúsculas sobre la columna `name` (`ILIKE`).
    *   `sortBy` (string, Opcional, por defecto `created_at`): Campo por el cual ordenar. Columnas permitidas: `name`, `isActive` (o `isactive`), `created_at`, `updated_at`.
    *   `sortDesc` (bool, Opcional, por defecto `false`): Determina si la ordenación es descendente (`true`) o ascendente (`false`).

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `PaginatedResult<PagedCenterTypeItem>` que envuelve la lista y los metadatos correspondientes:
```json
{
  "items": [
    {
      "id": 1,
      "name": "Clínica",
      "isActive": true,
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

### Lookup / Búsqueda Rápida de Tipos de Centro Activos (`GET /api/center-types/lookup`)

*   **Ruta:** `GET /api/center-types/lookup`
*   **Nombre de Acción:** `LookupCenterTypes`
*   **Autorización:** Pública (usado usualmente para poblar desplegables en formularios de centros médicos).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista simplificada `IEnumerable<LookupItem>` que contiene únicamente los tipos de centro **activos** (`is_active = true`), ordenados alfabéticamente:
```json
[
  {
    "id": 1,
    "name": "Clínica"
  },
  {
    "id": 2,
    "name": "Hospital"
  }
]
```

---

### Crear Tipo de Centro (`POST /api/center-types`)

*   **Ruta:** `POST /api/center-types`
*   **Nombre de Acción:** `CreateCenterType`
*   **Autorización:** Pública (Acceso libre en el enrutamiento).

#### Cuerpo de la Solicitud (JSON)
*   `name` (string, Requerido): Nombre del tipo de centro. Máx. 100 caracteres.

##### Ejemplo de Payload:
```json
{
  "name": "Consultorio Privado"
}
```

#### Respuesta Exitosa (`201 Created`)
Retorna los datos del tipo de centro creado, junto con la cabecera `Location` correspondiente:
```json
{
  "id": 3,
  "name": "Consultorio Privado",
  "isActive": true,
  "createdAt": "2026-06-04T03:30:00Z",
  "updatedAt": "2026-06-04T03:30:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Error de validación (el nombre es vacío o supera los 100 caracteres).

---

### Actualizar Tipo de Centro (`PUT /api/center-types/{id}`)

*   **Ruta:** `PUT /api/center-types/{id}`
*   **Nombre de Acción:** `UpdateCenterType`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (int, Requerido): ID del tipo de centro que se va a actualizar.

#### Cuerpo de la Solicitud (JSON)
*   `name` (string, Requerido): Nombre del tipo de centro. Máx. 100 caracteres.
*   `isActive` (bool, Requerido): Estado activo/inactivo del tipo de centro.

##### Ejemplo de Payload:
```json
{
  "name": "Consultorio Privado Modificado",
  "isActive": true
}
```

#### Respuesta Exitosa (`200 OK`)
Retorna el recurso actualizado:
```json
{
  "id": 3,
  "name": "Consultorio Privado Modificado",
  "isActive": true,
  "createdAt": "2026-06-04T03:30:00Z",
  "updatedAt": "2026-06-04T03:35:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos o errores de validación en el cuerpo.
*   **`404 Not Found`**: No se encuentra el tipo de centro con el `id` provisto.

---

### Activar o Inactivar Tipo de Centro (`PATCH /api/center-types/{id}/toggle-status`)

*   **Ruta:** `PATCH /api/center-types/{id}/toggle-status`
*   **Nombre de Acción:** `ToggleCenterTypeStatus`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (int, Requerido): ID del tipo de centro a alternar de estado.

#### Respuesta Exitosa (`200 OK`)
Retorna el nuevo estado lógico tras alternar su valor `isActive`:
```json
{
  "id": 3,
  "name": "Consultorio Privado Modificado",
  "isActive": false,
  "status": "Inactivado"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: No se encuentra el tipo de centro con el `id` provisto.
