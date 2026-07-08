# Category API

## Overview
Post category listing and management.

- `/api/category` — **public** (no authentication required), returns only **active** categories.
- `/api/admin/category` — requires **JWT authentication** + permission. Returns all categories.

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
| `ForbiddenException` | 403 |
| `BadRequestException` | 400 |

## Endpoints

### Client (Public)

| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/category | Get all active categories (list) |
| GET | /api/category/{categoryId} | Get a single active category by ID |
| GET | /api/category/paging | Get active categories (paged) |

### Admin (JWT + permission)

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | /api/admin/category | `PostCategories.View` | Get all categories (list) |
| GET | /api/admin/category/{categoryId} | `PostCategories.View` | Get a single category by ID |
| GET | /api/admin/category/paging | `PostCategories.View` | Get categories (paged) |
| POST | /api/admin/category | `PostCategories.Create` | Create a new category |
| PUT | /api/admin/category/{categoryId} | `PostCategories.Edit` | Update a category |
| DELETE | /api/admin/category/{categoryId} | `PostCategories.Delete` | Delete a category |

---

## Shared: PostCategoryResponse

Returned by all category endpoints.

```json
{
  "id": "guid",
  "name": "string",
  "slug": "string",
  "parentId": "guid | null",
  "isActive": true,
  "seoDescription": "string | null",
  "sortOrder": 0,
  "createdAt": "datetime",
  "modifiedAt": "datetime"
}
```

---

## GET /api/category

Returns all categories where `IsActive = true`.

### Response
`200 OK` — `List<PostCategoryResponse>`.

---

## GET /api/category/{categoryId}

### Route Params

| Field | Type | Required |
|-------|------|----------|
| categoryId | guid | Yes |

### Response
`200 OK` — `PostCategoryResponse`.

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Category not found or not active | CATEGORY_NOT_FOUND | NotFoundException | 404 |

---

## GET /api/category/paging

### Query

| Field | Type | Required | Default |
|-------|------|----------|---------|
| Keyword | string | No | null |
| CurrentPage | int | No | 1 |
| PageSize | int | No | 10 |

### Response
`200 OK` — paged envelope of `PostCategoryResponse`:
```json
{
  "result": [ ],
  "currentPage": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Notes:** Filters to `IsActive = true` only.

---

## GET /api/admin/category

Returns all categories regardless of `IsActive`.

### Auth
Bearer token + `Permissions.PostCategories.View`.

### Response
`200 OK` — `List<PostCategoryResponse>`.

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Caller lacks view permission | INSUFFICIENT_CATEGORY_PERMISSIONS | ForbiddenException | 403 |

---

## GET /api/admin/category/{categoryId}

### Auth
Bearer token + `Permissions.PostCategories.View`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| categoryId | guid | Yes |

### Response
`200 OK` — `PostCategoryResponse`.

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Caller lacks view permission | INSUFFICIENT_CATEGORY_PERMISSIONS | ForbiddenException | 403 |
| Category not found | CATEGORY_NOT_FOUND | NotFoundException | 404 |

---

## GET /api/admin/category/paging

### Auth
Bearer token + `Permissions.PostCategories.View`.

### Query

| Field | Type | Required | Default |
|-------|------|----------|---------|
| Keyword | string | No | null |
| CurrentPage | int | No | 1 |
| PageSize | int | No | 10 |

### Response
`200 OK` — paged envelope of `PostCategoryResponse`.

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Caller lacks view permission | INSUFFICIENT_CATEGORY_PERMISSIONS | ForbiddenException | 403 |

---

## POST /api/admin/category

### Auth
Bearer token + `Permissions.PostCategories.Create`.

### Request Body

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Name | string | Yes | Required, MaxLength(250) |
| Slug | string | Yes | Required, must be unique, varchar(250) |
| ParentId | guid | No | — |
| IsActive | bool | No | — |
| SeoKeywords | string | No | — |
| SeoDescription | string | No | — |
| SortOrder | int | No | — |

### Response
`200 OK` — `PostCategoryResponse` (the newly created category).

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Caller lacks create permission | INSUFFICIENT_CATEGORY_PERMISSIONS | ForbiddenException | 403 |
| Slug already exists | CATEGORY_SLUG_ALREADY_EXISTS | BadRequestException | 400 |
| Save to DB failed | CREATE_CATEGORY_FAILED | BadRequestException | 400 |

---

## PUT /api/admin/category/{categoryId}

### Auth
Bearer token + `Permissions.PostCategories.Edit`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| categoryId | guid | Yes |

### Request Body
Same fields as `POST /api/admin/category`.

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Caller lacks edit permission | INSUFFICIENT_CATEGORY_PERMISSIONS | ForbiddenException | 403 |
| Category not found | CATEGORY_NOT_FOUND | NotFoundException | 404 |
| Slug already in use by another category | CATEGORY_SLUG_ALREADY_EXISTS | BadRequestException | 400 |
| Save to DB failed | UPDATE_CATEGORY_FAILED | BadRequestException | 400 |

---

## DELETE /api/admin/category/{categoryId}

### Auth
Bearer token + `Permissions.PostCategories.Delete`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| categoryId | guid | Yes |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Caller lacks delete permission | INSUFFICIENT_CATEGORY_PERMISSIONS | ForbiddenException | 403 |
| Category not found | CATEGORY_NOT_FOUND | NotFoundException | 404 |
| Save to DB failed | DELETE_CATEGORY_FAILED | BadRequestException | 400 |

---

## Service Dependencies

| Service | Responsibility |
|---------|---------------|
| `ICategoryService` | All category read/write operations |
| `IPermissionService` | Checks view/create/edit/delete category permissions from JWT claims |
| `UnitOfWork` | Repository access (`ICategoryRepository`) and DB commit |
| `AutoMapper` | Maps request DTOs ↔ `PostCategory` entity ↔ `PostCategoryResponse` |
