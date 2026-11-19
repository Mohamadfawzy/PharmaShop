# القواعد المشتركة للـ API

## عنوان الخدمة والمسارات

- معظم مسارات الأعمال تحت /api/v1.
- المصادقة وإدارة الأدوار تحت /api/auth و/api/admin.
- استخدم المسارات كما تظهر في الجداول، بما فيها حالة الأحرف المعرفة في المصدر، مثل /api/v1/admin/Products و/api/v1/admin/Tags.
- عنوان API يتحدد حسب بيئة التشغيل؛ قيمة Postman الافتراضية https://localhost:7001 وقد تختلف عند تشغيل WebAPI.

## المصادقة

للطلبات المحمية أرسل الترويسة:

~~~http
Authorization: Bearer <accessToken>
~~~

تسجيل الدخول عبر POST /api/auth/login. الاستجابة تحتوي عادة على data.accessToken وdata.userId وdata.expiresAtUtc داخل AppResponse. خزن التوكن في مخزن آمن، ولا تضعه أو كلمة المرور في ملفات المصدر أو سجلات التطبيق.

## الأدوار والصلاحيات

| الدور أو الصلاحية | الاستخدام |
|---|---|
| Admin | Controllers التي ترث AdminBaseApiController، وإدارة الأدوار والصلاحيات. |
| Customer | السلة والعناوين والطلبات والوصفات الشخصية والنقاط والإشعارات. |
| Employee | بيانات الموظف الحالية. مراجعة الوصفات تعتمد على CanReviewPrescriptions. |
| DeliveryAgent | طلبات المندوب؛ يلزم أيضًا اجتياز CanDeliverOrders. |
| CanManageOrders | إدارة الطلبات وإسناد المندوب والمرتجعات وسجلات الدفع. |
| CanManageInventory | استلام المخزون والتسوية وسجل الحركات. |
| CanManagePoints | إضافة/خصم النقاط يدويًا. |
| CanReviewPrescriptions | قائمة مراجعة الوصفات وإدارتها. |
| CanDeliverOrders | تحديث حالة التوصيل. |

سياسات الصلاحيات التشغيلية تقبل Admin أو claim permission مناسبًا. بعض Controllers تتطلب أكثر من سياسة؛ مثال المندوب يتطلب دور DeliveryAgent وكذلك CanDeliverOrders. واجهات إدارة الأدوار والصلاحيات نفسها Admin-only. عند 403 افحص دور المستخدم والـ claims الموجودة في JWT.

## شكل الاستجابة

معظم عمليات الأعمال ترجع AppResponse أو AppResponse<T>، والبيانات تكون غالبًا تحت data:

~~~json
{
  "isSuccess": true,
  "title": "Success",
  "statusCode": 200,
  "detail": null,
  "data": {},
  "pagination": null,
  "location": null,
  "errorCode": "None",
  "errors": [],
  "fieldErrors": null,
  "traceId": "...",
  "timestamp": "2026-11-20T10:00:00Z"
}
~~~

هذه عينة شكلية؛ data وpagination يختلفان باختلاف المسار. عند الخطأ افحص HTTP status وisSuccess وerrors وfieldErrors. يضيف الخادم ترويسة X-Trace-Id. قد ترجع بعض endpoints ملفًا مباشرًا أو 204 No Content أو كائنًا غير ملفوف؛ لا تفترض شكلًا واحدًا لكل الردود.

## JSON وEnums

يقبل إعداد JSON أسماء الـ enums والقيم الرقمية. استخدم القيم المعرفة في DTO/enum للمجال نفسه:

- UnitLevel: Outer=1 وInner=2.
- CheckoutSourceType: Cart=1 وPrescription=2 وPointsRedemption=3.
- PaymentMethod حاليًا: CashOnDelivery=0 وCard=1 وCOD=2. لا تعتبر وجود Card تكاملًا مع بوابة دفع.

قد تختلف DTOs pagination والفرز والمرشحات حسب المجال. أسماء Query DTO في جداول endpoints تشير إلى تعريف المصدر الذي يجب الرجوع إليه عند استكمال حقول الفلترة.

## الملفات والصور

استخدم multipart/form-data، ودع HTTP client يضبط Content-Type والـ boundary. صور الوصفات تقبل من 1 إلى 5 صور JPG/JPEG أو PNG أو WebP، حد 10 MB لكل صورة و25 MB إجماليًا. /api/dev/images للتطوير فقط ويتطلب Admin وبيئة Development. لا تستخدمه في تطبيق الإنتاج.

## رموز HTTP الشائعة

- 401: التوكن مفقود أو غير صالح أو منتهي.
- 403: المستخدم موثق لكن لا يحمل الدور أو الصلاحية المطلوبة.
- 400: تحقق من الحقول وأنواعها والـ enums وfieldErrors.
- 404: المعرف غير موجود أو المورد غير مسموح للمستخدم الحالي.
- 409: تعارض أو قاعدة أعمال، مثل مرجع مكرر أو انتقال حالة غير مسموح.

## حدود التكامل الحالية

- تكامل ERP مؤجل؛ لا تفترض أسماء جداول ERP أو عقد مزامنة قبل تحديدهما.
- لم يُحدد مزود دفع. واجهات الدفع الموجودة لإدارة سجلات المحاولات والاسترداد فقط، وليست API لمزود خارجي.
- Checkout لا يستقبل CustomerId من العميل؛ يستنتجه الخادم من المستخدم الموثق.
