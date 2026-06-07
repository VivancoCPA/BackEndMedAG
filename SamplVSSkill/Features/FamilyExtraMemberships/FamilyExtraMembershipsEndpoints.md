# Documentación de Endpoints de Miembros Extras (Slice: FamilyExtraMemberships)

Esta documentación detalla los endpoints de **Gestión de Miembros Extras de Grupos Familiares** (`WithTags("Family Extra Memberships")`) que pertenecen al Slice Vertical de Miembros Extras (`FamilyExtraMemberships`) de la aplicación.

---

## 📌 Tabla de Contenidos

- [Listar Miembros Extras de un Grupo Familiar (`GET /api/family-groups/{familyGroupId}/extra-members`)](#listar-miembros-extras-de-un-grupo-familiar-get-apifamily-groupsfamilygroupidextra-members)
- [Crear Miembro Extra en Grupo Familiar (`POST /api/family-groups/{familyGroupId}/extra-members`)](#crear-miembro-extra-en-grupo-familiar-post-apifamily-groupsfamilygroupidextra-members)
- [Actualizar Miembro Extra (`PUT /api/family-groups/{familyGroupId}/extra-members/{id}`)](#actualizar-miembro-extra-put-apifamily-groupsfamilygroupidextra-membersid)
- [Desvincular y Eliminar Miembro Extra (`DELETE /api/family-groups/{familyGroupId}/extra-members/{id}`)](#desvincular-y-eliminar-miembro-extra-delete-apifamily-groupsfamilygroupidextra-membersid)
- [Activar o Inactivar Miembro Extra (`PATCH /api/family-groups/{familyGroupId}/extra-members/{id}/toggle-status`)](#activar-o-inactivar-miembro-extra-patch-apifamily-groupsfamilygroupidextra-membersidtoggle-status)

---

### Listar Miembros Extras de un Grupo Familiar (`GET /api/family-groups/{familyGroupId}/extra-members`)

*   **Ruta:** `GET /api/family-groups/{familyGroupId}/extra-members`
*   **Nombre de Acción:** `ListFamilyExtraMemberships`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `familyGroupId` (guid, Requerido): ID del grupo familiar del cual se desea listar los miembros extras.

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<FamilyExtraMembershipItem>` ordenada alfabéticamente por nombre completo:
```json
[
  {
    "id": 1,
    "fullName": "Abuelo Pedro",
    "idType": "DNI",
    "photoUrl": "/uploads/extra-members/pedro.jpg",
    "familyGroupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "description": "El abuelo de la familia",
    "isActive": true,
    "createdAt": "2026-06-06T01:00:00Z"
  }
]
```

#### Otras Respuestas
*   **`404 Not Found`**: Si el grupo familiar con el `familyGroupId` provisto no existe.

---

### Crear Miembro Extra en Grupo Familiar (`POST /api/family-groups/{familyGroupId}/extra-members`)

*   **Ruta:** `POST /api/family-groups/{familyGroupId}/extra-members`
*   **Nombre de Acción:** `CreateFamilyExtraMembership`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `familyGroupId` (guid, Requerido): ID del grupo familiar al cual asociar al miembro extra.

#### Cuerpo de la Solicitud (Multipart/Form-Data)
*   `FullName` (string, Requerido): Nombre completo del miembro extra. Máx. 200 caracteres.
*   `IdType` (string, Requerido): Tipo de documento identificatorio (DNI, pasaporte, etc.). Máx. 50 caracteres.
*   `Description` (string, Opcional): Descripción o nota adicional sobre el miembro.
*   `Photo` (file, Opcional): Archivo físico de imagen (foto de perfil).

> [!IMPORTANT]
> Al consumir este endpoint desde aplicaciones cliente (como React/TypeScript), se debe enviar la petición utilizando `FormData` y **no declarar manualmente la cabecera 'Content-Type'**, permitiendo que el navegador establezca el tipo multipart correspondiente con el límite de separación.

#### Respuesta Exitosa (`201 Created`)
Retorna los datos del miembro extra recién creado:
```json
{
  "id": 1,
  "fullName": "Abuelo Pedro",
  "idType": "DNI",
  "photoUrl": "/uploads/extra-members/pedro-guid.jpg",
  "familyGroupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "El abuelo de la familia",
  "isActive": true,
  "createdAt": "2026-06-06T01:00:00Z"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Errores de validación (por ejemplo, si `FullName` o `IdType` están vacíos).
*   **`404 Not Found`**: Si el grupo familiar especificado no existe.
*   **`500 Internal Server Error`**: Si ocurre un error al almacenar el archivo físico de la foto en el disco local del servidor (activa un rollback automático eliminando el registro de base de datos recién creado).

---

### Actualizar Miembro Extra (`PUT /api/family-groups/{familyGroupId}/extra-members/{id}`)

*   **Ruta:** `PUT /api/family-groups/{familyGroupId}/extra-members/{id}`
*   **Nombre de Acción:** `UpdateFamilyExtraMembership`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `familyGroupId` (guid, Requerido): ID del grupo familiar.
    *   `id` (int, Requerido): ID único del miembro extra.

#### Cuerpo de la Solicitud (Multipart/Form-Data)
*   `FullName` (string, Requerido): Nombre completo del miembro extra. Máx. 200 caracteres.
*   `IdType` (string, Requerido): Tipo de documento identificatorio. Máx. 50 caracteres.
*   `Description` (string, Opcional): Descripción o nota adicional.
*   `Photo` (file, Opcional): Nuevo archivo físico de imagen a subir.
*   `IsActive` (bool, Requerido): Estado activo/inactivo del miembro extra.

#### Respuesta Exitosa (`200 OK`)
Retorna el recurso actualizado:
```json
{
  "id": 1,
  "fullName": "Abuelo Pedro Modificado",
  "idType": "DNI",
  "photoUrl": "/uploads/extra-members/pedro-new-guid.jpg",
  "familyGroupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "El abuelo de la familia modificado",
  "isActive": true
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos o errores de validación en el cuerpo.
*   **`404 Not Found`**: Si el grupo familiar o el miembro extra con el `id` provisto no existen en el grupo.
*   **`500 Internal Server Error`**: Si ocurre un error al guardar la nueva foto físicamente (activa un rollback restaurando el path de la foto anterior en base de datos).

---

### Desvincular y Eliminar Miembro Extra (`DELETE /api/family-groups/{familyGroupId}/extra-members/{id}`)

*   **Ruta:** `DELETE /api/family-groups/{familyGroupId}/extra-members/{id}`
*   **Nombre de Acción:** `RemoveFamilyExtraMembership`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `familyGroupId` (guid, Requerido): ID del grupo familiar.
    *   `id` (int, Requerido): ID único del miembro extra a eliminar.

#### Respuesta Exitosa (`200 OK`)
Retorna los detalles de la desvinculación:
```json
{
  "id": 1,
  "familyGroupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Miembro extra desvinculado y eliminado del grupo familiar exitosamente."
}
```

#### Otras Respuestas
*   **`404 Not Found`**: Si el grupo familiar o el miembro extra con el `id` provisto no existen.

---

### Activar o Inactivar Miembro Extra (`PATCH /api/family-groups/{familyGroupId}/extra-members/{id}/toggle-status`)

*   **Ruta:** `PATCH /api/family-groups/{familyGroupId}/extra-members/{id}/toggle-status`
*   **Nombre de Acción:** `ToggleFamilyExtraMembershipStatus`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `familyGroupId` (guid, Requerido): ID del grupo familiar.
    *   `id` (int, Requerido): ID único del miembro extra a alternar de estado.

#### Respuesta Exitosa (`200 OK`)
Retorna el nuevo estado lógico tras alternar su valor `isActive`:
```json
{
  "id": 1,
  "fullName": "Abuelo Pedro",
  "isActive": false,
  "status": "Desactivado"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: Si el grupo familiar o el miembro extra con el `id` provisto no existen.

