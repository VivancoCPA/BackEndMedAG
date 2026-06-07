# Documentación de Endpoints de Membresías Familiares (Slice: FamilyMemberships)

Esta documentación detalla los endpoints de **Gestión de Membresías Familiares** (`WithTags("Family Memberships")`) que pertenecen al Slice Vertical de Membresías Familiares (`FamilyMemberships`) de la aplicación.

---

## 📌 Tabla de Contenidos

- [Listar Miembros de un Grupo Familiar (`GET /api/family-groups/{familyGroupId}/members`)](#listar-miembros-de-un-grupo-familiar-get-apifamily-groupsfamilygroupidmembers)
- [Asignar Miembro a Grupo Familiar (`POST /api/family-groups/{familyGroupId}/members`)](#asignar-miembro-a-grupo-familiar-post-apifamily-groupsfamilygroupidmembers)
- [Desvincular Miembro de Grupo Familiar (`DELETE /api/family-groups/{familyGroupId}/members/{userId}`)](#desvincular-miembro-de-grupo-familiar-delete-apifamily-groupsfamilygroupidmembersuserid)

---

### Listar Miembros de un Grupo Familiar (`GET /api/family-groups/{familyGroupId}/members`)

*   **Ruta:** `GET /api/family-groups/{familyGroupId}/members`
*   **Nombre de Acción:** `ListFamilyMemberships`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `familyGroupId` (guid, Requerido): ID del grupo familiar del cual se desea listar los miembros.

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<FamilyMembershipItem>` de los miembros registrados asociados al grupo:
```json
[
  {
    "id": 1,
    "userId": "user-guid-id-5678",
    "userEmail": "maria@example.com",
    "userName": "María",
    "userLastName": "Pérez",
    "userPhotoUrl": "/uploads/profiles/maria.jpg",
    "isAdmin": false,
    "relationship": "HIJO_A"
  }
]
```

#### Otras Respuestas
*   **`404 Not Found`**: Si el grupo familiar con el `familyGroupId` provisto no existe.
    *   Ejemplo de mensaje: `"Grupo familiar '3fa85f64-5717-4562-b3fc-2c963f66afa6' no encontrado."`

---

### Asignar Miembro a Grupo Familiar (`POST /api/family-groups/{familyGroupId}/members`)

*   **Ruta:** `POST /api/family-groups/{familyGroupId}/members`
*   **Nombre de Acción:** `AssignFamilyMembership`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `familyGroupId` (guid, Requerido): ID del grupo familiar al cual asociar al usuario.

#### Cuerpo de la Solicitud (JSON)
*   `userId` (string, Requerido): ID del usuario a asignar como miembro.
*   `isAdmin` (bool, Requerido): Determina si el usuario será administrador del grupo familiar.
*   `relationship` (string, Requerido): Tipo de relación familiar (por ejemplo: `MADRE`, `PADRE`, `HIJO_A`, `HERMANO_A`, `CONYUGE`, `SOBRINO_A`). Máx. 100 caracteres.

##### Ejemplo de Payload:
```json
{
  "userId": "user-guid-id-5678",
  "isAdmin": false,
  "relationship": "HIJO_A"
}
```

#### Respuesta Exitosa (`201 Created`)
Retorna los datos de la membresía familiar recién creada:
```json
{
  "id": 2,
  "userId": "user-guid-id-5678",
  "familyGroupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isAdmin": false,
  "relationship": "HIJO_A"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Errores de validación (por ejemplo, si el `userId` o `relationship` están vacíos).
*   **`404 Not Found`**: Si el grupo familiar o el usuario especificados no existen.
*   **`409 Conflict`**: Si el usuario ya pertenece a algún grupo familiar (incluyendo el grupo actual).
    *   Ejemplo de mensaje: `"El usuario ya pertenece a un grupo familiar."`

> [!IMPORTANT]
> Un usuario únicamente puede pertenecer a **un solo grupo familiar a la vez**. Si el usuario ya tiene una membresía activa en cualquier grupo, la asignación fallará retornando `409 Conflict`.

---

### Desvincular Miembro de Grupo Familiar (`DELETE /api/family-groups/{familyGroupId}/members/{userId}`)

*   **Ruta:** `DELETE /api/family-groups/{familyGroupId}/members/{userId}`
*   **Nombre de Acción:** `RemoveFamilyMembership`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `familyGroupId` (guid, Requerido): ID del grupo familiar.
    *   `userId` (string, Requerido): ID del usuario que se va a desvincular.

#### Respuesta Exitosa (`200 OK`)
Retorna los detalles de la desvinculación:
```json
{
  "userId": "user-guid-id-5678",
  "familyGroupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Miembro desvinculado del grupo familiar exitosamente."
}
```

#### Otras Respuestas
*   **`404 Not Found`**: Si la membresía no existe (el usuario no pertenece al grupo especificado).
    *   Ejemplo de mensaje: `"El usuario 'user-guid-id-5678' no es miembro del grupo familiar '3fa85f64-5717-4562-b3fc-2c963f66afa6'."`
