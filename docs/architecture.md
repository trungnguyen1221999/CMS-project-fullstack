# CMS Project - Architecture

## Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend | React | TODO |
| Backend API | ASP.NET Core | .NET 10 |
| Database | PostgreSQL | Neon Cloud |
| ORM | Entity Framework Core | 10.0.9 |
| Authentication | JWT Bearer Token | - |
| Authorization | Permission-based (Dynamic Policies) | - |
| Caching | IDistributedCache | Redis-ready |
| Email | MailKit | 4.17.0 |
| Logging | Serilog | Console + File |
| Mapping | AutoMapper | 16.1.1 |
| Testing | xUnit + Moq | 2.9.3 / 4.20.72 |

---

## Architecture Overview

```mermaid
graph TD
    Client["Client (React)"] --> WebApi["WebApi Layer"]
    WebApi --> Auth["JWT + Permission Authorization"]
    WebApi --> Controllers["Controllers"]
    Controllers --> AppLayer["Application Layer"]
    AppLayer --> Services["Services"]
    AppLayer --> UoW["IUnitOfWork"]
    Services --> UoW
    UoW --> Infra["Infrastructure Layer"]
    Infra --> Repos["Repositories"]
    Infra --> EF["EF Core"]
    EF --> DB["PostgreSQL (Neon)"]
    AppLayer --> Domain["Domain Layer"]
    Domain --> Entities["Entities (User, Post, Category, Tag...)"]
```

---

## Project Structure

```
backend/
├── src/
│   ├── Domain/                        → Entities, Enums, Constants
│   │   ├── Cores/Identity/            → User entity
│   │   ├── Cores/Content/             → Post, Category, Tag, PostTag
│   │   ├── Constants/                 → Roles, Permissions, UserClaims
│   │   └── Commons/                   → AuditableEntity base class
│   │
│   ├── Application/                   → Business Logic (no external dependencies)
│   │   ├── Services/
│   │   │   ├── Auth/                  → SignIn, SignUp, ForgotPassword
│   │   │   ├── User/                  → User CRUD + password/email/role
│   │   │   ├── Post/                  → Post CRUD
│   │   │   ├── Token/                 → ITokenService interface
│   │   │   ├── Otp/                   → IEmailService interface
│   │   │   └── Permission/            → Permission checking
│   │   ├── Contracts/                  → DTOs (Requests, Responses)
│   │   ├── Constants/                  → ErrorMessages, EmailTemplates
│   │   ├── Exceptions/                 → CustomException (NotFoundException, ForbiddenException, BadRequestException)
│   │   ├── Repositories/               → Repository interfaces
│   │   └── UnitOfWork/                 → IUnitOfWork interface
│   │
│   ├── Infrastructure/                 → External Concerns
│   │   ├── Repositories/               → EF Core implementations
│   │   ├── Services/                   → TokenService, EmailService
│   │   ├── UnitOfWork/                 → UnitOfWork implementation
│   │   └── Migrations/                 → EF Core migrations
│   │
│   └── WebApi/                         → Entry Point
│       ├── Controllers/                → Auth, User, AdminPost
│       ├── Authorization/              → Permission system
│       ├── Middlewares/                → GlobalExceptionHandlerMiddleware
│       └── Extensions/                 → DI, Auth, Serilog, Middleware
│
├── test/
│   ├── Application.Tests/              → service tests
│   ├── Infrastructure.Test/            → token + email tests
│   └── Test.Shared/                    → MockUserManager
│
└── docs/                                → Documentation
```

---

## Dependency Direction (Clean Architecture)

```mermaid
graph LR
    WebApi --> Application
    WebApi --> Infrastructure
    Infrastructure --> Application
    Application --> Domain
    Infrastructure --> Domain
```

---

## Authentication Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant M as JWT Middleware
    participant A as AuthController
    participant S as SignInService
    participant T as TokenService

    C->>A: POST /api/auth/signin (email, password)
    A->>S: SignInAsync(request)
    S->>S: FindByEmail + CheckPassword
    S->>T: GenerateAccessToken(user, roles)
    T->>T: Create JWT (sub, email, roles, permissions)
    S->>T: GenerateRefreshToken()
    S-->>C: 200 OK + tokens

    Note over C,M: Subsequent requests

    C->>M: Authorization: Bearer {token}
    M->>M: Decode → Validate → ClaimsPrincipal
    M->>M: HttpContext.User = principal
