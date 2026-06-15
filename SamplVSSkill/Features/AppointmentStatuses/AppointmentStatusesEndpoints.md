# Documentación de Endpoints de Estados de Cita (Slice: AppointmentStatuses)

Esta documentación detalla el endpoint para listar los posibles estados de las citas médicas.

---

## Listar Todos los Estados de Cita (`GET /api/appointment-statuses`)

*   **Ruta:** `GET /api/appointment-statuses`
*   **Nombre de Acción:** `ListAppointmentStatuses`
*   **Autorización:** No requiere autenticación.
*   **Descripción:** Retorna el listado completo de estados de cita cargado desde el archivo JSON de configuración.

#### Respuesta Exitosa (`200 OK`)
Devuelve un listado `IEnumerable<AppointmentStatusResponse>` con los estados disponibles:

```json
[
  {
    "id": "PENDIENTE",
    "label": "Pendiente"
  },
  {
    "id": "CONFIRMADA",
    "label": "Confirmada"
  },
  {
    "id": "INASISTENCIA",
    "label": "Inasistencia"
  },
  {
    "id": "CANCELADA",
    "label": "Cancelada"
  },
  {
    "id": "REPROGRAMADA",
    "label": "Reprogramada"
  },
  {
    "id": "ENCONSULTA",
    "label": "En Consulta"
  },
  {
    "id": "FINALIZADA",
    "label": "Finalizada"
  }
]
```
