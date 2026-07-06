# Post API

## Overview
Post listing and management for both client-side and admin-side APIs.

- Client endpoints are **public** (no authentication required).
- Admin endpoints require **JWT authentication** and permission checks.

> **Note:** Only endpoints implemented so far are documented below (Delete, Get-by-id, etc. are not yet built).

## Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | /api/posts | No | Get published posts (paged) |
| GET | /api/posts/category/{categorySlug} | No | Get published posts by category slug |
| GET | /api/posts/tag/{tagSlug} | No | Get published posts by tag slug |
| GET | /api/admin/posts | Yes (`Permissions.Posts.View`) | Get posts for current admin/editor/author scope (paged + filters) |
| POST | /api/admin/posts | Yes (`Permissions.Posts.Create`) | Create a new post |
| PUT | /api/admin/posts?postId={guid} | Yes (`Permissions.Posts.Edit`) | Update an existing post |

---

## GET /api/posts

### Query

| Field | Type | Required | Default | Validation |
|-------|------|----------|---------|------------|
| CurrentPage | int | No | 1 | None |
| PageSize | int | No | 10 | None |

### Response
`200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "result": [
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
        "status": 3,
        "isPaid": false,
        "royaltyAmount": 0,
        "paidDate": null
      }
    ],
    "currentPage": 1,
    "pageSize": 10,
    "totalCount": 0,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

> `status` is a numeric enum (`PostStatus`) — document the exact mapping (e.g. Draft/WaitingForApproval/Published) once confirmed.

### Errors
No business errors are returned by service for this endpoint.

**Notes:** Only posts with `Status = Published` are returned.

---

## GET /api/posts/category/{categorySlug}

### Route Params

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| categorySlug | string | Yes | None |

### Query

| Field | Type | Required | Default | Validation |
|-------|------|----------|---------|------------|
| CurrentPage | int | No | 1 | None |
| PageSize | int | No | 10 | None |

### Response
`200 OK` — same paged structure as `GET /api/posts`.

### Errors
No business errors are returned by service for this endpoint.

**Notes:**
- Always filters to `Published` posts.
- If `categorySlug` is empty, repository logic still returns published posts.

---

## GET /api/posts/tag/{tagSlug}

### Route Params

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| tagSlug | string | Yes | None |

### Query

| Field | Type | Required | Default | Validation |
|-------|------|----------|---------|------------|
| CurrentPage | int | No | 1 | None |
| PageSize | int | No | 10 | None |

### Response
`200 OK` — same paged structure as `GET /api/posts`.

### Errors
No business errors are returned by service for this endpoint.

**Notes:**
- Always filters to `Published` posts.
- Tag match uses `Contains(tagSlug)` on stored tag string.

---

## GET /api/admin/posts

### Auth
Requires Bearer token and permission: `Permissions.Posts.View`.

### Query

| Field | Type | Required | Default | Validation |
|-------|------|----------|---------|------------|
| CurrentPage | int | No | 1 | None |
| PageSize | int | No | 10 | None |
| Keyword | string | No | null | None |
| CategoryId | guid | No | null | None |

### Response
`200 OK` — paged `PostInListResponse` structure (same shape as public list response).

### Errors

| Condition | Code | HTTP |
|-----------|------|------|
| Current user not found | USER_NOT_FOUND | 404 |

**Notes:**
- Data scope depends on current user's permissions:
  - Has approve-post permission: own posts (all statuses) + others' posts (`WaitingForApproval`, `Published`).
  - No approve-post permission: only own posts (all statuses).

---

## POST /api/admin/posts

### Auth
Requires Bearer token and permission: `Permissions.Posts.Create`.

### Request

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Name | string | Required by domain | ⚠️ Not enforced by DTO validation |
| Slug | string | Required by domain | Must be unique; ⚠️ not enforced by DTO validation |
| Description | string | No | MaxLength(500) |
| Thumbnail | string | No | ⚠️ Not enforced by DTO validation |
| CategoryId | guid | Required by domain | Category must exist; ⚠️ not enforced by DTO validation |
| Content | string | No | ⚠️ Not enforced by DTO validation |
| Source | string | No | ⚠️ Not enforced by DTO validation |
| Tags | string[] | No | Default: empty array |
| SeoDescription | string | No | ⚠️ Not enforced by DTO validation |

> ⚠️ Several required-by-domain fields currently have no FluentValidation rule — requests with empty `Name`/`Slug`/`CategoryId` are only caught downstream (e.g. missing category → `CATEGORY_NOT_FOUND`), not rejected upfront as a validation error. Worth adding explicit validators.

### Response
`200 OK` — no data (`WriteResponse.Success()`).

### Errors

| Condition | Code | HTTP |
|-----------|------|------|
| Slug already exists | SLUG_ALREADY_EXISTS | 400 |
| Current user not found | USER_NOT_FOUND | 404 |
| Category not found | CATEGORY_NOT_FOUND | 404 |
| Save failed | CREATE_POST_FAILED | 400 |

**Notes:**
- Author fields (`AuthorUserId`, `AuthorUserName`, `AuthorName`) are set from current user.
- Category denormalized fields (`CategoryName`, `CategorySlug`) are copied from category entity.
- Tags are normalized by generated slug; missing tags are auto-created and linked.

---

## PUT /api/admin/posts?postId={guid}

### Auth
Requires Bearer token and permission: `Permissions.Posts.Edit`.

### Query

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| postId | guid | Yes | Existing post |

### Request
Same body as `POST /api/admin/posts` (same validation gaps apply).

### Response
`200 OK` — no data (`WriteResponse.Success()`).

### Errors

| Condition | Code | HTTP |
|-----------|------|------|
| Post not found | POST_NOT_FOUND | 404 |
| Slug already exists (another post) | SLUG_ALREADY_EXISTS | 400 |
| Category not found | CATEGORY_NOT_FOUND | 404 |
| Save failed | UPDATE_POST_FAILED | 400 |

**Notes:**
- Existing post is updated in-place via AutoMapper.
- Old post-tag links are cleared and re-created from request tags.

---

## Service Dependencies

| Service/Component | Responsibility |
|-------------------|---------------|
| PostService | Orchestrates post read/create/update flow |
| PostRepository | Query filtering + pagination for post lists |
| UserManager | Resolve current user info for admin operations |
| PermissionService | Determine post-approval visibility scope in admin list |
| UnitOfWork | Coordinates repositories and transaction commit |
| AutoMapper | Maps request DTOs to post entity and list response |