# Promotional Banners — عقد الواجهة

هذا الدليل لوحدة البنرات في الهاتف ولوحة الإدارة. المسارات الإدارية تتطلب JWT بدور Admin؛ المسار العام لا يتطلب تسجيل الدخول.

## المسارات

| Method | Route | الغرض |
|---|---|---|
| GET | `/api/v1/banners?placement=Home` | قائمة عامة حسب الموضع؛ placement اختياري. |
| GET | `/api/v1/admin/banners` | بحث إداري بـ q, placement, isActive, page, pageSize. |
| GET | `/api/v1/admin/banners/{id}` | تفاصيل بنر نشط وغير محذوف منطقيًا. |
| POST | `/api/v1/admin/banners` | إنشاء ورفع صورة عبر multipart/form-data. |
| PUT | `/api/v1/admin/banners/{id}` | تحديث البيانات وصورة اختيارية. |
| DELETE | `/api/v1/admin/banners/{id}` | حذف منطقي مع تعطيل البنر. |
| PATCH | `/api/v1/admin/banners/{id}/active` | التفعيل أو التعطيل. |
| PATCH | `/api/v1/admin/banners/reorder` | تحديث ترتيب عدة بنرات بمعاملة واحدة. |

## Enums والأهداف

- `BannerPlacement`: `Home`, `Offers`, `Categories`.
- `BannerActionType`: `None`, `Product`, `Offer`, `Category`, `Brand`, `ExternalUrl`.

| ActionType | TargetId | ParentTargetId | ActionUrl |
|---|---|---|---|
| None | لا يرسل | لا يرسل | لا يرسل |
| Product | Product.Id موجود ونشط | لا يرسل | لا يرسل |
| Offer | Promotion.Id موجود وغير محذوف | لا يرسل | لا يرسل |
| Category | Category.Id موجود ونشط | اختياري؛ يجب أن يطابق الأب الفعلي | لا يرسل |
| Brand | Company.Id موجودة ونشطة | لا يرسل | لا يرسل |
| ExternalUrl | لا يرسل | لا يرسل | URI مطلق HTTPS |

`Company` هي بيانات العلامة/الشركة الموجودة في قاعدة المشروع. TargetId علاقة متعددة الأنواع؛ يتحقق Service من وجود الهدف في جدوله. ParentTargetId متاح للتصنيف الفرعي فقط.

## إنشاء بنر وتحديثه

`POST` و`PUT` يستخدمان `multipart/form-data` بحقول مسطحة بهذه الأسماء:

| Field | النوع | القاعدة |
|---|---|---|
| name | string | مطلوب، حتى 150 حرفًا. |
| placement | enum string | Home أو Offers أو Categories. |
| actionType | enum string | إحدى القيم الست أعلاه. |
| targetId | int? | إلزامي حسب ActionType؛ ويجب أن يشير إلى سجل صالح. |
| parentTargetId | int? | للتصنيف الفرعي؛ يجب أن يكون الأب الحقيقي. |
| actionUrl | string? | إلزامي فقط لـ ExternalUrl، ويجب أن يكون HTTPS. |
| image | file | مطلوب في POST واختياري في PUT؛ JPEG/PNG/WebP حتى 10 MB. |
| altText | string? | حتى 200 حرف. |
| displayOrder | int | صفر أو موجب؛ تصاعديًا. |
| isActive | bool | الافتراضي true عند الإنشاء. |
| startsAt / endsAt | ISO-8601 UTC? | اختياريان؛ End بعد Start إذا أرسلا معًا. null يعني بلا حد زمني. |

في حالة غياب `placement` و`actionType` يأخذان `Home` و`None` (القيمة الافتراضية للـenum). البحث الإداري يبدأ من `page=1` و`pageSize=20`؛ الخدمة تقيد pageSize بين 1 و100.

مثال بنر منتج في form-data: `name=خصم على المنتج`, `placement=Home`, `actionType=Product`, `targetId=12`, `displayOrder=1`, `isActive=true`, `image=[اختر الملف]`.

مثال تصنيف فرعي: `placement=Categories`, `actionType=Category`, `targetId=31`, `parentTargetId=8` مع بقية الحقول والصورة.

مثال رابط خارجي: `placement=Offers`, `actionType=ExternalUrl`, `actionUrl=https://example.com/campaign` مع بقية الحقول والصورة.

عند تحديث بنر لا ترسل ملف `image` إن كنت تريد الاحتفاظ بالصورة الحالية. الحد الأعلى لحجم HTTP request نحو 11 MB.

