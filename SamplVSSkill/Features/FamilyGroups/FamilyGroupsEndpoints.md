# Documentación de Endpoints de Grupos Familiares (Slice: FamilyGroups)

Esta documentación detalla de forma exhaustiva los endpoints de **Gestión de Grupos Familiares** (`WithTags("Family Groups")`) que pertenecen al Slice Vertical de Grupos Familiares (`FamilyGroups`) de la aplicación.

---

## 📌 Tabla de Contenidos

- [Obtener Grupo Familiar por ID (`GET /api/family-groups/{id}`)](#obtener-grupo-familiar-por-id-get-apifamily-groupsid)
- [Listar Todos los Grupos Familiares (`GET /api/family-groups`)](#listar-todos-los-grupos-familiares-get-apifamily-groups)
- [Obtener Mi Grupo Familiar (`GET /api/family-groups/my`)](#obtener-mi-grupo-familiar-get-apifamily-groupsmy)
- [Listar Grupos Familiares Paginados (`GET /api/family-groups/paged`)](#listar-grupos-familiares-paginados-get-apifamily-groupspaged)
- [Crear Grupo Familiar (`POST /api/family-groups`)](#crear-grupo-familiar-post-apifamily-groups)
- [Actualizar Grupo Familiar (`PUT /api/family-groups/{id}`)](#actualizar-grupo-familiar-put-apifamily-groupsid)
- [Activar o Inactivar Grupo Familiar (`PATCH /api/family-groups/{id}/toggle-status`)](#activar-o-inactivar-grupo-familiar-patch-apifamily-groupsidtoggle-status)

---

### Obtener Grupo Familiar por ID (`GET /api/family-groups/{id}`)

*   **Ruta:** `GET /api/family-groups/{id}`
*   **Nombre de Acción:** `GetFamilyGroup`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID único del grupo familiar.

#### Respuesta Exitosa (`200 OK`)
Retorna los detalles del grupo familiar consultado, incluyendo el nombre del propietario (`ownerName` obtenido a partir del `AspNetUsers` relacionado):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Familia Pérez",
  "userId": "user-guid-id-1234",
  "ownerName": "Juan Pérez",
  "photoUrl": "/uploads/groups/perez.jpg",
  "isActive": true,
  "createdAt": "2026-06-05T10:00:00Z"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: Cuando el grupo familiar con el `id` provisto no existe.
    *   Ejemplo de mensaje: `"Grupo familiar '3fa85f64-5717-4562-b3fc-2c963f66afa6' no encontrado."`

---

### Listar Todos los Grupos Familiares (`GET /api/family-groups`)

*   **Ruta:** `GET /api/family-groups`
*   **Nombre de Acción:** `ListFamilyGroups`
*   **Autorización:** Pública.

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<ListFamilyGroupsResponse>` ordenada alfabéticamente por el nombre del grupo familiar, incluyendo sus miembros y miembros extras:
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Familia Pérez",
    "userId": "user-guid-id-1234",
    "ownerName": "Juan Pérez",
    "photoUrl": "/uploads/groups/perez.jpg",
    "isActive": true,
    "createdAt": "2026-06-05T10:00:00Z",
    "members": [
      {
        "id": 1,
        "userId": "user-guid-id-5678",
        "email": "maria@example.com",
        "name": "María",
        "lastName": "Pérez",
        "photoUrl": "/uploads/profiles/maria.jpg",
        "isAdmin": false,
        "relationship": "HIJO_A",
        "isActive": true
      }
    ],
    "extraMembers": [
      {
        "id": 1,
        "fullName": "Abuelo Pedro",
        "idType": "DNI",
        "photoUrl": "/uploads/extra/pedro.jpg",
        "description": "El abuelo de la familia",
        "isActive": true,
        "createdAt": "2026-06-06T01:00:00Z"
      }
    ]
  }
]
```

---

### Obtener Mi Grupo Familiar (`GET /api/family-groups/my`)

*   **Ruta:** `GET /api/family-groups/my`
*   **Nombre de Acción:** `GetMyFamilyGroup`
*   **Autorización:** Requerido (`.RequireAuthorization()`).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<ListFamilyGroupsResponse>` (que contiene como máximo un elemento si el usuario es miembro registrado de algún grupo del cual no es creador/propietario, o una lista vacía `[]` si no pertenece a ninguno):
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Familia Pérez",
    "userId": "user-guid-id-1234",
    "ownerName": "Juan Pérez",
    "photoUrl": "/uploads/groups/perez.jpg",
    "isActive": true,
    "createdAt": "2026-06-05T10:00:00Z",
    "members": [
      {
        "id": 1,
        "userId": "user-guid-id-5678",
        "email": "maria@example.com",
        "name": "María",
        "lastName": "Pérez",
        "photoUrl": "/uploads/profiles/maria.jpg",
        "isAdmin": false,
        "relationship": "HIJO_A",
        "isActive": true
      }
    ],
    "extraMembers": [
      {
        "id": 1,
        "fullName": "Abuelo Pedro",
        "idType": "DNI",
        "photoUrl": "/uploads/extra/pedro.jpg",
        "description": "El abuelo de la familia",
        "isActive": true,
        "createdAt": "2026-06-06T01:00:00Z"
      }
    ]
  }
]
```

#### Otras Respuestas
*   **`401 Unauthorized`**: Si la solicitud no incluye un token de autorización válido o el ID de usuario no puede extraerse.

---

### Listar Grupos Familiares Paginados (`GET /api/family-groups/paged`)

*   **Ruta:** `GET /api/family-groups/paged`
*   **Nombre de Acción:** `PagedFamilyGroups`
*   **Autorización:** Pública.
*   **Parámetros de Consulta (Query Params):**
    *   `page` (int, Opcional, por defecto `1`): Número de página. Debe ser mayor o igual a 1.
    *   `pageSize` (int, Opcional, por defecto `10`): Tamaño de la página. Debe estar entre 1 y 100.
    *   `search` (string, Opcional): Filtro de búsqueda insensible a mayúsculas/minúsculas sobre el nombre del grupo (`fg.name`) o el nombre del propietario (`u.name || ' ' || u.last_name`).
    *   `sortBy` (string, Opcional, por defecto `name`): Campo por el cual ordenar. Columnas permitidas: `name`, `ownername`, `isactive`, `createdat`.
    *   `sortDesc` (bool, Opcional, por defecto `false`): Determina si la ordenación es descendente (`true`) o ascendente (`false`).

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto paginado `PaginatedResult<PagedFamilyGroupItem>` con la lista y los metadatos correspondientes, incluyendo miembros y miembros extras para cada grupo de la página:
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Familia Pérez",
      "userId": "user-guid-id-1234",
      "ownerName": "Juan Pérez",
      "photoUrl": "/uploads/groups/perez.jpg",
      "isActive": true,
      "createdAt": "2026-06-05T10:00:00Z",
      "members": [
        {
          "id": 1,
          "userId": "user-guid-id-5678",
          "email": "maria@example.com",
          "name": "María",
          "lastName": "Pérez",
          "photoUrl": "/uploads/profiles/maria.jpg",
          "isAdmin": false,
          "relationship": "HIJO_A",
          "isActive": true
        }
      ],
      "extraMembers": [
        {
          "id": 1,
          "fullName": "Abuelo Pedro",
          "idType": "DNI",
          "photoUrl": "/uploads/extra/pedro.jpg",
          "description": "El abuelo de la familia",
          "isActive": true,
          "createdAt": "2026-06-06T01:00:00Z"
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

### Crear Grupo Familiar (`POST /api/family-groups`)

*   **Ruta:** `POST /api/family-groups`
*   **Nombre de Acción:** `CreateFamilyGroup`
*   **Autorización:** Pública (Autorización comentada en el enrutador actual).

#### Cuerpo de la Solicitud (FormData)
*   `name` (string, Requerido): Nombre del grupo familiar (máx. 200 caracteres).
*   `userId` (string, Opcional): ID del usuario propietario del grupo familiar.
*   `photo` (file, Opcional): Archivo de imagen de la foto de perfil o logo del grupo familiar.

> [!IMPORTANT]
> Este endpoint consume datos en formato `multipart/form-data` (FormData) para permitir la carga del archivo físico.


#### Respuesta Exitosa (`201 Created`)
Retorna los datos del grupo familiar recién creado, junto con la cabecera `Location` correspondiente:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Familia Pérez",
  "userId": "user-guid-id-1234",
  "isActive": true,
  "createdAt": "2026-06-06T01:50:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Error de validación (por ejemplo, si el nombre está vacío o supera los 200 caracteres).
    *   Ejemplo de validación:
        ```json
        {
          "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
          "title": "One or more validation errors occurred.",
          "status": 400,
          "errors": {
            "Name": [
              "'Name' must not be empty."
            ]
          }
        }
        ```

---

### Actualizar Grupo Familiar (`PUT /api/family-groups/{id}`)

*   **Ruta:** `PUT /api/family-groups/{id}`
*   **Nombre de Acción:** `UpdateFamilyGroup`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID del grupo familiar a actualizar.

#### Cuerpo de la Solicitud (FormData)
*   `name` (string, Requerido): Nombre del grupo familiar (máx. 200 caracteres).
*   `userId` (string, Opcional): ID del usuario propietario del grupo.
*   `photo` (file, Opcional): Archivo de imagen de la nueva foto de perfil o logo del grupo.

> [!IMPORTANT]
> Este endpoint consume datos en formato `multipart/form-data` (FormData) para permitir la carga del archivo físico.


#### Respuesta Exitosa (`200 OK`)
Retorna el recurso actualizado:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Familia Pérez Modificada",
  "userId": "user-guid-id-1234",
  "photoUrl": "/uploads/groups/perez_new.jpg",
  "isActive": true
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos o errores de validación de FluentValidation.
*   **`404 Not Found`**: Cuando el grupo familiar con el `id` provisto no existe.

---

### Activar o Inactivar Grupo Familiar (`PATCH /api/family-groups/{id}/toggle-status`)

*   **Ruta:** `PATCH /api/family-groups/{id}/toggle-status`
*   **Nombre de Acción:** `ToggleFamilyGroupStatus`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (guid, Requerido): ID del grupo familiar a alternar de estado.

#### Respuesta Exitosa (`200 OK`)
Retorna el nuevo estado lógico tras alternar su valor `isActive`:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Familia Pérez Modificada",
  "isActive": false,
  "status": "Desactivado"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: Cuando el grupo familiar con el `id` provisto no existe.
