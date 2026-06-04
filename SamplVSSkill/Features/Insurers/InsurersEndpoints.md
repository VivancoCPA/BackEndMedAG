# Documentación de Endpoints de Aseguradoras (Slice: Insurers)

Esta documentación detalla de forma exhaustiva únicamente los endpoints de **Gestión de Aseguradoras** (`WithTags("Insurers")`) que pertenecen al Slice Vertical de Aseguradoras (`Insurers`) de la aplicación.

---

## 📌 Tabla de Contenidos

- [Obtener Aseguradora por ID (`GET /api/insurers/{id}`)](#obtener-aseguradora-por-id-get-apiinsurersid)
- [Listar Todas las Aseguradoras (`GET /api/insurers`)](#listar-todas-las-aseguradoras-get-apiinsurers)
- [Listar Aseguradoras Paginadas (`GET /api/insurers/paged`)](#listar-aseguradoras-paginados-get-apiinsurerspaged)
- [Lookup / Búsqueda Rápida de Aseguradoras Activas (`GET /api/insurers/lookup`)](#lookup--búsqueda-rápida-de-aseguradoras-activas-get-apiinsurerslookup)
- [Crear Aseguradora (`POST /api/insurers`)](#crear-aseguradora-post-apiinsurers)
- [Actualizar Aseguradora (`PUT /api/insurers/{id}`)](#actualizar-aseguradora-put-apiinsurersid)
- [Activar o Inactivar Aseguradora (`PATCH /api/insurers/{id}/toggle-status`)](#activar-o-inactivar-aseguradora-patch-apiinsurersidtoggle-status)

---

### Obtener Aseguradora por ID (`GET /api/insurers/{id}`)

*   **Ruta:** `GET /api/insurers/{id}`
*   **Nombre de Acción:** `GetInsurer`
*   **Autorización:** Requerido (`.RequireAuthorization()`)
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único de la aseguradora a consultar.

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `GetInsurerResponse` con la información detallada de la aseguradora:
```json
{
  "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "name": "Seguros Salud S.A.",
  "address": "Av. República de Panamá 3055, San Isidro",
  "phone": "+511234567",
  "email": "contacto@segurossalud.com",
  "personInCharge": "Carlos Pérez",
  "logoUrl": "https://storage.segurossalud.com/logo.png",
  "isActive": true,
  "createdAt": "2026-06-03T10:00:00Z",
  "updatedAt": "2026-06-03T10:00:00Z"
}
```

#### Otras Respuestas
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales de autenticación válidas.
*   **`404 Not Found`**: No se encuentra una aseguradora con el `id` provisto.

---

### Listar Todas las Aseguradoras (`GET /api/insurers`)

*   **Ruta:** `GET /api/insurers`
*   **Nombre de Acción:** `ListInsurers`
*   **Autorización:** Ninguna (Acceso Público, comentado en el enrutamiento).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<ListInsurersResponse>` con todas las aseguradoras registradas en el sistema, ordenadas alfabéticamente por nombre:
```json
[
  {
    "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
    "name": "Aseguradora Alfa",
    "phone": "+511111222",
    "email": "contacto@alfa.com",
    "personInCharge": "Juan Gomez",
    "isActive": true,
    "createdAt": "2026-06-03T10:00:00Z",
    "updatedAt": "2026-06-03T10:00:00Z"
  },
  {
    "id": "018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8",
    "name": "Seguros Salud S.A.",
    "phone": "+511234567",
    "email": "contacto@segurossalud.com",
    "personInCharge": "Carlos Pérez",
    "isActive": true,
    "createdAt": "2026-06-03T10:00:00Z",
    "updatedAt": "2026-06-03T10:00:00Z"
  }
]
```

#### Otras Respuestas
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales de autenticación válidas.

---

### Listar Aseguradoras Paginadas (`GET /api/insurers/paged`)

*   **Ruta:** `GET /api/insurers/paged`
*   **Nombre de Acción:** `PagedInsurers`
*   **Autorización:** Ninguna (Acceso Público, comentado en el enrutamiento).
*   **Parámetros de Consulta (Query Params):**
    *   `page` (int, Opcional, por defecto `1`): Número de página. Debe ser mayor o igual a 1.
    *   `pageSize` (int, Opcional, por defecto `10`): Tamaño de la página. Debe estar entre 1 y 100.
    *   `search` (string, Opcional): Filtro de búsqueda que aplica sobre `name` y `email` (búsqueda insensible a mayúsculas/minúsculas mediante `ILIKE`).
    *   `sortBy` (string, Opcional, por defecto `created_at`): Campo de ordenación. Columnas permitidas: `name`, `email`, `isActive`, `created_at`, `updated_at`.
    *   `sortDesc` (bool, Opcional, por defecto `false`): Determina si la ordenación es descendente (`true`) o ascendente (`false`).

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `PaginatedResult<PagedInsurerItem>` que envuelve la lista y los metadatos de paginación:
```json
{
  "items": [
    {
      "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
      "name": "Seguros Salud S.A.",
      "phone": "+511234567",
      "email": "contacto@segurossalud.com",
      "address": "Av. República de Panamá 3055, San Isidro",
      "logoUrl": "https://storage.segurossalud.com/logo.png",
      "personInCharge": "Carlos Pérez",
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
*   **`400 Bad Request`**: Parámetros de paginación fuera de rango o inválidos.
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales de autenticación válidas.

---

### Lookup / Búsqueda Rápida de Aseguradoras Activas (`GET /api/insurers/lookup`)

*   **Ruta:** `GET /api/insurers/lookup`
*   **Nombre de Acción:** `LookupInsurers`
*   **Autorización:** Ninguna (Acceso Público, utilizado habitualmente por el frontend para poblar dropdowns o combos de selección).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista simplificada `IEnumerable<LookupItemGuid>` que contiene únicamente las aseguradoras **activas** (`is_active = true`), ordenada alfabéticamente:
```json
[
  {
    "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
    "name": "Aseguradora Alfa"
  },
  {
    "id": "018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8",
    "name": "Seguros Salud S.A."
  }
]
```

---

### Crear Aseguradora (`POST /api/insurers`)

*   **Ruta:** `POST /api/insurers`
*   **Nombre de Acción:** `CreateInsurer`
*   **Autorización:** Ninguna (Acceso Público, comentado en el enrutamiento).

#### Cuerpo de la Solicitud (FormData / `multipart/form-data`)
La petición debe enviarse codificada como formulario (`multipart/form-data`) con los siguientes campos:

*   `name` (string, Requerido): Nombre de la aseguradora. Máx. 200 caracteres.
*   `address` (string, Requerido): Dirección física de la aseguradora. Máx. 500 caracteres.
*   `phone` (string, Requerido): Teléfono de contacto. Máx. 30 caracteres.
*   `email` (string, Requerido): Correo electrónico. Debe ser válido y tener máx. 200 caracteres.
*   `personInCharge` (string, Opcional): Nombre de la persona a cargo.
*   `photo` (file / IFormFile, Opcional): Archivo de imagen para el logo de la aseguradora.

> [!IMPORTANT]
> **Recomendación para el FrontEnd (React / JS / TS)**:
> Al invocar este endpoint utilizando un objeto `FormData`, **NO debes definir manualmente la cabecera `'Content-Type': 'multipart/form-data'`** en las cabeceras (`headers`) de tu cliente HTTP (`fetch` o `axios`). 
> Al pasar el objeto `FormData` como cuerpo de la petición, el navegador asignará y gestionará de manera automática la cabecera `multipart/form-data` e inyectará el parámetro dinámico de separación `boundary`. Si lo declaras a mano, la petición fallará en el servidor por ausencia de delimitadores.

#### Comportamiento
*   Valida los campos obligatorios del formulario.
*   Si se proporciona una foto/imagen en el campo `photo`, se guarda físicamente en el directorio local del servidor `wwwroot/uploads/insurers/` generando un nombre de archivo único mediante un GUID para evitar sobreescrituras o ataques path traversal.
*   **Atomicidad**: Primero se inserta el registro de la aseguradora en la base de datos PostgreSQL. Si tiene éxito, se escribe la imagen físicamente en el disco. Si la escritura física en disco falla, se realiza un rollback automático eliminando el registro recién creado de la aseguradora para evitar datos huérfanos o inconsistencias.

#### Respuesta Exitosa (`201 Created`)
Retorna la aseguradora creada con su ID autogenerado (UUID v7), la cabecera `Location` correspondiente y la ruta relativa del logo almacenado:
```json
{
  "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "name": "Seguros Salud S.A.",
  "address": "Av. República de Panamá 3055, San Isidro",
  "phone": "+511234567",
  "email": "contacto@segurossalud.com",
  "personInCharge": "Carlos Pérez",
  "logoUrl": "/uploads/insurers/7a2be748f65e2b1a42c398fed27e7fcd.jpg",
  "isActive": true,
  "createdAt": "2026-06-03T10:00:00Z",
  "updatedAt": "2026-06-03T10:00:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos en el cuerpo (error de validación FluentValidation).
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales de autenticación válidas.
*   **`500 Internal Server Error`**: Ocurrió un error físico en el disco al guardar el archivo de la imagen, resultando en un rollback del registro.

---

### Actualizar Aseguradora (`PUT /api/insurers/{id}`)

*   **Ruta:** `PUT /api/insurers/{id}`
*   **Nombre de Acción:** `UpdateInsurer`
*   **Autorización:** Ninguna (Acceso Público, comentado en el enrutamiento).
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único de la aseguradora a actualizar.

#### Cuerpo de la Solicitud (FormData / `multipart/form-data`)
La petición debe enviarse codificada como formulario (`multipart/form-data`) con los siguientes campos:

*   `name` (string, Requerido): Nombre de la aseguradora. Máx. 200 caracteres.
*   `address` (string, Requerido): Dirección física. Máx. 500 caracteres.
*   `phone` (string, Requerido): Teléfono de contacto. Máx. 30 caracteres.
*   `email` (string, Requerido): Correo electrónico. Debe ser válido y tener máx. 200 caracteres.
*   `personInCharge` (string, Opcional): Persona a cargo.
*   `photo` (file / IFormFile, Opcional): Nuevo archivo de imagen para el logo. Si se omite, se conserva el logo actual.
*   `isActive` (bool, Requerido): Estado de actividad.

> [!IMPORTANT]
> **Recomendación para el FrontEnd (React / JS / TS)**:
> Al igual que al crear, **NO debes definir manualmente la cabecera `'Content-Type'`** al enviar el objeto `FormData`. Deja que el navegador configure automáticamente `multipart/form-data` con sus separadores dinámicos.

#### Comportamiento
*   Valida los campos obligatorios del formulario.
*   Si se proporciona una nueva foto en `photo`:
    *   Primero se guardan los datos textuales y el nuevo path en la base de datos.
    *   Se escribe el archivo de imagen físicamente en el servidor (`wwwroot/uploads/insurers/`).
    *   Si se guarda correctamente, se elimina el archivo de imagen anterior de la aseguradora para evitar almacenamiento basura.
    *   **Atomicidad**: Si la escritura física del nuevo archivo falla, se realiza un rollback automático restaurando el path de la imagen anterior en la base de datos y se devuelve un `500 Internal Server Error`.
*   Si `photo` es nulo, los datos de la aseguradora se actualizan y se conserva la imagen que ya poseía.

#### Respuesta Exitosa (`200 OK`)
Retorna la aseguradora actualizada con los nuevos valores:
```json
{
  "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "name": "Seguros Salud S.A. - Sucursal Norte",
  "address": "Av. Alfredo Mendiola 3400, Los Olivos",
  "phone": "+5117654321",
  "email": "norte@segurossalud.com",
  "personInCharge": "Carlos Pérez Modificado",
  "logoUrl": "/uploads/insurers/018fdf9c6a7b7b0b8d765fa42c9431f2.jpg",
  "isActive": true,
  "createdAt": "2026-06-03T10:00:00Z",
  "updatedAt": "2026-06-03T10:30:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos en el cuerpo (error de validación FluentValidation).
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales de autenticación válidas.
*   **`404 Not Found`**: No se encuentra una aseguradora con el `id` provisto.
*   **`500 Internal Server Error`**: Ocurrió un error físico en el disco al guardar la nueva imagen, resultando en un rollback del registro a su estado anterior.

---

### Activar o Inactivar Aseguradora (`PATCH /api/insurers/{id}/toggle-status`)

*   **Ruta:** `PATCH /api/insurers/{id}/toggle-status`
*   **Nombre de Acción:** `ToggleInsurerStatus`
*   **Autorización:** Ninguna (Acceso Público, comentado en el enrutamiento).
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único de la aseguradora a la que se le alternará el estado.

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto indicando el nuevo estado lógico de la aseguradora:
```json
{
  "id": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "name": "Seguros Salud S.A.",
  "isActive": false,
  "status": "Inactivado"
}
```

#### Otras Respuestas
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales de autenticación válidas.
*   **`404 Not Found`**: No se encuentra una aseguradora con el `id` provisto.