## سلوك GET العام والصور

- `GET /api/v1/banners?placement=Home`؛ القيم الأخرى Offers وCategories. من دون placement يعيد جميع المواضع.
- يعيد البنرات `IsActive=true` وغير المحذوفة منطقيًا فقط، وفي نافذة `StartsAt`/`EndsAt` حسب UTC. قيمة تاريخ null لا تفرض حدًا.
- الترتيب `DisplayOrder ASC` ثم `Id ASC`.
- `imageUrl` و`thumbnailUrl` مطلقان وصالحان للهاتف، مثل `https://api.example.com/uploads/original/banner-id.webp` و`https://api.example.com/uploads/small/banner-id.webp`.
- استخدم الإعداد الاختياري `Banner:PublicBaseUrl` عند الاستضافة خلف Proxy أو نطاق ثابت. عند غيابه يستخدم API عنوان الطلب الحالي.
- يخزن الجدول الروابط النسبية. الحذف المنطقي يحتفظ بالملف؛ استبدال الصورة يحفظ الجديدة ثم يحذف القديمة بعد نجاح تعديل DB.

## Response DTO

كل النتائج داخل `AppResponse<T>` المعتاد. `BannerDto` يحتوي: `id`, `name`, `placement`, `actionType`, `targetId`, `parentTargetId`, `actionUrl`, `imageUrl`, `thumbnailUrl`, `altText`, `displayOrder`, `isActive`, `startsAt`, `endsAt`, `createdAt`, `updatedAt`.

مثال رد القائمة العامة:

~~~json
{
  "isSuccess": true,
  "title": "Banners retrieved successfully",
  "statusCode": 200,
  "data": [
    {
      "id": 14,
      "name": "خصم على المنتج",
      "placement": "Home",
      "actionType": "Product",
      "targetId": 12,
      "parentTargetId": null,
      "actionUrl": null,
      "imageUrl": "https://api.example.com/uploads/original/banner-id.webp",
      "thumbnailUrl": "https://api.example.com/uploads/small/banner-id.webp",
      "altText": "عرض خاص",
      "displayOrder": 1,
      "isActive": true,
      "startsAt": null,
      "endsAt": null,
      "createdAt": "2026-11-22T08:00:00Z",
      "updatedAt": null
    }
  ],
  "pagination": null
}
~~~

استجابة POST وPUT تعيد BannerDto في `data`. نجاح POST قد يحمل status داخليًا 201، لكن Controller convention الحالي يعيد HTTP 200. قائمة الإدارة تعيد `pagination`.

## التفعيل والترتيب

`PATCH /api/v1/admin/banners/14/active` body:

~~~json
{"isActive": false}
~~~

`PATCH /api/v1/admin/banners/reorder` body:

~~~json
{"items":[{"id":14,"displayOrder":0},{"id":18,"displayOrder":1}]}
~~~

يجب أن تكون Ids فريدة وموجودة وغير محذوفة، والترتيب غير سالب. إذا فشل أي سجل لا يحفظ ترتيب جزئي.

## أخطاء متوقعة وتوجيه UI

- 400 عند TargetId غير مناسب أو غير موجود، ActionUrl غير HTTPS، حقول متعارضة، تاريخ غير صالح أو صورة غير مقبولة.
- 404 عند عدم وجود بنر أو كونه محذوفًا منطقيًا.
- بعد update/active/reorder أعد تحميل قائمة البنرات أو أبطل cache.
- استخدم enum `actionType` لتحديد التنقل، ولا تستنتجه من وجود TargetId.
- صفحة الإدارة تعرض `thumbnailUrl`، والتطبيق يستخدم `imageUrl` عند عرض الصورة الكبيرة.

## Postman وقاعدة البيانات

- Collection: [PharmaShop-Promotional-Banners](../../postman/promotional-banners/PharmaShop-Promotional-Banners.postman_collection.json)؛ خطوات التشغيل في [README مجلد Postman](../../postman/promotional-banners/README.md).
- أنشئ جدول الوحدة بتشغيل `SQLScripts/new/15 - Promotional Banners.sql` بعد مخطط قاعدة البيانات. السكربت مستقل وآمن لإعادة التشغيل.
- لا تغيّر هذه الوحدة جداول أو سلوك السلة أو Checkout أو عروض Promotion الحالية؛ عروض Banner تستهدف Promotion بمعرف فقط.
- المشروع لا يستخدم نمط MediatR/CQRS handlers؛ `BannerService` يؤدي دور معالج طلبات المجال وفق نمط الخدمات الموجود.