```

---

## Authorization Levels

| Level | Mechanism | Example |
|-------|-----------|---------|
| Authenticated | `[Authorize]` | ChangeMyPassword |
| Role-based | `[Authorize(Roles="Admin")]` | Available (not used) |
| Permission-based | `[HasPermission(...)]` | Users.View, Posts.Create |

### Permission Flow

```mermaid
sequenceDiagram
    participant R as Request
    participant PP as PermissionPolicyProvider
    participant PH as PermissionHandler
    participant T as JWT Token

    R->>PP: Policy = "Permissions.Users.View"
    PP->>PP: Create dynamic policy
    PP->>PH: PermissionRequirement
    PH->>T: Read "permissions" claim
    PH->>PH: Split by comma → Contains check
    PH-->>R: 200 OK / 403 Forbidden
```

---

## Response Pattern

```csharp
// Write (no data)
WriteResponse.Success()
WriteResponse.Failure(errorCode, errorMessage?)

// Read (with data)
ReadResponse<T>.Success(data)
ReadResponse<T>.Failure(errorCode)

// Auth-specific
SignInResponse.Success(token, refreshToken)
SignUpResponse.Success()
```

---

## Unit of Work Pattern

```mermaid
graph TD
    Service["Service"] --> UoW["IUnitOfWork"]
    UoW --> Users["IUserRepository"]
    UoW --> Posts["IPostRepository"]
    UoW --> Categories["ICategoryRepository"]
    UoW --> Tags["ITagRepository"]
    UoW --> PostTags["IPostTagsRepository"]
    UoW --> Complete["CompleteAsync() → SaveChanges 1 time"]
```

---

## Exception Handling

All unhandled exceptions are caught centrally by `GlobalExceptionHandlerMiddleware` (registered as the **first** middleware in the pipeline).

### Custom Exception Hierarchy

```
CustomException  (abstract, Application.Exceptions)
├── NotFoundException   → 404 Not Found
├── ForbiddenException  → 403 Forbidden
└── BadRequestException → 400 Bad Request
```

Each subclass carries:
- `ErrorCode` — machine-readable constant (same format as `ErrorMessages`)
- `Message` — human-readable description (defaults to `ErrorCode` if omitted)

### Middleware Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant M as GlobalExceptionHandlerMiddleware
    participant P as Next Middleware / Controller

    C->>M: HTTP Request
    M->>P: await _next(context)
    alt CustomException thrown
        P-->>M: NotFoundException / ForbiddenException / BadRequestException
        M->>M: Map to 404 / 403 / 400
        M-->>C: WriteResponse.Failure(errorCode, message)
    else Unexpected Exception
        P-->>M: Exception
        M->>M: Log.Error (Serilog)
        M-->>C: 500 WriteResponse.Failure(INTERNAL_SERVER_ERROR)
    end
```

### When to throw vs. return Failure

| Scenario | Approach |
|----------|---------|
| Service method returns result to caller (most cases) | Return `WriteResponse.Failure(errorCode)` |
| Deep helper / private method that cannot return a result | Throw `CustomException` subclass |
| Truly unexpected runtime error | Let it bubble — middleware catches it |

---

## OTP Email Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant F as ForgotPasswordService
    participant E as EmailService
    participant Cache as IDistributedCache
    participant SMTP as Gmail SMTP

    C->>F: forgot-password (email)
    F->>E: SendOtpAsync(email)
    E->>E: Generate 4-digit OTP
    E->>Cache: Save OTP (5min expiry)
    E->>SMTP: Send HTML email
    F-->>C: 200 OK

    C->>F: reset-password (email, code, newPassword)
    F->>E: ValidateOtpAsync(email, code)
    E->>Cache: Get + compare OTP
    F->>F: ResetPasswordAsync (Identity)
    F->>E: RemoveOtpAsync(email)
    F-->>C: 200 OK
```