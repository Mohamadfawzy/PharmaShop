# Mobile API Contract — .NET MAUI

هذا الدليل المختصر لمطور MAUI. المرجع الشامل هو API_ENDPOINTS_OVERVIEW.md وAPI_ENDPOINTS_DETAILS.md؛ نماذج الطلب في request-models.md وأمثلة JSON في API_REQUEST_RESPONSE_EXAMPLES.md.

## الإعداد

- عنوان الخدمة يختلف حسب البيئة؛ في Postman المحلي القيمة المعتادة https://localhost:7001 وقد تختلف حسب تشغيل WebAPI.
- أرسل JSON بترميز UTF-8، وAuthorization: Bearer {accessToken} فقط للمسارات التي تتطلب JWT.
- للـmultipart لا تثبت Content-Type يدويًا؛ اترك HttpClient يضيف boundary.
- احتفظ بالتوكن في SecureStorage، ولا تخزنه في ملفات إعداد MAUI أو السجلات.

## تسجيل الدخول

POST /api/auth/login بجسم identifier/password/rememberMe. الرد AppResponse<LoginResponseDto> ويحتوي data.userId وdata.accessToken وdata.expiresAtUtc. لا يوجد refreshToken أو logout أو OTP endpoint في التنفيذ الحالي. عند انتهاء Access Token يحتاج العميل تسجيل الدخول مجددًا.

مستخدم Customer يصل إلى وظائف العميل. الحصول على بيانات الهوية والأذونات: GET /api/v1/me وGET /api/v1/me/permissions. ملف الموظف عبر GET /api/v1/me/employee يتطلب Employee أو DeliveryAgent.

## تدفق الكتالوج والعنوان

1. اجلب GET /api/v1/locations/governorates ثم GET /api/v1/locations/governorates/{id}/areas.
2. اعرض المتاجر ومناطق التوصيل العامة، ثم المنتجات والتصنيفات والوسوم والشركات والوحدات والعروض.
3. أنشئ عنوانًا عبر POST /api/v1/addresses باستخدام governorateId وgovernorateAreaId وstreet وlandmark وisDefault.
4. لا ترسل CustomerId؛ العنوان مربوط بالمستخدم الحالي. منطقة العنوان يجب أن تكون نشطة وتتبع المحافظة المختارة.

## تدفق السلة وCheckout

1. POST /api/v1/cart/items لإضافة عنصر عادي أو /api/v1/cart/redeem/items لاسترداد بالنقاط.
2. GET /api/v1/cart لقراءة الأسعار الحالية ولقطات سعر العنصر وحالة الصلاحية والمخزون.
3. POST /api/v1/cart/preview لمعاينة السلة ورسوم التوصيل.
4. POST /api/v1/orders/checkout/preview يراجع المصدر والعنوان والدفع ويعرض الإجماليات والتحذيرات وتغييرات السعر.
5. POST /api/v1/orders/checkout للتأكيد النهائي؛ أعد قراءة الطلب إذا انقطع الاتصال قبل استجابة النجاح. الخدمة تستخدم Transaction وتطالب بمصدر السلة/الوصفة لمنع إعادة تأكيد مصدر مستهلك.
6. تابع GET /api/v1/orders وGET /api/v1/orders/{id}. عمليات الإلغاء والإرجاع محكومة بحالة الطلب.

Checkout DTOs لا تتضمن PreviewId أو Version أو Idempotency-Key. لا تعرض المعاينة كحجز للسعر أو المخزون.

## الوصفات

- الرفع: POST /api/v1/prescriptions multipart/form-data بحقل storeId، قائمة files (1 إلى 5)، وnotes/primaryIndex اختياريان.
- الملفات المسموحة JPEG/JPG وPNG وWebP؛ 10 MB للصورة و25 MB إجماليًا.
- اقرأ GET /api/v1/prescriptions وGET /api/v1/prescriptions/{id}; الصور عبر /api/v1/prescriptions/{id}/images/{imageIdentifier}.
- الموظف يراجع عبر /api/v1/employee/prescriptions/* ويتطلب CanReviewPrescriptions. قيم الحالة أرقام byte وليست Enum موحدًا، لذا لا تخمّن حالات جديدة.

## Responses والأخطاء

- تحقق من HTTP status وContent-Type أولًا. معظم ردود الخدمات AppResponse، لكن الملف/صور التطوير أو تحديات JWT قد لا تتبع Envelope.
- نجاح معظم Controllers هو HTTP 200؛ لا تستنتج 201 من body.statusCode وحده.
- أخطاء ModelState ترجع HTTP 400 مع errors وfieldErrors. 401 يعني مصادقة مفقودة/منتهية؛ 403 يعني دورًا أو permission غير كافٍ؛ 404 مورد غير متاح/غير موجود؛ 409 تعارض.
- سمات JSON غالبًا camelCase. Enums المعرفة ترسل نصًا مثل sourceType: "Cart"؛ بعض الحقول مثل status/unitLevel في Responses نوعها byte فترسل أرقامًا.
- حقول null قد تظهر في JSON؛ اجعل نماذج العميل nullable حسب DTO.

## قائمة Features للعميل

| Feature | المسارات |
|---|---|
| المصادقة | /api/auth/* |
| الحساب | /api/v1/me, /api/v1/me/permissions |
| المنتج والتصنيف | /api/v1/products, /api/v1/categories, /api/v1/tags, /api/v1/companies, /api/v1/units |
| المتجر والعرض | /api/v1/stores, /api/v1/delivery-zones, /api/v1/promotions/* |
| العناوين | /api/v1/addresses/*, /api/v1/locations/* |
| السلة/الشراء/الطلب | /api/v1/cart/*, /api/v1/orders/* |
| الوصفات | /api/v1/prescriptions/* |
| نقاط العميل | /api/v1/points/* |
| الإشعارات | /api/v1/notifications/* |
| المرتجعات | /api/v1/orders/{orderId}/returns* |
| الموظف والمندوب | /api/v1/me/employee, /api/v1/employee/prescriptions/*, /api/v1/delivery/orders/* |

