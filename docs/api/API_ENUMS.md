# API Enums and Fixed Values

Program.cs يضيف JsonStringEnumConverter(allowIntegerValues: true) إلى MVC: الـEnums المعرفة كـEnum تسلسل غالبًا إلى النص، ويقبل JSON النص أو القيمة الرقمية. الحقول التي نوعها byte وليست Enum تظل أرقامًا ولا يجوز تحويلها إلى أسماء دون مصدر.

## Enums المستخدمة في العقود

| Enum | القيم الرقمية | الاستخدام |
|---|---|---|
| UnitLevel | Outer=1, Inner=2 | مستوى وحدة المنتج في CartAddItemDto. |
| PaymentMethod | CashOnDelivery=0, Card=1, COD=2 | طريقة الدفع في معاينة السلة وCheckout. لا يدل Card على وجود بوابة دفع. |
| CheckoutSourceType | Cart=1, Prescription=2, PointsRedemption=3 | يحدد مصدر عناصر Checkout. |
| CartStatus | Active=1, CheckedOut=2, Abandoned=3, Expired=4, Invalid=5 | حالة السلة في المجال. |
| CartItemSourceType | Normal=1, PointsRedemption=2 | مصدر عنصر السلة. |
| PrescriptionSortOption | Newest=1, Oldest=2, StatusUpdatedDesc=3 | ترتيب قائمة مراجعة الوصفات. |
| ProductSortOption | NameAsc=0, NameDesc=1, PriceAsc=2, PriceDesc=3, Newest=4, Oldest=5, Id=6 | ترتيب بحث المنتجات. |
| TagMatchMode | Any=0, All=1 | منطق مطابقة عدة tagIds في البحث. |
| PromotionSortOption | Newest=1, Oldest=2, StartAtAsc=3, StartAtDesc=4 | ترتيب العروض. |
| BannerPlacement | Home=0, Offers=1, Categories=2 | موضع ظهور البنر. |
| BannerActionType | None=0, Product=1, Offer=2, Category=3, Brand=4, ExternalUrl=5 | نوع الإجراء عند ضغط البنر. |
| ImageOutputFormat | Auto=0, Jpeg=1, Png=2, Webp=3 | Query parameter format لمسارات رفع الصور. |
| ImageVariant | Original=0, Medium=1, Small=2 | نوع نسخة الصورة. |
| StoredImageFormat | Jpeg=1, Png=2, Webp=3 | الصيغة المخزنة metadata للصورة. |
| AppErrorCode | انظر القائمة أدناه وAPI_ERRORS.md | ErrorCode داخل AppResponse. |
| ProductOrderBy | CreatedAt=0, UpdatedAt=1, Price=2, Name=3, Barcode=4 | Enum موجود في Shared، لكنه لا يظهر كـProperty مباشر في Query DTO الحالي؛ ProductSearchQueryParams.Sort يستخدم ProductSortOption. |

## قيم المجال ذات النوع byte

أجزاء من النماذج تستخدم byte بدل Enum؛ JSON لها رقم. أمثلتها OrderStatusUpdateDto.status وUpdateDeliveryOrderStatusDto.status وPrescriptionStatusUpdateDto.status وUpdatePaymentAttemptDto.status، وحقول status في Query DTO. معاني الأرقام وانتقالات الحالة مصدرها خدمة المجال والجداول؛ لا يقدم المشروع Enum موحدًا يغطي كل هذه المسارات. راجع OrderService وDeliveryService وPrescriptionService وPaymentService قبل إرسال قيمة جديدة.

## AppErrorCode — قائمة القيم

None=0, ValidationFailed=100, RequiredFieldMissing=101, InvalidFormat=102, ExceedsMaxLength=103, BelowMinLength=104, InvalidRange=105, InvalidEnumValue=106, Unauthorized=200, Forbidden=201, TokenExpired=202, InvalidCredentials=203, AccountLocked=204, AccountNotConfirmed=205, AccessDenied=206, BusinessRuleViolation=2000, ValidationError=1000, NotFound=404, BadRequest=400, EntityNotFound=301, ResourceNotFound=302, UserNotFound=303, PageNotFound=304, Conflict=400, DuplicateRecord=401, AlreadyExists=402, EmailAlreadyExists=403, UsernameAlreadyExists=404, PhoneAlreadyExists=405, OperationConflict=406, OperationNotAllowed=500, InvalidOperation=501, PrerequisiteNotMet=502, BusinessRuleViolated=503, OrderAlreadyCompleted=504, SubscriptionExpired=505, DatabaseError=600, DbUpdateFailed=601, ConstraintViolation=602, RecordLocked=603, ExternalServiceUnavailable=700, ExternalServiceTimeout=701, ExternalServiceError=702, PaymentGatewayError=703, SmsProviderError=704, EmailProviderError=705, NetworkError=800, Timeout=801, ServiceUnavailable=802, TooManyRequests=803, InternalServerError=900, UnknownError=901, UnhandledException=902, NotImplemented=903, SerializationError=904.

## Enum في URL/JSON

- Query parameter مثل sort=PriceAsc يقبل النص عند ربط MVC للـEnum؛ يمكن أن يقبل رقمًا أيضًا بسبب allowIntegerValues=true.
- JSON لخصائص enum يكتب عادةً "unitLevel":"Outer" أو "paymentMethod":"COD"؛ الأرقام قد تُقبل لكنها أقل وضوحًا.
- الحقول من النوع byte ترسل أرقامًا مثل status: 3، لأن الكود لا يعرّف converter يترجم هذا الحقل إلى اسم حالة.
- مسار image variant نص وليس Enum model-bound، والقيم المقبولة حرفيًا original وmedium وsmall بعد تحويل النص إلى lowercase.

