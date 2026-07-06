# Auth API

## Overview
Authentication and account management: registration, login, forgot/reset password.

All endpoints are **public** (no authentication required).

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
| FirstName | string | Yes | MaxLength(50) |
| LastName | string | Yes | MaxLength(50) |
| Email | string | Yes | Email format |
| Password | string | Yes | MinLength(6) |
| ConfirmPassword | string | Yes | Must match Password |

### Response
`200 OK` — no data (`WriteResponse.Success()`)

### Errors

| Condition | Code | HTTP |
|-----------|------|------|
| Email already exists | USER_ALREADY_EXISTS | 400 |
| User creation failed (weak password, etc.) | CREATE_FAILED | 400 |
| Role assignment failed | FAILED_TO_ASSIGN_ROLE | 400 |

**Notes:** New account is auto-assigned the `User` role. Password is hashed by Identity.

---

## POST /api/auth/signin

### Request

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Email | string | Yes | Email format |
| Password | string | Yes | MinLength(6) |

### Response
`200 OK`
```json
{
  "accessToken": "string (JWT)",
  "refreshToken": "string"
}
```

### Errors

| Condition | Code | HTTP |
|-----------|------|------|
| Email does not exist | USER_NOT_FOUND | 400 |
| Wrong password | INVALID_PASSWORD | 400 |
| Failed to persist refresh token | UPDATE_FAILED | 400 |

**Notes:** Access token contains `sub`, `email`, `roles`, `permissions` claims. Refresh token + expiry are persisted on the user entity.

---

## POST /api/auth/forgot-password

### Request

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Email | string | Yes | Email format |

### Response
`200 OK` — no data

### Errors

| Condition | Code | HTTP |
|-----------|------|------|
| User does not exist | USER_NOT_FOUND | 400 |

**Notes:** A 4-digit OTP is generated, cached for 5 minutes (key: `otp:reset:{email}`), and emailed via Gmail SMTP.

---

## POST /api/auth/reset-password

### Request

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Email | string | Yes | Email format |
| Code | string | Yes | Exactly 4 digits |
| NewPassword | string | Yes | MinLength(6) |
| ConfirmationPassword | string | Yes | Must match NewPassword |

### Response
`200 OK` — no data

### Errors

| Condition | Code | HTTP |
|-----------|------|------|
| User does not exist | USER_NOT_FOUND | 400 |
| OTP is wrong or expired | CODE_INVALID | 400 |
| Password reset failed (password policy) | RESET_PASSWORD_FAILED | 400 |

**Notes:** OTP is removed from cache after a successful reset.

---

## Service Dependencies

| Service | Responsibility |
|---------|---------------|
| SignUpService | Registration + role assignment |
| SignInService | Authentication + JWT + refresh token |
| ForgotPasswordService | Orchestrate OTP forgot/reset flow |
| EmailService | Generate/validate/remove OTP + send email |
| TokenService | Generate JWT access token + refresh token |