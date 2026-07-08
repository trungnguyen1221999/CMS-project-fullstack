# Post API

## Overview
Post listing and management for both client-side and admin-side APIs.

- Client endpoints (`/api/posts`) are **public** (no authentication required).
- Admin endpoints (`/api/admin/posts`) require **JWT authentication** and permission checks.

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
| GET | /api/posts/paging | Get published posts (paged) |
| GET | /api/posts/{postId} | Get a single published post by ID |
| GET | /api/posts/category/{categorySlug} | Get published posts by category |
| GET | /api/posts/tag/{tagSlug} | Get published posts by tag |

### Admin (JWT required)

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | /api/admin/posts | `Posts.View` | Get posts scoped to current user role |
| GET | /api/admin/posts/{postId} | `Posts.View` | Get a single post (access-controlled) |
| GET | /api/admin/posts/reject-reason/{postId} | `Posts.View` | Get rejection reason of a post |
| GET | /api/admin/posts/activity-logs/{postId} | `Posts.Approve` | Get activity log of a post |
| POST | /api/admin/posts | `Posts.Create` | Create a new post |
| PUT | /api/admin/posts/{postId} | `Posts.Edit` | Update an existing post |
| DELETE | /api/admin/posts | `Posts.Delete` | Delete posts by IDs |
| PUT | /api/admin/posts/approve/{postId} | `Posts.Approve` | Approve a post |
| PUT | /api/admin/posts/reject/{postId} | `Posts.Approve` | Reject a post |
| PUT | /api/admin/posts/approval-submit/{postId} | JWT only | Submit post for approval |

---

## Shared: PagingRequest (query params)

Used by all list endpoints.

| Field | Type | Required | Default |
|-------|------|----------|---------|
| Keyword | string | No | null |
| CurrentPage | int | No | 1 |
| PageSize | int | No | 10 |

## Shared: PostInListResponse (paged list item)

```json
{
  "id": "guid",
  "name": "string",
  "slug": "string",
  "description": "string | null",
  "thumbnail": "string | null",
  "viewCount": 0,
  "categorySlug": "string",
  "categoryName": "string",
  "authorUserName": "string",
  "authorName": "string",
  "status": 0,
  "isPaid": false,
  "royaltyAmount": 0,
  "paidDate": null
}
```

`status` enum: `0 = Draft`, `1 = WaitingForApproval`, `2 = Rejected`, `3 = Published`.

## Shared: Paged response envelope

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

---

## GET /api/posts/paging

Returns all published posts, paged.

