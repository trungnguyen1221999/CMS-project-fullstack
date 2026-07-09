# Royalty API

## Overview
Royalty reporting, transaction history, and royalty payment (admin area).

Base route: `/api/admin/royalty`

All endpoints require **JWT authentication** (`[Authorize]`).

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

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | /api/admin/royalty/report-by-user-and-month | JWT only (service checks permission) | Royalty report grouped by user + month |
| GET | /api/admin/royalty/report-by-user | JWT only (service checks permission) | Royalty report grouped by user |
| GET | /api/admin/royalty/report-by-month | JWT only (service checks permission) | Royalty report grouped by month |
| GET | /api/admin/royalty/transaction-histories | JWT only + service permission check | Transaction history (paged) |
| POST | /api/admin/royalty/pay-royalty/{toUserId} | `Permissions.Royalty.Pay` | Pay royalty for all unpaid published posts of a user |

---

## Shared request: RoyaltyReportByUserAndMonthRequest

Used by:
- `GET /report-by-user-and-month`
- `GET /report-by-user`
- `GET /report-by-month`

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| UserId | guid? | No | Optional filter by author |
| FromMonth | int | Yes | Start month |
| FromYear | int | Yes | Start year |
| ToMonth | int | Yes | End month |
| ToYear | int | Yes | End year |

Date range rule: `(FromYear, FromMonth)` must be <= `(ToYear, ToMonth)`.

---

## Shared response counters

All report responses contain these counters:

| Field |
|-------|
| NumberOfDraftPosts |
| NumberOfWaitingApprovalPosts |
| NumberOfRejectedPosts |
| NumberOfPublishPosts |
| NumberOfPaidPublishPosts |
| NumberOfUnpaidPublishPosts |

Post status mapping: `0 = Draft`, `1 = WaitingForApproval`, `2 = Rejected`, `3 = Published`.

---

## GET /api/admin/royalty/report-by-user-and-month

### Query
Uses `RoyaltyReportByUserAndMonthRequest`.

### Response
`200 OK` — `List<RoyaltyReportByUserAndMonthResponse>`
```json
[
  {
    "userId": "guid",
    "userName": "string",
    "month": 7,
    "year": 2026,
    "numberOfDraftPosts": 0,
    "numberOfWaitingApprovalPosts": 0,
    "numberOfRejectedPosts": 0,
    "numberOfPublishPosts": 0,
    "numberOfPaidPublishPosts": 0,
    "numberOfUnpaidPublishPosts": 0
  }
]
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Invalid date range | INVALID_DATE_RANGE | BadRequestException | 400 |
| Requesting another user without report-view permission | INSUFFICIENT_ROYALTY_PERMISSIONS | ForbiddenException | 403 |
| `UserId` provided but user not found | USER_NOT_FOUND | NotFoundException | 404 |

---

## GET /api/admin/royalty/report-by-user

### Query
Uses `RoyaltyReportByUserAndMonthRequest`.

### Response
`200 OK` — `List<RoyaltyReportByUserResponse>`
```json
[
  {
    "userId": "guid",
    "userName": "string",
    "numberOfDraftPosts": 0,
    "numberOfWaitingApprovalPosts": 0,
    "numberOfRejectedPosts": 0,
    "numberOfPublishPosts": 0,
    "numberOfPaidPublishPosts": 0,
    "numberOfUnpaidPublishPosts": 0
  }
]
```

### Errors
Same as `report-by-user-and-month`.

---

## GET /api/admin/royalty/report-by-month

### Query
Uses `RoyaltyReportByUserAndMonthRequest`.

### Response
`200 OK` — `List<RoyaltyReportByMonthResponse>`
```json
[
  {
    "month": 7,
    "year": 2026,
    "numberOfDraftPosts": 0,
    "numberOfWaitingApprovalPosts": 0,
    "numberOfRejectedPosts": 0,
    "numberOfPublishPosts": 0,
    "numberOfPaidPublishPosts": 0,
    "numberOfUnpaidPublishPosts": 0
  }
]
```

### Errors
Same as `report-by-user-and-month`.

---

## GET /api/admin/royalty/transaction-histories

### Query
Uses `TransactionHistoryRequest`:

| Field | Type | Required | Default |
|-------|------|----------|---------|
| Keyword | string | No | null |
| CurrentPage | int | No | 1 |
| PageSize | int | No | 10 |
| FromMonth | int | Yes | — |
| FromYear | int | Yes | — |
| ToMonth | int | Yes | — |
| ToYear | int | Yes | — |

### Response
`200 OK` — paged `Transaction` data
```json
{
  "result": [
    {
      "id": "guid",
      "fromUserName": "string",
      "fromUserId": "guid",
      "toUserName": "string",
      "toUserId": "guid",
      "amount": 0,
      "transactionType": 0,
      "note": "string | null",
      "createdAt": "datetime",
      "modifiedAt": "datetime"
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

`transactionType`: `0 = RoyaltyPay`.

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| No royalty view permission | INSUFFICIENT_ROYALTY_PERMISSIONS | ForbiddenException | 403 |
| Invalid date range | INVALID_DATE_RANGE | BadRequestException | 400 |

---

## POST /api/admin/royalty/pay-royalty/{toUserId}

Pay royalty to one user for **all unpaid published posts**.

### Auth
Bearer token + `Permissions.Royalty.Pay`.

### Route Params

| Field | Type | Required |
|-------|------|----------|
| toUserId | guid | Yes |

### Response
`200 OK`
```json
true
```

### Errors

| Condition | Code | Exception | HTTP |
|-----------|------|-----------|------|
| Payer (`fromUserId`) not found | USER_NOT_FOUND | NotFoundException | 404 |
| Receiver (`toUserId`) not found | USER_NOT_FOUND | NotFoundException | 404 |
| No royalty pay permission | INSUFFICIENT_ROYALTY_PERMISSIONS | ForbiddenException | 403 |
| Receiver has no unpaid published posts | NO_UNPAID_POSTS | BadRequestException | 400 |

**Notes:**
- Marks each unpaid published post as paid (`IsPaid = true`, `PaidDate = UtcNow`, `RoyaltyAmount = receiver.RoyaltyAmountPerPost`).
- Increases receiver `Balance` by total paid amount.
- Creates one `Transaction` record with `TransactionType = RoyaltyPay`.

---

## Service Dependencies

| Service | Responsibility |
|---------|---------------|
| `IRoyaltyService` | Royalty reports, transaction history, payment flow |
| `IPermissionService` | Checks royalty view/pay permissions |
| `UserManager` | Resolve payer/receiver users and update balance |
| `UnitOfWork` | Access posts/transactions repositories and commit |
