# Mobile API contract updates

This document describes the API changes for best sellers, checkout notes, COD, checkout retries, and refreshable sessions.

## Database preparation

Before starting the updated API, apply `SQLScripts/new/17 - Checkout Idempotency and Refresh Tokens.sql` to the existing `pharma_shope_db`. It is an additive, rerunnable script. It adds `CustomerNotes` and checkout idempotency columns/index to `Orders`, and creates `RefreshTokens`. It does not recreate the database or change existing order/payment records.

Customer notes use the new `Orders.CustomerNotes` column so later internal order-status notes cannot overwrite them. COD remains `PaymentMethod = 1`, matching the current database constraint. No payment database change is needed.

## Release order

Checkout now requires `Idempotency-Key`; an older mobile build that does not send it will receive 400 after the API update. Release the mobile change that generates and sends a unique key first (the older API safely ignores this header), then apply the SQL update and deploy the API. Alternatively, coordinate both releases in one window. Requests that send COD value 1 remain compatible.

## 1. Best-selling products

`GET /api/v1/products/best-sellers?storeId=1&periodDays=30&page=1&pageSize=20`

- Public endpoint. `storeId` defaults to `1`; period defaults to 30 rolling UTC days and must be 1–365.
- It uses the same `ProductListItemDto` cards as `GET /api/v1/products` and the same paged response envelope.
- Orders count when their status is Confirmed (2), Preparing (3), OutForDelivery (4), or Delivered (5). Pending, Cancelled, Returned, and PendingApproval orders are excluded.
- It sums ordered quantities and subtracts quantities from approved return requests (return status 2) associated with orders created in the selected period. Products with zero net sales are omitted.
- Results sort by net quantity descending, then product ID ascending.

Example response shape:

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "title": "Best-selling products retrieved successfully",
  "data": [
    {
      "id": 42,
      "nameAr": "اسم المنتج",
      "nameEn": "Product name",
      "primaryImageUrl": "https://api.example.com/uploads/products/42.jpg",
      "categoryNameAr": "مسكنات",
      "companyNameAr": "الشركة",
      "outerUnitNameAr": "علبة",
      "outerUnitPrice": 45.0,
      "hasPromotion": false,
      "promotionDiscountPercent": 0,
      "isFeatured": false,
      "points": 0,
      "requiresPrescription": false,
      "isAvailable": true,
      "isActive": true,
      "tagsCount": 2
    }
  ],
  "pagination": { "currentPage": 1, "pageSize": 20, "totalCount": 1, "totalPages": 1 }
}
```

## 2. Customer notes on checkout

`POST /api/v1/orders/checkout` accepts an optional `notes` field, maximum 500 characters. The API trims it; an empty value is stored as null. It is saved to the order and returned by `GET /api/v1/orders/{id}`.

With a note:

```json
{
  "sourceType": 1,
  "sourceId": 12,
  "addressId": 5,
  "paymentMethod": 1,
  "notes": "اتصل بي عند الوصول"
}
```

Without a note:

```json
{
  "sourceType": 1,
  "sourceId": 12,
  "addressId": 5,
  "paymentMethod": 1
}
```

After confirmation, the customer order details response has `data.notes` set to the submitted note. Customer notes are stored separately from internal status notes; admin order details also expose internal notes as `data.internalNotes`.

## 3. Cash on delivery value

The single supported payment method is COD, numeric value `1`. Send `paymentMethod: 1` to checkout and checkout preview. The shared enum now uses `COD = 1` (`CashOnDelivery` is an alias for the same value). Card payment is not enabled by this API contract.

The database already constrains `Orders.PaymentMethod` to `1`; there is no data migration. Mobile builds that send `1` need no payment-value change. Any client sending `2` should be updated to `1` when the updated API is deployed.

## 4. Checkout idempotency

Every checkout confirmation now requires the `Idempotency-Key` header. Use a new unique key for each intended order, and reuse that exact key and request body for network retries. Keys are scoped to the authenticated customer and may contain up to 100 characters.

The API stores the key, a hash of checkout input (`sourceType`, `sourceId`, `addressId`, `paymentMethod`, and normalized `notes`), and the original checkout result in the same transaction as the order. Repeating the key with identical input returns the same `201` response and order. Reusing it with changed input returns `409 Conflict`. The checkout basket, prescription, offer, and points-redemption processing remains in the existing checkout flow.

Example first request and retry after the connection drops:

```http
POST /api/v1/orders/checkout
Authorization: Bearer {{customerToken}}
Idempotency-Key: mobile-order-2026-000042
Content-Type: application/json
```

```json
{
  "sourceType": 1,
  "sourceId": 12,
  "addressId": 5,
  "paymentMethod": 1,
  "notes": "اتصل بي عند الوصول"
}
```

Send the same header value and identical body on retry. The response is the original order result, including `data.orderId` and `data.orderNumber`, with the order location. If the same key is sent with a different address, source, payment value, or note, the API returns 409.

Successful response shape:

```json
{
  "isSuccess": true,
  "statusCode": 201,
  "title": "Order created successfully",
  "location": "/api/v1/orders/123",
  "data": {
    "orderId": 123,
    "orderNumber": "ORD-20261122120000000",
    "subtotal": 120.0,
    "promotionDiscountTotal": 0.0,
    "subtotalAfterPromotions": 120.0,
    "redeemedPoints": 0,
    "redeemedAmount": 0.0,
    "discountAmount": 0.0,
    "deliveryFee": 15.0,
    "grandTotal": 135.0,
    "items": []
  }
}
```

## 5. Refreshable sessions

Login now returns `refreshToken` and `refreshTokenExpiresAtUtc` alongside the access token. Refresh tokens are random opaque values; only their SHA-256 hashes are stored. The default lifetime is 30 days (`Jwt:RefreshTokenDays`). Each successful refresh rotates the token: the old token is revoked and a replacement token is returned. The same refresh token cannot be used twice.

### Refresh

`POST /api/auth/refresh` is public and accepts:

```json
{ "refreshToken": "<refresh token from login>" }
```

Success returns the normal login response shape with a new access token and rotated refresh token. Invalid, expired, or revoked tokens return 401.

The updated login and refresh response data includes:

```json
{
  "userId": 25,
  "accessToken": "<JWT>",
  "expiresAtUtc": "2026-12-02T12:00:00Z",
  "refreshToken": "<opaque random token>",
  "refreshTokenExpiresAtUtc": "2026-12-22T12:00:00Z",
  "tokenType": "Bearer"
}
```

### Logout

`POST /api/auth/logout` requires the current access token and the refresh token to revoke:

```http
Authorization: Bearer {{customerToken}}
```

```json
{ "refreshToken": "<current refresh token>" }
```

Logout revokes that refresh token. Access JWTs remain valid until their configured expiry (`Jwt:AccessTokenMinutes`, currently 14400 minutes); clients should discard the access token locally at logout.

## Postman

Import `postman/PharmaShop-Mobile-Contract-Updates.postman_collection.json`. Set `baseUrl`, `loginIdentifier`, `loginPassword`, `checkoutSourceId`, and `addressId`. Run the included updated Login request to populate the access and refresh tokens. Checkout examples use COD value 1. The retry item deliberately reuses the same idempotency key; the conflict example changes notes while keeping the key. The collection includes only these new or changed contracts, including Login because its response now contains refresh-token fields.