### Query
See [Shared: PagingRequest](#shared-pagingrequest-query-params).

### Response
`200 OK` — [Paged envelope](#shared-paged-response-envelope) of `PostInListResponse`.

**Notes:** Filters to `Status = Published` only.

---

## GET /api/posts/{postId}

Returns the full detail of a single published post.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| postId | guid | Yes |

### Response
`200 OK`
```json
{
  "id": "guid",
  "categoryId": "guid",
  "content": "string | null",
  "authorUserId": "guid",
  "source": "string | null",
  "tags": "string | null",
  "seoDescription": "string | null",
  "createdAt": "datetime",
  "modifiedAt": "datetime"
}
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Post not found or not published | POST_NOT_FOUND | NotFoundException | 404 |

---

## GET /api/posts/category/{categorySlug}

### Route Params

| Field | Type | Required |
|-------|------|----------|
| categorySlug | string | Yes |

### Query
See [Shared: PagingRequest](#shared-pagingrequest-query-params).

### Response
`200 OK` — [Paged envelope](#shared-paged-response-envelope) of `PostInListResponse`.

**Notes:** Filters to `Status = Published` only.

---

## GET /api/posts/tag/{tagSlug}

### Route Params

| Field | Type | Required |
|-------|------|----------|
| tagSlug | string | Yes |

### Query
See [Shared: PagingRequest](#shared-pagingrequest-query-params).

### Response
`200 OK` — [Paged envelope](#shared-paged-response-envelope) of `PostInListResponse`.

**Notes:** Filters to `Status = Published` only. Tag match uses `Contains(tagSlug)` on stored tag string.

---

## GET /api/admin/posts

### Auth
Bearer token + `Permissions.Posts.View`.

### Query

| Field | Type | Required | Default |
|-------|------|----------|---------|
| Keyword | string | No | null |
| CurrentPage | int | No | 1 |
| PageSize | int | No | 10 |
| CategoryId | guid | No | null |

### Response
`200 OK` — [Paged envelope](#shared-paged-response-envelope) of `PostInListResponse`.

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Current user not found | USER_NOT_FOUND | NotFoundException | 404 |

**Notes:** Data scope depends on the current user's permissions:
- **Has approve-post permission** (Editor/Admin): own posts (all statuses) + others' posts with status `WaitingForApproval` or `Published`.
- **No approve-post permission** (Author): only own posts (all statuses).

---

## GET /api/admin/posts/{postId}

### Auth
Bearer token + `Permissions.Posts.View`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| postId | guid | Yes |

### Response
`200 OK` — raw `Post` entity.

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Post not found | POST_NOT_FOUND | NotFoundException | 404 |
| Current user not found | USER_NOT_FOUND | NotFoundException | 404 |
| Post is `Draft` and caller is not the author | POST_NOT_FOUND | NotFoundException | 404 |
| Caller is not author and lacks approve permission | INSUFFICIENT_POST_PERMISSION | ForbiddenException | 403 |

---

## GET /api/admin/posts/reject-reason/{postId}

### Auth
Bearer token + `Permissions.Posts.View`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| postId | guid | Yes |

### Response
`200 OK` — string (rejection reason text).

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Post not found | POST_NOT_FOUND | NotFoundException | 404 |
| Current user not found | USER_NOT_FOUND | NotFoundException | 404 |
| Post status is not `Rejected` | POST_NOT_REJECTED | BadRequestException | 400 |
| Caller is not author and lacks approve permission | INSUFFICIENT_POST_PERMISSION | ForbiddenException | 403 |
| Rejection reason not found in activity log | FAIL_TO_GET_REJECT_REASON | BadRequestException | 400 |

---

## GET /api/admin/posts/activity-logs/{postId}

### Auth
Bearer token + `Permissions.Posts.Approve`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| postId | guid | Yes |

### Response
`200 OK` — `List<PostActivityLog>`.

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Post not found | POST_NOT_FOUND | NotFoundException | 404 |
| Current user not found | USER_NOT_FOUND | NotFoundException | 404 |

---

## POST /api/admin/posts

### Auth
Bearer token + `Permissions.Posts.Create`.

### Request Body

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Name | string | Yes* | — |
| Slug | string | Yes* | Must be globally unique |
| Description | string | No | MaxLength(500) |
| Thumbnail | string | No | — |
| CategoryId | guid | Yes* | Category must exist |
| Content | string | No | — |
| Source | string | No | — |
| Tags | string[] | No | Default: `[]` |
| SeoDescription | string | No | — |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Slug already exists | SLUG_ALREADY_EXISTS | BadRequestException | 400 |
| Current user not found | USER_NOT_FOUND | NotFoundException | 404 |
| Category not found | CATEGORY_NOT_FOUND | NotFoundException | 404 |
| Save to DB failed | CREATE_POST_FAILED | BadRequestException | 400 |

**Notes:**
- Author fields (`AuthorUserId`, `AuthorUserName`, `AuthorName`) are populated from current user.
- Category denormalized fields (`CategoryName`, `CategorySlug`) are copied from the category entity.
- Tags are slug-normalized; new tags are auto-created and linked via `PostTags`.

---

## PUT /api/admin/posts/{postId}

### Auth
Bearer token + `Permissions.Posts.Edit`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| postId | guid | Yes |

### Request Body
Same fields as `POST /api/admin/posts`.

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Post not found | POST_NOT_FOUND | NotFoundException | 404 |
| Slug already in use by another post | SLUG_ALREADY_EXISTS | BadRequestException | 400 |
| Category not found | CATEGORY_NOT_FOUND | NotFoundException | 404 |
| Save to DB failed | UPDATE_POST_FAILED | BadRequestException | 400 |

**Notes:** Old post-tag links are cleared and re-created from request tags on every update.

---

## DELETE /api/admin/posts

### Auth
Bearer token + `Permissions.Posts.Delete`.

### Query

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ids | guid[] | Yes | Repeat param: `?ids=...&ids=...` |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Current user not found | USER_NOT_FOUND | NotFoundException | 404 |
| Any ID not found / count mismatch | POST_NOT_FOUND | NotFoundException | 404 |
| Caller tries to delete another author's post without delete permission | INSUFFICIENT_POST_PERMISSION | ForbiddenException | 403 |
| Save to DB failed | DELETE_POST_FAILED | BadRequestException | 400 |

**Notes:** Post-tag and post-in-series links are also removed for each deleted post.

---

## PUT /api/admin/posts/approve/{postId}

### Auth
Bearer token + `Permissions.Posts.Approve`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| postId | guid | Yes |

### Request Body

| Field | Type | Required |
|-------|------|----------|
| note | string | No |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Post not found | POST_NOT_FOUND | NotFoundException | 404 |
| Current user not found | USER_NOT_FOUND | NotFoundException | 404 |
| Approve transition failed (wrong status) | APPROVE_POST_FAILED | BadRequestException | 400 |

---

## PUT /api/admin/posts/reject/{postId}

### Auth
Bearer token + `Permissions.Posts.Approve`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| postId | guid | Yes |

### Request Body

| Field | Type | Required |
|-------|------|----------|
| note | string | No |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Post not found | POST_NOT_FOUND | NotFoundException | 404 |
| Current user not found | USER_NOT_FOUND | NotFoundException | 404 |
| Reject transition failed (wrong status) | REJECT_POST_FAILED | BadRequestException | 400 |

---

## PUT /api/admin/posts/approval-submit/{postId}

Submit a post for review (author action).

### Auth
Bearer token (any authenticated user).

### Route Params

| Field | Type | Required |
|-------|------|----------|
| postId | guid | Yes |

### Request Body

| Field | Type | Required |
|-------|------|----------|
| note | string | No |

### Response
`200 OK`
```json
{ "isSuccess": true }
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Post not found | POST_NOT_FOUND | NotFoundException | 404 |
| Current user not found | USER_NOT_FOUND | NotFoundException | 404 |
| Caller is not the post's author | INSUFFICIENT_POST_PERMISSION | ForbiddenException | 403 |
| Submit transition failed (wrong status) | SUBMIT_FOR_APPROVAL_FAILED | BadRequestException | 400 |

---

## Service Dependencies

| Service | Responsibility |
|---------|---------------|
| `IClientPostService` | Public read operations (published posts) |
| `IAdminPostService` | Admin read + all write/workflow operations |
| `PostRepository` | Query filtering + pagination |
| `UserManager` | Resolve current user for admin operations |
| `IPermissionService` | Determine approval/delete visibility scope |
| `UnitOfWork` | Coordinates repositories and DB commit |
| `AutoMapper` | Maps request DTOs <-> entities <-> responses |
