# User API

## Overview
User self-management (client) and full user administration (admin).

- `/api/users` — requires **JWT authentication** (any authenticated user, no specific permission).
- `/api/admin/users` — requires **JWT authentication** + explicit permission per action.

## Error Handling
Services throw `CustomException` subclasses. Errors are caught by `GlobalExceptionHandlerMiddleware` and serialized as:
```json
{
  "isSuccess": false,
  "errorCode": "ERROR_CODE",
  "errorMessage": "ERROR_CODE"
}
```

| Exception type | HTTP status |
|----------------|------------|
| `NotFoundException` | 404 |
| `BadRequestException` | 400 |

## Endpoints

### Client (JWT required, no specific permission)

| Method | Route | Description |
|--------|-------|-------------|
| PUT | /api/users/change-password | Change own password |

### Admin (JWT + permission)

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | /api/admin/users/paging | `Users.View` | Get paged list of users |
| GET | /api/admin/users/{id} | `Users.View` | Get user detail by ID |
| POST | /api/admin/users | `Users.Create` | Create a new user |
| PUT | /api/admin/users/{id} | `Users.Edit` | Update user info |
| DELETE | /api/admin/users | `Users.Delete` | Delete users by IDs |
| PUT | /api/admin/users/{id}/set-password | `Users.Edit` | Set a new password for a user |
| PUT | /api/admin/users/{id}/change-email | `Users.Edit` | Change email of a user |
| PUT | /api/admin/users/{id}/assign-roles | `Users.Edit` | Assign roles to a user |

---

## PUT /api/users/change-password

### Auth
Bearer token (any authenticated user). User ID is extracted from JWT claims.

### Request Body

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| CurrentPassword | string | Yes | Required |
| NewPassword | string | Yes | Required |
| ConfirmNewPassword | string | Yes | Required, must match NewPassword |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| User not found | USER_NOT_FOUND | NotFoundException | 404 |
| New password is the same as current | NEW_PASSWORD_SAME_AS_CURRENT_PASSWORD | BadRequestException | 400 |
| Current password is wrong | CURRENT_PASSWORD_INCORRECT | BadRequestException | 400 |
| Identity change failed (policy) | CHANGE_PASSWORD_FAILED | BadRequestException | 400 |

---

## GET /api/admin/users/paging

### Auth
Bearer token + `Permissions.Users.View`.

### Query

| Field | Type | Required | Default |
|-------|------|----------|---------|
| Keyword | string | No | null |
| CurrentPage | int | No | 1 |
| PageSize | int | No | 10 |

### Response
`200 OK` — paged envelope of `UserListItemResponse`:
```json
{
  "result": [
    {
      "id": "guid",
      "firstName": "string",
      "lastName": "string",
      "userName": "string",
      "email": "string",
      "isActive": true,
      "createdAt": "datetime",
      "roles": ["string"]
    }
  ],
  "currentPage": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

## GET /api/admin/users/{id}

### Auth
Bearer token + `Permissions.Users.View`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| id | guid | Yes |

### Response
`200 OK` — `UserResponse`:
```json
{
  "id": "guid",
  "firstName": "string",
  "lastName": "string",
  "userName": "string",
  "email": "string",
  "phoneNumber": "string",
  "createdAt": "datetime",
  "isActive": true,
  "roles": ["string"],
  "dob": "datetime | null",
  "avatar": "string | null",
  "vipStartDate": "datetime | null",
  "vipExpireDate": "datetime | null",
  "lastLoginDate": "datetime | null",
  "balance": 0,
  "royaltyAmountPerPost": 0
}
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| User not found | USER_NOT_FOUND | NotFoundException | 404 |

---

## POST /api/admin/users

### Auth
Bearer token + `Permissions.Users.Create`.

### Request Body

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| FirstName | string | Yes | Required, MaxLength(100) |
| LastName | string | Yes | Required, MaxLength(100) |
| Email | string | Yes | Required, email format |
| PhoneNumber | string | No | MaxLength(20) |
| Password | string | Yes | Required, MinLength(6), MaxLength(100) |
| Dob | datetime | No | — |
| Avatar | string | No | MaxLength(500) |
| IsActive | bool | No | — |
| RoyaltyAmountPerPost | decimal | No | — |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Email already exists | USER_ALREADY_EXISTS | BadRequestException | 400 |
| Identity creation failed (password policy, etc.) | CREATE_FAILED | BadRequestException | 400 |

**Notes:** `UserName` is set to `Email` automatically.

---

## PUT /api/admin/users/{id}

### Auth
Bearer token + `Permissions.Users.Edit`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| id | guid | Yes |

### Request Body

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| FirstName | string | Yes | Required, MaxLength(100) |
| LastName | string | Yes | Required, MaxLength(100) |
| PhoneNumber | string | No | MaxLength(20) |
| Dob | datetime | No | — |
| Avatar | string | No | MaxLength(500) |
| IsActive | bool | No | — |
| RoyaltyAmountPerPost | decimal | No | `>= 0` |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| User not found | USER_NOT_FOUND | NotFoundException | 404 |
| Identity update failed | UPDATE_FAILED | BadRequestException | 400 |

**Notes:** If request body is null, controller short-circuits with `400 INVALID_REQUEST` before calling the service.

---

## DELETE /api/admin/users

### Auth
Bearer token + `Permissions.Users.Delete`.

### Request Body

```json
["guid", "guid"]
```

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| IDs list is null or empty | INVALID_IDS | BadRequestException | 400 |
| No users found matching the IDs | USER_NOT_FOUND | NotFoundException | 404 |

---

## PUT /api/admin/users/{id}/set-password

### Auth
Bearer token + `Permissions.Users.Edit`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| id | guid | Yes |

### Request Body

| Field | Type | Required |
|-------|------|----------|
| NewPassword | string | Yes |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| User not found | USER_NOT_FOUND | NotFoundException | 404 |
| New password same as current | NEW_PASSWORD_SAME_AS_CURRENT_PASSWORD | BadRequestException | 400 |
| Identity reset failed | SET_PASSWORD_FAILED | BadRequestException | 400 |

---

## PUT /api/admin/users/{id}/change-email

### Auth
Bearer token + `Permissions.Users.Edit`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| id | guid | Yes |

### Request Body

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Email | string | Yes | Required, email format |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| User not found | USER_NOT_FOUND | NotFoundException | 404 |
| Identity change email failed | CHANGE_EMAIL_FAILED | BadRequestException | 400 |

---

## PUT /api/admin/users/{id}/assign-roles

### Auth
Bearer token + `Permissions.Users.Edit`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| id | guid | Yes |

### Request Body

```json
["Admin", "Editor"]
```

Available roles: `Admin`, `Editor`, `Author`, `User`.

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| User not found | USER_NOT_FOUND | NotFoundException | 404 |
| Role assignment failed | ASSIGN_ROLES_FAILED | BadRequestException | 400 |

**Notes:** All existing roles are removed before new roles are assigned (full replacement, not additive).

---

## Service Dependencies

| Service | Responsibility |
|---------|---------------|
| `IUserService` | All user read/write/password/email/role operations |
| `UserManager` | ASP.NET Core Identity operations |
| `UnitOfWork` | Repository access (`IUserRepository`) and DB commit |
| `AutoMapper` | Maps request DTOs ↔ user entity ↔ response DTOs |
