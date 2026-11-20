# API Request and Response Examples

الأمثلة مشتقة من DTOs الفعلية وتستخدم أسماء JSON camelCase. قيم IDs أمثلة فقط؛ استخدم المعرفات التي ترجع من بيئتك. التفاصيل الكاملة للحقول والقيود في request-models.md.

## Login

POST /api/auth/login

~~~json
{
  "identifier": "customer@example.com",
  "password": "ExamplePassword1!",
  "rememberMe": false
}
~~~

LoginResponseDto يحتوي userId وaccessToken وexpiresAtUtc فقط؛ لا يوجد refreshToken:

~~~json
{
  "isSuccess": true,
  "title": "Operation Successful",
  "type": null,
  "statusCode": 200,
  "detail": null,
  "instance": null,
  "location": null,
  "errorCode": "None",
  "traceId": "0HABCDEF123456:00000001",
  "timestamp": "2026-11-20T10:00:00Z",
  "errors": [],
  "fieldErrors": null,
  "data": {
    "userId": 42,
    "accessToken": "<JWT issued by this API>",
    "expiresAtUtc": "2026-11-20T10:30:00Z"
  },
  "pagination": null
}
~~~

## عنوان أساسي للعميل

POST /api/v1/addresses

~~~json
{
  "title": "المنزل",
  "governorateId": 1,
  "governorateAreaId": 1,
  "street": "شارع التحرير، مبنى 10، شقة 4",
  "landmark": "بجوار محطة المترو",
  "latitude": 30.0444,
  "longitude": 31.2357,
  "isDefault": true
}
~~~

بنية CustomerAddressDto:

~~~json
{
  "id": 9,
  "title": "المنزل",
  "governorateId": 1,
  "governorateNameAr": "القاهرة",
  "governorateAreaId": 1,
  "areaNameAr": "مدينة نصر",
  "areaNameEn": "Nasr City",
  "city": "القاهرة",
  "region": null,
  "street": "شارع التحرير، مبنى 10، شقة 4",
  "landmark": "بجوار محطة المترو",
  "latitude": 30.0444,
  "longitude": 31.2357,
  "isDefault": true,
  "createdAt": "2026-11-20T10:00:00",
  "updatedAt": null
}
~~~

الخدمة قد ترجع ID بعد الإنشاء بدل DTO كاملة؛ راجع CustomerAddressService عند بناء العميل.

## بحث المنتجات

GET /api/v1/products?storeId=1&q=paracetamol&categoryId=4&tagIds=1&tagMatch=Any&minPrice=10&maxPrice=100&isAvailable=true&sort=PriceAsc&page=1&pageSize=20

بنية ProductListItemDto:

~~~json
{
  "id": 25,
  "nameAr": "باراسيتامول 500 مجم",
  "nameEn": "Paracetamol 500 mg",
  "primaryImageUrl": "/uploads/small/product-example.webp",
  "categoryNameAr": "خافضات الحرارة",
  "companyNameAr": "إيبيكو",
  "outerUnitNameAr": "علبة",
  "outerUnitPrice": 35.00,
  "hasPromotion": false,
  "promotionDiscountPercent": 0,
  "promotionStartsAt": null,
  "promotionEndsAt": null,
  "isFeatured": false,
  "points": 10,
  "requiresPrescription": false,
  "isAvailable": true,
  "isActive": true,
  "tagsCount": 2
}
~~~

هذه بنية العنصر؛ الـlist قد يتضمن pagination حسب نتيجة Service.

## السلة

POST /api/v1/cart/items

~~~json
{
  "storeId": 1,
  "productId": 25,
  "unitLevel": "Outer",
  "quantity": 2,
  "deviceId": "android-device-01",
  "appInstanceId": "pharmashop-mobile"
}
~~~

GET /api/v1/cart، بنية CartViewDto وCartItemViewDto:

~~~json
{
  "cartId": 14,
  "storeId": 1,
  "status": 1,
  "statusUpdatedAt": "2026-11-20T10:00:00",
  "subtotalCurrent": 70.00,
  "items": [
    {
      "cartItemId": 31,
      "productId": 25,
      "sourceType": "Normal",
      "unitLevel": 1,
      "quantity": 2,
      "nameAr": "باراسيتامول 500 مجم",
      "nameEn": "Paracetamol 500 mg",
      "primaryImageUrl": "/uploads/small/product-example.webp",
      "currentUnitPrice": 35.00,
      "currentUnitPriceSnapshot": 35.00,
      "availableQty": 100,
      "exceedsAvailableQty": false,
      "isRedeemableByPoints": false,
      "isActive": true,
      "isAvailable": true,
      "deletedAt": null,
      "isValid": true,
      "invalidReason": null
    }
  ]
}
~~~

