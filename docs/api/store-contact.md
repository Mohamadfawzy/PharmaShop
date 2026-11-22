# Store contact endpoint

## Public endpoint

`GET /api/v1/stores/{storeId}/contact`

Returns contact information for one active store. The phone, WhatsApp, email, address, and working-hours properties are nullable; unavailable values are returned as `null`. The current app can continue using `storeId=1`. When branch selection is enabled later, pass the selected store ID without changing this API contract.

Example:

```http
GET /api/v1/stores/1/contact
```

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "title": "Store contact details retrieved successfully",
  "data": {
    "phone": "+201000000000",
    "whatsApp": "+201000000000",
    "email": "branch@example.com",
    "address": "عنوان الفرع",
    "workingHours": "يوميًا من 9 صباحًا حتى 11 مساءً"
  }
}
```

Unknown, inactive, or deleted stores return 404. When contact values are not configured, a successful response may contain null fields; the client can keep showing its “بيانات التواصل غير متاحة حاليًا” fallback when all fields are null.

## Managing branch details

Contact information is stored on `Stores`, so values can differ by branch. Admins can set all contact fields using `PUT /api/v1/admin/stores/{storeId}/contact`. Store creation can also include `phone`, `whatsApp`, `email`, `address`, and `workingHours`. The separate update route keeps older admin clients from clearing contact data when they update only the store name or status.

Limits: phone and WhatsApp up to 50 characters, email up to 256 characters and must be a valid email address when supplied, address up to 300 characters, and working hours up to 500 characters. The contact update replaces the contact profile; omitted optional fields become null. It accepts an optional `rowVersion` for concurrency protection.

Before starting the updated API, apply `SQLScripts/new/18 - Store Contact Information.sql` to the current database. It only adds nullable columns and does not rebuild the database. The script was added to `SQLScripts/new` for the full schema setup sequence.

## Postman

Import `postman/PharmaShop-Store-Contact.postman_collection.json`. Set `baseUrl`, `storeId`, and `adminToken`. The public GET can be run without a token; the collection also includes the existing admin store update request with the newly supported contact fields.
