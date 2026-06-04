# Documentación de Endpoints de Especialidades Médicas (Slice: Specialties)

Esta documentación detalla de forma exhaustiva únicamente los endpoints de **Gestión de Especialidades** (`WithTags("Specialties")`) que pertenecen al Slice Vertical de Especialidades (`Specialties`) de la aplicación.

---

## 📌 Tabla de Contenidos

- [Obtener Especialidad por ID (`GET /api/specialties/{id}`)](#obtener-especialidad-por-id-get-apispecialtiesid)
- [Listar Todas las Especialidades (`GET /api/specialties`)](#listar-todas-las-especialidades-get-apispecialties)
- [Listar Especialidades Paginadas (`GET /api/specialties/paged`)](#listar-especialidades-paginadas-get-apispecialtiespaged)
- [Lookup / Búsqueda Rápida de Especialidades Activas (`GET /api/specialties/lookup`)](#lookup--búsqueda-rápida-de-especialidades-activas-get-apispecialtieslookup)
- [Crear Especialidad (`POST /api/specialties`)](#crear-especialidad-post-apispecialties)
- [Actualizar Especialidad (`PUT /api/specialties/{id}`)](#actualizar-especialidad-put-apispecialtiesid)
- [Activar o Inactivar Especialidad (`PATCH /api/specialties/{id}/toggle-status`)](#activar-o-inactivar-especialidad-patch-apispecialtiesidtoggle-status)

---

### Obtener Especialidad por ID (`GET /api/specialties/{id}`)

*   **Ruta:** `GET /api/specialties/{id}`
*   **Nombre de Acción:** `GetSpecialty`
*   **Autorización:** Pública (Acceso libre en el enrutamiento).
*   **Parámetros de Ruta:**
    *   `id` (int, Requerido): ID único correlativo de la especialidad.

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `GetSpecialtyResponse` con los detalles de la especialidad (incluyendo su descripción):
```json
{
  "id": 1,
  "name": "Cardiología",
  "description": "Especialidad encargada de las enfermedades del corazón y del aparato circulatorio.",
  "isActive": true,
  "createdAt": "2026-06-03T10:00:00Z"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: No se encuentra una especialidad con el `id` provisto.

---

### Listar Todas las Especialidades (`GET /api/specialties`)

*   **Ruta:** `GET /api/specialties`
*   **Nombre de Acción:** `ListSpecialties`
*   **Autorización:** Pública (Acceso libre, enrutamiento público).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<ListSpecialtiesResponse>` de todas las especialidades registradas en el sistema, ordenadas alfabéticamente por nombre:
```json
[
  {
    "id": 1,
    "name": "Cardiología",
    "description": "Especialidad encargada de las enfermedades del corazón y del aparato circulatorio.",
    "isActive": true,
    "createdAt": "2026-06-03T10:00:00Z"
  },
  {
    "id": 2,
    "name": "Pediatría",
    "description": "Especialidad médica que estudia al niño y sus enfermedades.",
    "isActive": true,
    "createdAt": "2026-06-03T10:15:00Z"
  }
]
```

---

### Listar Especialidades Paginadas (`GET /api/specialties/paged`)

*   **Ruta:** `GET /api/specialties/paged`
*   **Nombre de Acción:** `PagedSpecialties`
*   **Autorización:** Pública.
*   **Parámetros de Consulta (Query Params):**
    *   `page` (int, Opcional, por defecto `1`): Número de página. Debe ser mayor o igual a 1.
    *   `pageSize` (int, Opcional, por defecto `10`): Tamaño de la página. Debe estar entre 1 y 100.
    *   `search` (string, Opcional): Filtro de búsqueda insensible a mayúsculas/minúsculas sobre la columna `name` (`ILIKE`).
    *   `sortBy` (string, Opcional, por defecto `created_at`): Campo por el cual ordenar. Columnas permitidas: `name`, `description`, `isActive` (o `isactive`), `created_at`.
    *   `sortDesc` (bool, Opcional, por defecto `false`): Determina si la ordenación es descendente (`true`) o ascendente (`false`).

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `PaginatedResult<PagedSpecialtyItem>` que envuelve la lista y los metadatos correspondientes:
```json
{
  "items": [
    {
      "id": 1,
      "name": "Cardiología",
      "description": "Especialidad encargada de las enfermedades del corazón y del aparato circulatorio.",
      "isActive": true,
      "createdAt": "2026-06-03T10:00:00Z"
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

### Lookup / Búsqueda Rápida de Especialidades Activas (`GET /api/specialties/lookup`)

*   **Ruta:** `GET /api/specialties/lookup`
*   **Nombre de Acción:** `LookupSpecialties`
*   **Autorización:** Pública (usado usualmente para autocompletar comboboxes en el registro de doctores).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista simplificada `IEnumerable<LookupItem>` que contiene únicamente las especialidades **activas** (`is_active = true`), ordenadas alfabéticamente:
```json
[
  {
    "id": 1,
    "name": "Cardiología"
  },
  {
    "id": 2,
    "name": "Pediatría"
  }
]
```

---

### Crear Especialidad (`POST /api/specialties`)

*   **Ruta:** `POST /api/specialties`
*   **Nombre de Acción:** `CreateSpecialty`
*   **Autorización:** Pública (Acceso libre en el enrutamiento).

#### Cuerpo de la Solicitud (JSON)
*   `name` (string, Requerido): Nombre de la especialidad médica. Máx. 100 caracteres.
*   `description` (string, Opcional): Descripción detallada. Máx. 500 caracteres.

##### Ejemplo de Payload:
```json
{
  "name": "Neurología",
  "description": "Estudio y tratamiento de los trastornos del sistema nervioso."
}
```

#### Respuesta Exitosa (`201 Created`)
Retorna los datos de la especialidad creada, junto con la cabecera `Location` correspondiente:
```json
{
  "id": 3,
  "name": "Neurología",
  "description": "Estudio y tratamiento de los trastornos del sistema nervioso.",
  "isActive": true,
  "createdAt": "2026-06-04T03:30:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Error de validación (el nombre es vacío, supera los 100 caracteres o la descripción supera los 500 caracteres).

---

### Actualizar Especialidad (`PUT /api/specialties/{id}`)

*   **Ruta:** `PUT /api/specialties/{id}`
*   **Nombre de Acción:** `UpdateSpecialty`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (int, Requerido): ID de la especialidad que se va a actualizar.

#### Cuerpo de la Solicitud (JSON)
*   `name` (string, Requerido): Nombre de la especialidad médica. Máx. 100 caracteres.
*   `description` (string, Opcional): Descripción detallada. Máx. 500 caracteres.
*   `isActive` (bool, Requerido): Estado activo/inactivo de la especialidad.

##### Ejemplo de Payload:
```json
{
  "name": "Neurología Clínica",
  "description": "Estudio y tratamiento clínico avanzado de los trastornos del sistema nervioso.",
  "isActive": true
}
```

#### Respuesta Exitosa (`200 OK`)
Retorna el recurso actualizado:
```json
{
  "id": 3,
  "name": "Neurología Clínica",
  "description": "Estudio y tratamiento clínico avanzado de los trastornos del sistema nervioso.",
  "isActive": true,
  "createdAt": "2026-06-04T03:30:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos o errores de validación en el cuerpo.
*   **`404 Not Found`**: No se encuentra una especialidad con el `id` provisto.

---

### Activar o Inactivar Especialidad (`PATCH /api/specialties/{id}/toggle-status`)

*   **Ruta:** `PATCH /api/specialties/{id}/toggle-status`
*   **Nombre de Acción:** `ToggleSpecialtyStatus`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (int, Requerido): ID de la especialidad a alternar de estado.

#### Respuesta Exitosa (`200 OK`)
Retorna el nuevo estado lógico tras alternar su valor `isActive`:
```json
{
  "id": 3,
  "name": "Neurología Clínica",
  "isActive": false,
  "status": "Inactivado"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: No se encuentra la especialidad con el `id` provisto.