الـGET يلف CartViewDto داخل AppResponse<T>.

## معاينة Checkout وتأكيده

POST /api/v1/orders/checkout/preview

~~~json
{
  "sourceType": "Cart",
  "sourceId": 14,
  "addressId": 9,
  "paymentMethod": "COD"
}
~~~

POST /api/v1/orders/checkout

~~~json
{
  "sourceType": "Cart",
  "sourceId": 14,
  "addressId": 9,
  "paymentMethod": "CashOnDelivery"
}
~~~

حقل customerId في Checkout DTO موسوم JsonIgnore؛ لا يرسله التطبيق، فالخادم يستنتجه من هوية JWT. بنية CheckoutPreviewResponseDto:

~~~json
{
  "sourceType": "Cart",
  "sourceId": 14,
  "subtotal": 70.00,
  "promotionDiscountTotal": 0.00,
  "subtotalAfterPromotions": 70.00,
  "maxRedeemablePoints": 0,
  "requestedRedeemPoints": 0,
  "appliedRedeemPoints": 0,
  "redeemValueMoney": 0.00,
  "deliveryFee": 25.00,
  "grandTotal": 95.00,
  "hasPriceChanges": false,
  "warnings": [],
  "items": [
    {
      "productId": 25,
      "unitLevel": 1,
      "quantity": 2,
      "availableQuantity": 100,
      "isAvailable": true,
      "unitPrice": 35.00,
      "discountPercent": 0.00,
      "discountAmount": 0.00,
      "finalUnitPrice": 35.00,
      "lineTotal": 70.00,
      "appliedPromotionId": null,
      "snapshotPrice": 35.00
    }
  ]
}
~~~

CheckoutResultDto الفعلي يحتوي orderId وorderNumber وsubtotal وpromotionDiscountTotal وsubtotalAfterPromotions وredeemedPoints وredeemedAmount وdiscountAmount وdeliveryFee وgrandTotal وitems. عناصر CheckoutItemResultDto:

~~~json
{
  "orderId": 1007,
  "orderNumber": "ORD-20261120100000000",
  "subtotal": 70.00,
  "promotionDiscountTotal": 0.00,
  "subtotalAfterPromotions": 70.00,
  "redeemedPoints": 0,
  "redeemedAmount": 0.00,
  "discountAmount": 0.00,
  "deliveryFee": 25.00,
  "grandTotal": 95.00,
  "items": [
    {
      "productId": 25,
      "unitLevel": 1,
      "quantity": 2,
      "unitPrice": 35.00,
      "discountPercent": 0.00,
      "discountAmount": 0.00,
      "finalUnitPrice": 35.00,
      "lineTotal": 70.00,
      "appliedPromotionId": null
    }
  ]
}
~~~

Preview/Confirm لا يعرّفان PreviewId أو Version أو Idempotency-Key في DTOs الحالية. OrderService يفتح Transaction للتأكيد ويطالب بمصدر السلة/الوصفة لمنع إعادة تأكيد مصدر استُخدم؛ لا توجد آلية idempotency عامة عبر header. المعاينة لا تنشئ order ولا تحجز السعر.

## رفع وصفة طبية

POST /api/v1/prescriptions، Content-Type multipart/form-data:

- storeId: رقم صحيح.
- files: قائمة IFormFile، من صورة إلى خمس صور.
- primaryIndex: اختياري.
- notes: اختياري.
- الأنواع المقبولة في Controller: JPEG/JPG أو PNG أو WebP؛ 10 MB لكل ملف و25 MB إجماليًا.

~~~bash
curl -X POST "https://localhost:7001/api/v1/prescriptions" \
  -H "Authorization: Bearer <accessToken>" \
  -F "storeId=1" \
  -F "primaryIndex=0" \
  -F "notes=يرجى مراجعة الوصفة" \
  -F "files=@prescription-front.jpg;type=image/jpeg" \
  -F "files=@prescription-back.jpg;type=image/jpeg"
~~~

## مثال استجابة Validation

~~~json
{
  "isSuccess": false,
  "title": "Validation Failed",
  "type": "https://httpstatuses.com/400",
  "statusCode": 400,
  "detail": "Validation failed",
  "instance": "/api/v1/addresses",
  "location": null,
  "errorCode": "ValidationError",
  "traceId": "0HABCDEF123456:00000001",
  "timestamp": "2026-11-20T10:00:00Z",
  "errors": ["governorateAreaId: invalid"],
  "fieldErrors": {
    "governorateAreaId": ["invalid"]
  }
}
~~~

الحقول النهائية لـAppResponse موضحة في API_ERRORS.md. لا تستخدم هذا المثال بدل استجابة المسار حين يكون الـAction يعيد ملفًا أو DTO مباشرًا.

