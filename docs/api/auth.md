# Auth API

## Overview
Authentication and account management: registration, login, forgot/reset password.

All endpoints are **public** (no authentication required).

## Error Handling
Auth services throw `CustomException` subclasses instead of returning `WriteResponse.Failure`. Errors are caught by `GlobalExceptionHandlerMiddleware` and serialized as:
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

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | /api/auth/signup | No | Register a new account |
| POST | /api/auth/signin | No | Login and receive JWT tokens |
| POST | /api/auth/forgot-password | No | Send 4-digit OTP to email |
| POST | /api/auth/reset-password | No | Reset password using OTP |

---

## POST /api/auth/signup

### Request

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| FirstName | string | Yes | Required, MaxLength(50) |
| LastName | string | Yes | Required, MaxLength(50) |
| Email | string | Yes | Required, email format |
| Password | string | Yes | Required, MinLength(6) |
| ConfirmPassword | string | Yes | Required, must match Password |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Email already exists | USER_ALREADY_EXISTS | BadRequestException | 400 |
| User creation failed (password policy, etc.) | CREATE_FAILED | BadRequestException | 400 |
| Role assignment failed | FAILED_TO_ASSIGN_ROLE | BadRequestException | 400 |

**Notes:** New account is auto-assigned the `User` role. Password is hashed by ASP.NET Core Identity.

---

## POST /api/auth/signin

### Request

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Email | string | Yes | Required, email format |
| Password | string | Yes | Required, MinLength(6) |

### Response
`200 OK`
```json
{
  "token": "string (JWT)",
  "refreshToken": "string"
}
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Email does not exist | USER_NOT_FOUND | NotFoundException | 404 |
| Wrong password | INVALID_PASSWORD | BadRequestException | 400 |
| Failed to persist refresh token | UPDATE_FAILED | BadRequestException | 400 |

**Notes:** Refresh token + expiry time are persisted on the user entity. `IsActive` is set to `true` on sign-in.

### JWT Access Token — Claims

| Claim key | Source | Description |
|-----------|--------|-------------|
| `sub` | `user.Id` | Standard JWT subject (user GUID) |
| `jti` | `Guid.NewGuid()` | Unique token ID |
| `id` | `user.Id` | User GUID (custom claim) |
| `email` | `user.Email` | User email |
| `firstName` | `user.FirstName` | First name |
| `lastName` | `user.LastName` | Last name |
| `avatar` | `user.Avatar` | Avatar URL (empty string if unset) |
| `balance` | `user.Balance` | Royalty balance |
| `roles` | Identity roles | Comma-separated role names, e.g. `Author,Editor` |
| `permissions` | Aggregated from roles | Comma-separated permission strings (see table below) |

**Token expiry:** configured via `JwtSettings:AccessTokenExpiryMinutes` (default: 15 minutes).

### Permissions Claim — Role Mapping

Permissions are **aggregated** across all roles the user belongs to (union of sets). The `permissions` claim is a single comma-separated string.

| Role | Included permission sets | Effective permissions |
|------|--------------------------|-----------------------|
| `User` | PublicReader | `Permissions.PostCategories.View`, `Permissions.Posts.View`, `Permissions.Series.View` |
| `Author` | PublicReader + Writing | User's permissions + `Permissions.Posts.Create`, `Permissions.Posts.Edit`, `Permissions.Royalty.View` |
| `Editor` | PublicReader + Editing | User's permissions + `Permissions.Dashboard.View`, `Permissions.PostCategories.Create`, `Permissions.PostCategories.Edit`, `Permissions.Series.Create`, `Permissions.Series.Edit`, `Permissions.Series.Delete`, `Permissions.Posts.Delete`, `Permissions.Posts.Approve` |
| `Admin` | All permissions | Every permission defined in `Permissions` class |

**Example decoded `permissions` claim for an `Author`:**
```
Permissions.PostCategories.View,Permissions.Posts.View,Permissions.Series.View,Permissions.Posts.Create,Permissions.Posts.Edit,Permissions.Royalty.View
```

**How it is used:** `[HasPermission("Permissions.Posts.Create")]` on a controller action triggers `PermissionHandler`, which reads the `permissions` claim, splits by `,`, and checks for containment. A missing or non-matching claim results in `403 Forbidden`.

---

## POST /api/auth/forgot-password

### Request

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
| User does not exist | USER_NOT_FOUND | NotFoundException | 404 |

**Notes:** A 4-digit OTP is generated, cached for 5 minutes (key: `otp:reset:{email}`), and emailed via Gmail SMTP.

---

## POST /api/auth/reset-password

### Request

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Email | string | Yes | Required, email format |
| Code | string | Yes | Required, exactly 4 digits (`^\d{4}$`) |
| NewPassword | string | Yes | Required |
| ConfirmationPassword | string | Yes | Required, must match NewPassword |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| User does not exist | USER_NOT_FOUND | NotFoundException | 404 |
| OTP is wrong or expired | CODE_INVALID | BadRequestException | 400 |
| Password reset failed (password policy) | RESET_PASSWORD_FAILED | BadRequestException | 400 |

**Notes:** OTP is removed from cache after a successful reset.

---

## Service Dependencies

| Service | Responsibility |
|---------|---------------|
| `SignUpService` | Registration + role assignment |
| `SignInService` | Authentication + JWT + refresh token persistence |
| `ForgotPasswordService` | Orchestrate OTP forgot/reset flow |
| `IEmailService` | Generate / validate / remove OTP + send email |
| `ITokenService` | Generate JWT access token + refresh token |