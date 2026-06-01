# Documentación de Endpoints de Usuario (Slice: Auth)

Esta documentación detalla de forma exhaustiva únicamente los endpoints de **Gestión de Usuario** (`WithTags("Users")`) y los endpoints de **Flujo y Autenticación del Usuario** (`WithTags("Auth")`) que pertenecen al Slice Vertical de Autenticación (`Auth`) de la aplicación. 

Se excluyen explícitamente los endpoints de gestión de roles que no operan sobre un usuario individual (como la creación de roles `/api/roles` o eliminación global de los mismos).

---

## 📌 Tabla de Contenidos

- [1. Gestión de Perfil de Usuario (Tag: `Users`)](#1-gestión-de-perfil-de-usuario-tag-users)
  - [Obtener Usuario por ID (`GET /api/users/{userId}`)](#obtener-usuario-por-id-get-apiusersuserid)
  - [Listar Todos los Usuarios (`GET /api/users`)](#listar-todos-los-usuarios-get-apiusers)
  - [Listar Usuarios Paginados (`GET /api/auth/users/paged`)](#listar-usuarios-paginados-get-apiauthuserspaged)
  - [Actualizar Información de Usuario (`PUT /api/auth/users/{id}`)](#actualizar-información-de-usuario-put-apiauthusersid)
  - [Activar o Bloquear Usuario (`PATCH /api/users/{userId}/toggle-status`)](#activar-o-bloquear-usuario-patch-apiusersuseridtoggle-status)
  - [Creación Administrativa de Usuario (`POST /api/auth/users`)](#creación-administrativa-de-usuario-post-apiauthusers)
- [2. Gestión de Roles del Usuario (Tag: `Users`)](#2-gestión-de-roles-del-usuario-tag-users)
  - [Obtener Roles de un Usuario (`GET /api/users/{userId}/roles`)](#obtener-roles-de-un-usuario-get-apiusersuseridroles)
  - [Asignar Rol a un Usuario (`POST /api/users/{userId}/roles`)](#asignar-rol-a-un-usuario-post-apiusersuseridroles)
  - [Remover Rol de un Usuario (`DELETE /api/users/{userId}/roles/{roleName}`)](#remover-rol-de-un-usuario-delete-apiusersuseridrolesrolename)
- [3. Gestión de Claims del Usuario (Tag: `Users`)](#3-gestión-de-claims-del-usuario-tag-users)
  - [Obtener Claims de un Usuario (`GET /api/users/{userId}/claims`)](#obtener-claims-de-un-usuario-get-apiusersuseridclaims)
  - [Asignar Claim a un Usuario (`POST /api/users/{userId}/claims`)](#asignar-claim-a-un-usuario-post-apiusersuseridclaims)
  - [Remover Claim de un Usuario (`DELETE /api/users/{userId}/claims`)](#remover-claim-de-un-usuario-delete-apiusersuseridclaims)
- [4. Endpoints de Autenticación y Cuenta (Tag: `Auth`)](#4-endpoints-de-autenticación-y-cuenta-tag-auth)
  - [Registro de Usuario (`POST /api/auth/register`)](#registro-de-usuario-post-apiauthregister)
  - [Inicio de Sesión / Login (`POST /api/auth/login`)](#inicio-de-sesión--login-post-apiauthlogin)
  - [Cambio de Contraseña (`POST /api/auth/change-password`)](#cambio-de-contraseña-post-apiauthchange-password)
  - [Solicitud de Recuperación de Contraseña (`POST /api/auth/forgot-password`)](#solicitud-de-recuperación-de-contraseña-post-apiauthforgot-password)
  - [Restablecer Contraseña (`POST /api/auth/reset-password`)](#restablecer-contraseña-post-apiauthreset-password)
  - [Renovación de Token / Refresh Token (`POST /api/auth/refresh`)](#renovación-de-token--refresh-token-post-apiauthrefresh)

---

## 1. Gestión de Perfil de Usuario (Tag: `Users`)

### Obtener Usuario por ID (`GET /api/users/{userId}`)

*   **Ruta:** `GET /api/users/{userId}`
*   **Nombre de Acción:** `GetUser`
*   **Autorización:** Requerido (`.RequireAuthorization()`)
*   **Parámetros de Ruta:**
    *   `userId` (string, Requerido): ID único del usuario a consultar.

#### Respuesta Exitosa (`200 OK`)
Devuelve un objeto `GetUserResponse` con la información detallada del perfil:

```json
{
  "id": "string",
  "email": "string",
  "name": "string",
  "lastName": "string",
  "dateOfBirth": "2026-05-22T00:00:00Z",
  "emailConfirmed": true,
  "isLockedOut": false,
  "lockoutEnd": null,
  "passwordConfirmed": true,
  "lastAccess": "2026-06-01T09:46:00Z",
  "roles": ["string"],
  "claims": ["TipoClaim:ValorClaim"]
}
```

#### Otras Respuestas
*   **`401 Unauthorized`**: El usuario no ha proporcionado credenciales de autenticación válidas.
*   **`404 Not Found`**: No se encuentra un usuario con el `userId` provisto.

---

### Listar Todos los Usuarios (`GET /api/users`)

*   **Ruta:** `GET /api/users`
*   **Nombre de Acción:** `ListUsers`
*   **Autorización:** Ninguna (Comentada en el mapeo del endpoint).

#### Respuesta Exitosa (`200 OK`)
Retorna una lista `IEnumerable<ListUsersResponse>` con todos los usuarios registrados y su grupo familiar/aseguradoras asociadas (enfoque optimizado mediante Dapper en 2 consultas):

```json
[
  {
    "id": "string",
    "email": "string",
    "name": "string",
    "lastName": "string",
    "phoneNumber": "string",
    "dateOfBirth": "2026-05-22T00:00:00Z",
    "photoUrl": "string",
    "address": "string",
    "emailConfirmed": true,
    "isLockedOut": false,
    "lastAccess": "2026-06-01T09:46:00Z",
    "passwordConfirmed": true,
    "familyGroupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "familyGroupName": "string",
    "insurances": [
      {
        "insurerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "insurerName": "string",
        "insurerPhone": "string",
        "insurerEmail": "string",
        "logoUrl": "string"
      }
    ]
  }
]
```

---

### Listar Usuarios Paginados (`GET /api/auth/users/paged`)

*   **Ruta:** `GET /api/auth/users/paged`
*   **Nombre de Acción:** `PagedUsers`
*   **Autorización:** Ninguna (Comentada en el mapeo del endpoint).
*   **Parámetros de Consulta (Query Params):**
    *   `page` (int, opcional, por defecto `1`): Número de página a consultar.
    *   `pageSize` (int, opcional, por defecto `10`): Cantidad máxima de registros por página (Min: 1, Max: 100).
    *   `search` (string, opcional, por defecto `null`): Filtra por coincidencia parcial (ILIKE) en los campos: Email, Nombre, Apellido o Nombre de Grupo Familiar.
    *   `sortBy` (string, opcional, por defecto `"name"`): Campo por el que ordenar. Opciones admitidas: `"name"`, `"lastname"`, `"email"`, `"emailconfirmed"`, `"createdat"`.
    *   `sortDesc` (bool, opcional, por defecto `false`): Si se establece en `true` ordena de manera descendente.

#### Respuesta Exitosa (`200 OK`)
Retorna un objeto `PaginatedResult<PagedUserItem>` que incluye metadatos de la paginación:

```json
{
  "items": [
    {
      "id": "string",
      "email": "string",
      "name": "string",
      "lastName": "string",
      "phoneNumber": "string",
      "dateOfBirth": "2026-05-22",
      "photoUrl": "string",
      "address": "string",
      "emailConfirmed": true,
      "isLockedOut": false,
      "createdAt": "2026-05-22T18:24:27Z",
      "lastAccess": "2026-06-01T09:46:00Z",
      "passwordConfirmed": true,
      "familyGroupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "familyGroupName": "string",
      "insurances": [
        {
          "insurerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "insurerName": "string",
          "insurerPhone": "string",
          "insurerEmail": "string",
          "logoUrl": "string"
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

### Actualizar Información de Usuario (`PUT /api/auth/users/{id}`)

*   **Ruta:** `PUT /api/auth/users/{id}`
*   **Nombre de Acción:** `UpdateUser`
*   **Autorización:** Ninguna (Comentada en el mapeo del endpoint).
*   **Parámetros de Ruta:**
    *   `id` (string, Requerido): ID único del usuario a actualizar.

#### Cuerpo de la Solicitud (Request Body - JSON)
Objeto `UpdateUserCommand` con los campos actualizables:
```json
{
  "name": "Juan",
  "lastName": "Pérez",
  "dateOfBirth": "1990-05-15",
  "phoneNumber": "+1234567890",
  "photoUrl": "https://example.com/avatar.jpg",
  "address": "Calle Falsa 123"
}
```
*   **Validaciones:**
    *   `name`: Obligatorio, longitud máxima 100 caracteres.
    *   `lastName`: Obligatorio, longitud máxima 100 caracteres.
    *   `dateOfBirth`: Opcional, debe tener un formato de fecha válido `yyyy-MM-dd`.

#### Respuesta Exitosa (`200 OK`)
Retorna `UpdateUserResponse` confirmando los cambios realizados:
```json
{
  "id": "string",
  "email": "string",
  "name": "Juan",
  "lastName": "Pérez",
  "dateOfBirth": "1990-05-15T00:00:00Z",
  "phoneNumber": "+1234567890",
  "photoUrl": "https://example.com/avatar.jpg",
  "address": "Calle Falsa 123"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos en el cuerpo (problema de validación).
*   **`404 Not Found`**: El usuario no existe en el sistema.

---

### Activar o Bloquear Usuario (`PATCH /api/users/{userId}/toggle-status`)

*   **Ruta:** `PATCH /api/users/{userId}/toggle-status`
*   **Nombre de Acción:** `ToggleUserStatus`
*   **Autorización:** Ninguna (Comentada en el mapeo del endpoint).
*   **Parámetros de Ruta:**
    *   `userId` (string, Requerido): ID único del usuario.

#### Comportamiento
*   Si el usuario está bloqueado (`LockoutEnd > UTC Now`), se **desbloquea** limpiando el campo `LockoutEnd` a `null`.
*   Si el usuario está activo, se **bloquea** asignándole un bloqueo permanente fijando `LockoutEnd` en `DateTimeOffset.MaxValue`.

#### Respuesta Exitosa (`200 OK`)
Retorna `ToggleUserStatusResponse` con el estado final:
```json
{
  "userId": "string",
  "email": "string",
  "isLockedOut": true,
  "status": "Bloqueado" // O "Activado"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: No se encontró al usuario.

---

### Creación Administrativa de Usuario (`POST /api/auth/users`)

*   **Ruta:** `POST /api/auth/users`
*   **Nombre de Acción:** `CreateUser`
*   **Autorización:** Ninguna (Omitido por desarrollo, se puede proteger con Roles/Admins luego).

#### Cuerpo de la Solicitud (FormData / `multipart/form-data`)
La petición debe enviarse codificada como formulario (`multipart/form-data`) con los siguientes campos:

*   `email` (string, Requerido): Correo electrónico del usuario. Debe ser único y tener formato de dirección válido.
*   `name` (string, Requerido): Nombre del usuario. Máx. 100 caracteres.
*   `lastName` (string, Requerido): Apellido del usuario. Máx. 100 caracteres.
*   `phone` (string, Opcional): Teléfono del usuario.
*   `dateOfBirth` (string / DateTime, Opcional): Fecha de nacimiento (ej. `1995-08-25`).
*   `photo` (file / IFormFile, Opcional): Archivo de imagen de la foto de perfil.

> [!IMPORTANT]
> **Recomendación para el FrontEnd (React / JS / TS)**:
> Al invocar este endpoint utilizando un objeto `FormData`, **NO debes definir manualmente la cabecera `'Content-Type': 'multipart/form-data'`** en las cabeceras (`headers`) de tu cliente HTTP (`fetch` o `axios`). 
> Al pasar el objeto `FormData` como cuerpo de la petición, el navegador asignará y gestionará de manera automática la cabecera `multipart/form-data` e inyectará el parámetro dinámico de separación `boundary`. Si lo declaras a mano, la petición fallará en el servidor por ausencia de delimitadores.

#### Comportamiento
*   Verifica que el email no esté en uso.
*   Autogenera una contraseña temporal segura que cumple con las directivas de complejidad de Identity.
*   **Gestión de Foto de Perfil**: Si se proporciona un archivo de imagen en la propiedad `photo`, se crea recursivamente la carpeta local `wwwroot/uploads/profiles/` en el servidor (si no existe), se le genera un nombre seguro y único usando un GUID para evitar colisiones y path traversal, se escribe físicamente el archivo en disco, y se almacena la ruta de acceso relativa (ej. `/uploads/profiles/a5b6c7d8e9f0...jpg`) en el campo `PhotoUrl` del registro de usuario. Si se produce un error durante la creación definitiva del registro en la base de datos, el archivo cargado se elimina de forma automática y preventiva para evitar almacenamiento basura.
*   Registra el usuario con `EmailConfirmed = true` (ya confirmado automáticamente por el administrador) y `PasswordConfirmed = false` (indica que debe ser cambiada en el primer ingreso).
*   Envía un correo de bienvenida al usuario final con sus credenciales y contraseña temporal.

#### Respuesta Exitosa (`201 Created`)
Retorna `CreateUserResponse` con los detalles del usuario creado e incluye la ruta de acceso a la foto cargada:
```json
{
  "id": "d748f65e-2b1a-42c3-98fe-d27e7fcd61a2",
  "email": "nuevo.usuario@example.com",
  "name": "Juan",
  "lastName": "Pérez",
  "photoUrl": "/uploads/profiles/7a2be748f65e2b1a42c398fed27e7fcd.jpg",
  "passwordConfirmed": false
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Datos inválidos en el formulario enviado (problema de validación).
*   **`409 Conflict`**: Si el email provisto ya se encuentra registrado.

---

## 2. Gestión de Roles del Usuario (Tag: `Users`)

### Obtener Roles de un Usuario (`GET /api/users/{userId}/roles`)

*   **Ruta:** `GET /api/users/{userId}/roles`
*   **Nombre de Acción:** `GetUserRoles`
*   **Parámetros de Ruta:**
    *   `userId` (string, Requerido): ID único del usuario.

#### Respuesta Exitosa (`200 OK`)
Retorna la lista de roles asociados al usuario (`GetUserRolesResponse`):
```json
{
  "userId": "string",
  "email": "string",
  "roles": ["Admin", "User"]
}
```

#### Otras Respuestas
*   **`404 Not Found`**: El usuario no existe.

---

### Asignar Rol a un Usuario (`POST /api/users/{userId}/roles`)

*   **Ruta:** `POST /api/users/{userId}/roles`
*   **Nombre de Acción:** `AssignRole`
*   **Parámetros de Ruta:**
    *   `userId` (string, Requerido): ID único del usuario.

#### Cuerpo de la Solicitud (Request Body - JSON)
```json
{
  "roleName": "Admin"
}
```

#### Respuesta Exitosa (`200 OK`)
Asigna el rol especificado y retorna el listado completo actualizado de los roles del usuario (`AssignRoleResponse`):
```json
{
  "userId": "string",
  "email": "string",
  "roles": ["User", "Admin"]
}
```

#### Otras Respuestas
*   **`404 Not Found`**: El usuario no existe.
*   **`409 Conflict`**: El usuario ya posee asignado el rol provisto en `roleName`.

---

### Remover Rol de un Usuario (`DELETE /api/users/{userId}/roles/{roleName}`)

*   **Ruta:** `DELETE /api/users/{userId}/roles/{roleName}`
*   **Nombre de Acción:** `RemoveRole`
*   **Parámetros de Ruta:**
    *   `userId` (string, Requerido): ID único del usuario.
    *   `roleName` (string, Requerido): Nombre del rol a remover.

#### Respuesta Exitosa (`200 OK`)
Remueve el rol especificado del usuario y retorna `RemoveRoleResponse` con los roles restantes del mismo:
```json
{
  "userId": "string",
  "email": "string",
  "remainingRoles": ["User"]
}
```

#### Otras Respuestas
*   **`404 Not Found`**: El usuario no existe, o el usuario no posee asignado el rol especificado.

---

## 3. Gestión de Claims del Usuario (Tag: `Users`)

### Obtener Claims de un Usuario (`GET /api/users/{userId}/claims`)

*   **Ruta:** `GET /api/users/{userId}/claims`
*   **Nombre de Acción:** `GetUserClaims`
*   **Parámetros de Ruta:**
    *   `userId` (string, Requerido): ID único del usuario.

#### Respuesta Exitosa (`200 OK`)
Retorna `GetUserClaimsResponse` con los claims asociados:
```json
{
  "userId": "string",
  "email": "string",
  "claims": [
    {
      "type": "Permission",
      "value": "ViewReports"
    }
  ]
}
```

#### Otras Respuestas
*   **`404 Not Found`**: El usuario no existe.

---

### Asignar Claim a un Usuario (`POST /api/users/{userId}/claims`)

*   **Ruta:** `POST /api/users/{userId}/claims`
*   **Nombre de Acción:** `AssignClaim`
*   **Parámetros de Ruta:**
    *   `userId` (string, Requerido): ID del usuario.

#### Cuerpo de la Solicitud (Request Body - JSON)
```json
{
  "claimType": "Permission",
  "claimValue": "ViewReports"
}
```
*   **Validaciones:**
    *   `claimType`: Obligatorio, máximo 100 caracteres.
    *   `claimValue`: Obligatorio, máximo 256 caracteres.

#### Respuesta Exitosa (`201 Created`)
Registra el nuevo Claim en la tabla de ASP.NET Identity y retorna `AssignClaimResponse`:
```json
{
  "userId": "string",
  "email": "string",
  "claimType": "Permission",
  "claimValue": "ViewReports"
}
```

#### Otras Respuestas
*   **`404 Not Found`**: El usuario no existe.
*   **`409 Conflict`**: El usuario ya cuenta con ese claim asignado.

---

### Remover Claim de un Usuario (`DELETE /api/users/{userId}/claims`)

*   **Ruta:** `DELETE /api/users/{userId}/claims`
*   **Nombre de Acción:** `RemoveClaim`
*   **Parámetros de Ruta:**
    *   `userId` (string, Requerido): ID del usuario.
*   **Parámetros de Consulta (Query Params):**
    *   `claimType` (string, Requerido): Tipo de claim.
    *   `claimValue` (string, Requerido): Valor de claim.

#### Respuesta Exitosa (`200 OK`)
Remueve el claim especificado y retorna `RemoveClaimResponse`:
```json
{
  "userId": "string",
  "email": "string",
  "message": "Claim eliminado correctamente."
}
```

#### Otras Respuestas
*   **`404 Not Found`**: El usuario no existe, o no se encontró una coincidencia para el par `claimType` / `claimValue` en los claims actuales del usuario.

---

## 4. Endpoints de Autenticación y Cuenta (Tag: `Auth`)

Estos endpoints forman parte integral de la gestión e identidad del usuario, controlando el ciclo de autenticación y seguridad de su cuenta.

### Registro de Usuario (`POST /api/auth/register`)

*   **Ruta:** `POST /api/auth/register`
*   **Nombre de Acción:** `Register`
*   **Autorización:** Ninguna.

#### Cuerpo de la Solicitud (Request Body - JSON)
```json
{
  "name": "Juan",
  "lastName": "Pérez",
  "email": "juan.perez@example.com",
  "password": "SecurePassword123!",
  "phone": "+1234567890",
  "dateOfBirth": "1990-05-15"
}
```
*   **Validaciones:**
    *   `name`: Obligatorio, longitud máxima 100 caracteres.
    *   `lastName`: Obligatorio, longitud máxima 100 caracteres.
    *   `email`: Obligatorio, formato de email válido y único.
    *   `password`: Obligatorio. Mínimo 8 caracteres, al menos una mayúscula, una minúscula, un dígito y un carácter especial.
    *   `dateOfBirth`: Opcional, debe tener un formato de fecha válido `yyyy-MM-dd`.

#### Respuesta Exitosa (`201 Created`)
Genera la cuenta en la base de datos, autocalcula un token de sesión JWT, genera un refresh token y retorna `RegisterResponse`:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "d2FudGVkX3JlZnJlc2hfdG9rZW5fYmFzZTY0...",
  "email": "juan.perez@example.com",
  "name": "Juan",
  "lastName": "Pérez"
}
```

---

### Inicio de Sesión / Login (`POST /api/auth/login`)

*   **Ruta:** `POST /api/auth/login`
*   **Nombre de Acción:** `Login`
*   **Autorización:** Ninguna.

#### Cuerpo de la Solicitud (Request Body - JSON)
```json
{
  "email": "juan.perez@example.com",
  "password": "SecurePassword123!"
}
```

#### Respuesta Exitosa (`200 OK`)
Retorna `LoginResponse` incluyendo el JWT generado y el Refresh Token persistido:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "d2FudGVkX3JlZnJlc2hfdG9rZW5fYmFzZTY0...",
  "email": "juan.perez@example.com",
  "name": "Juan",
  "lastName": "Pérez",
  "passwordConfirmed": true
}
```

#### Otras Respuestas
*   **`401 Unauthorized`**: Credenciales de inicio de sesión erróneas o cuenta inexistente (mensaje: `"Credenciales inválidas"`).
*   **`403 Forbidden`**: Si la cuenta del usuario se encuentra actualmente bloqueada (mensaje: `"Usuario bloqueado"`, detalle: `"Tu cuenta se encuentra bloqueada. Contacta al administrador."`).

---

### Cambio de Contraseña (`POST /api/auth/change-password`)

*   **Ruta:** `POST /api/auth/change-password`
*   **Nombre de Acción:** `ChangePassword`
*   **Autorización:** Ninguna.

#### Cuerpo de la Solicitud (Request Body - JSON)
```json
{
  "email": "juan.perez@example.com",
  "currentPassword": "SecurePassword123!",
  "newPassword": "NewSecurePassword123!"
}
```

#### Respuesta Exitosa (`200 OK`)
Modifica la contraseña actual del usuario y retorna `ChangePasswordResponse`:
```json
{
  "email": "juan.perez@example.com",
  "message": "Contraseña cambiada exitosamente."
}
```

---

### Solicitud de Recuperación de Contraseña (`POST /api/auth/forgot-password`)

*   **Ruta:** `POST /api/auth/forgot-password`
*   **Nombre de Acción:** `ForgotPassword`
*   **Autorización:** Ninguna.

#### Cuerpo de la Solicitud (Request Body - JSON)
```json
{
  "email": "juan.perez@example.com"
}
```

#### Comportamiento
*   Verifica si el email existe en el sistema.
*   Autogenera una contraseña temporal segura (mín. 8 caracteres, que incluye al menos 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial).
*   Restablece internamente la contraseña del usuario al valor temporal generado.
*   Establece `PasswordConfirmed = false` en el perfil del usuario (lo que obliga a cambiarla mediante la ventana de cambio de contraseña en su próximo inicio de sesión exitoso).
*   Envía la nueva contraseña temporal al correo electrónico del usuario a través de `SendForgotPasswordTemporaryPasswordEmailAsync`.
*   Para evitar **ataques de enumeración de usuarios**, el endpoint siempre retorna `200 OK` con el mismo mensaje genérico de respuesta independientemente de si el correo existe o no en el sistema.

#### Respuesta Exitosa (`200 OK`)
Retorna `ForgotPasswordResponse`:
```json
{
  "message": "Si el correo electrónico ingresado existe en nuestro sistema, recibirás una nueva contraseña temporal por correo."
}
```

---

### Restablecer Contraseña (`POST /api/auth/reset-password`)

*   **Ruta:** `POST /api/auth/reset-password`
*   **Nombre de Acción:** `ResetPassword`
*   **Autorización:** Ninguna.

#### Cuerpo de la Solicitud (Request Body - JSON)
```json
{
  "email": "juan.perez@example.com",
  "token": "AQAAANCMnd8BFdERjHO...",
  "newPassword": "AnotherNewPassword123!"
}
```

#### Respuesta Exitosa (`200 OK`)
Restablece la contraseña del usuario utilizando el token generado en `forgot-password` y retorna `ResetPasswordResponse`:
```json
{
  "email": "juan.perez@example.com",
  "message": "Contraseña restablecida correctamente."
}
```

---

### Renovación de Token / Refresh Token (`POST /api/auth/refresh`)

*   **Ruta:** `POST /api/auth/refresh`
*   **Nombre de Acción:** `RefreshToken`
*   **Autorización:** Ninguna (Acceso Anónimo).

#### Cuerpo de la Solicitud (Request Body - JSON)
```json
{
  "token": "expired_access_token_string",
  "refreshToken": "refresh_token_string"
}
```

#### Respuesta Exitosa (`200 OK`)
Genera un nuevo token de acceso (JWT) y aplica la rotación del Refresh Token, retornando un nuevo token de refresco:
```json
{
  "token": "new_access_token_string",
  "refreshToken": "new_refresh_token_string"
}
```

#### Otras Respuestas
*   **`400 Bad Request`**: Si el Access Token o el Refresh Token son nulos o vacíos, si el Access Token es inválido, si el Refresh Token no coincide en base de datos, o si el Refresh Token ha expirado.
