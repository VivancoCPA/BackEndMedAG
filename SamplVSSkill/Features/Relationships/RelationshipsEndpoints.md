# Documentación de Endpoints de Parentescos (Slice: Relationships)

Esta documentación detalla el endpoint del Slice Vertical de Parentescos (`Relationships`) de la aplicación, el cual expone los tipos de relaciones familiares disponibles desde un origen de datos estático (`parentescos.json`).

---

## 📌 Tabla de Contenidos

- [Listar Todos los Parentescos (`GET /api/relationships`)](#listar-todos-los-parentescos-get-apirelationships)

---

### Listar Todos los Parentescos (`GET /api/relationships`)

*   **Ruta:** `GET /api/relationships`
*   **Nombre de Acción:** `ListRelationships`
*   **Autorización:** Pública (Acceso libre, enrutamiento público).
*   **Origen de Datos**: `Domain/Raw/parentescos.json`

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<RelationshipResponse>` de los parentescos configurados:
```json
[
  {
    "id": "MADRE",
    "label": "Madre"
  },
  {
    "id": "PADRE",
    "label": "Padre"
  },
  {
    "id": "HIJO_A",
    "label": "Hijo/a"
  },
  {
    "id": "HERMANO_A",
    "label": "Hermano/a"
  },
  {
    "id": "CONYUGE",
    "label": "Cónyuge"
  }
]
```

#### Otras Respuestas
*   **`500 Internal Server Error`**: Si el archivo origen `parentescos.json` no existe en la ruta especificada.
