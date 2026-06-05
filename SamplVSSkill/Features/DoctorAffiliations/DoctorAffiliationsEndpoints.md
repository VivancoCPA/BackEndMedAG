# Documentación de Endpoints de Afiliaciones de Médicos (Slice: DoctorAffiliations)

Esta documentación detalla de forma exhaustiva únicamente los endpoints de **Afiliaciones de Médicos a Centros Médicos** (`WithTags("Doctor Affiliations")`) que pertenecen al Slice Vertical de Afiliaciones de Médicos (`DoctorAffiliations`) de la aplicación.

---

## 📌 Tabla de Contenidos

- [Crear Afiliación (`POST /api/doctor-affiliations`)](#crear-afiliación-post-apidoctor-affiliations)
- [Listar Afiliaciones (`GET /api/doctor-affiliations`)](#listar-afiliaciones-get-apidoctor-affiliations)
- [Actualizar Afiliación (`PUT /api/doctor-affiliations/{id}`)](#actualizar-afiliación-put-apidoctor-affiliationsid)
- [Eliminar Afiliación (`DELETE /api/doctor-affiliations/{id}`)](#eliminar-afiliación-delete-apidoctor-affiliationsid)

---

### Crear Afiliación (`POST /api/doctor-affiliations`)

*   **Ruta:** `POST /api/doctor-affiliations`
*   **Nombre de Acción:** `CreateDoctorAffiliation`
*   **Autorización:** Pública.

#### Cuerpo de la Solicitud (JSON)
*   `doctorId` (guid, Requerido): ID del médico a afiliar.
*   `centerId` (guid, Requerido): ID del centro médico de destino.
*   `officeNumber` (string, Opcional): Número de consultorio, módulo u oficina asignada.
*   `workSchedule` (string, Opcional): Horario asignado (ej: "Lun-Mie 8am-12pm").

##### Ejemplo de Payload:
```json
{
  "doctorId": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
  "centerId": "018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8",
  "officeNumber": "Consultorio 402",
  "workSchedule": "Lun-Vie 9:00 AM - 1:00 PM"
}
```

#### Comportamiento
*   Valida que `doctorId` y `centerId` no estén vacíos.
*   Verifica si el médico ya se encuentra afiliado al centro indicado. Si ya existe la relación, rechaza la solicitud retornando `400 Bad Request`.

#### Respuesta Exitosa (`201 Created`)
Retorna el ID único autoincremental de la afiliación creada (entero), junto con la cabecera `Location` correspondiente:
```json
15
```

#### Otras Respuestas
*   **`400 Bad Request`**: Error de validación o conflicto de negocio (si el médico ya está afiliado al centro).
    *   Ejemplo de conflicto: `{"error": "El médico ya está afiliado a este centro."}`
*   **`401 Unauthorized`**: No se proporcionaron credenciales válidas.

---

### Listar Afiliaciones (`GET /api/doctor-affiliations`)

*   **Ruta:** `GET /api/doctor-affiliations`
*   **Nombre de Acción:** `ListDoctorAffiliations`
*   **Autorización:** Pública.
*   **Parámetros de Consulta (Query Params):**
    *   `doctorId` (guid, Opcional): Filtra las afiliaciones por el ID del médico.
    *   `centerId` (guid, Opcional): Filtra las afiliaciones por el ID del centro médico.

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<DoctorAffiliationItem>` ordenada por el nombre del centro médico y del médico:
```json
[
  {
    "id": 15,
    "doctorId": "018fdf9c-6a7b-7b0b-8d76-5fa42c9431f2",
    "doctorName": "Juan Perez",
    "centerId": "018fdf9c-6d2c-7b0b-8d76-6ee29c9431f8",
    "centerName": "Clínica San Felipe",
    "officeNumber": "Consultorio 402",
    "workSchedule": "Lun-Vie 9:00 AM - 1:00 PM",
    "createdAt": "2026-06-05T03:00:00Z"
  }
]
```

#### Otras Respuestas
*   **`401 Unauthorized`**: No autorizado.

---

### Actualizar Afiliación (`PUT /api/doctor-affiliations/{id}`)

*   **Ruta:** `PUT /api/doctor-affiliations/{id}`
*   **Nombre de Acción:** `UpdateDoctorAffiliation`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (int, Requerido): ID de la afiliación a actualizar.

#### Cuerpo de la Solicitud (JSON)
*   `officeNumber` (string, Opcional): Nuevo número de consultorio u oficina.
*   `workSchedule` (string, Opcional): Nuevo horario de atención.

##### Ejemplo de Payload:
```json
{
  "officeNumber": "Consultorio 402-A",
  "workSchedule": "Lun-Vie 2:00 PM - 6:00 PM"
}
```

#### Respuesta Exitosa (`204 No Content`)
La actualización se realizó con éxito y no se retorna cuerpo en la respuesta.

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos en el cuerpo.
*   **`404 Not Found`**: No se encuentra una afiliación con el `id` provisto.
*   **`401 Unauthorized`**: No autorizado.

---

### Eliminar Afiliación (`DELETE /api/doctor-affiliations/{id}`)

*   **Ruta:** `DELETE /api/doctor-affiliations/{id}`
*   **Nombre de Acción:** `DeleteDoctorAffiliation`
*   **Autorización:** Pública.
*   **Parámetros de Ruta:**
    *   `id` (int, Requerido): ID de la afiliación a eliminar de la base de datos.

#### Respuesta Exitosa (`204 No Content`)
La afiliación se eliminó correctamente del sistema.

#### Otras Respuestas
*   **`404 Not Found`**: No se encuentra una afiliación con el `id` provisto.
*   **`401 Unauthorized`**: No autorizado.
