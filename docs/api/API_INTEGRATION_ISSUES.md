# API Integration Issues and Audit Findings

هذا تقرير ملاحظات فقط؛ لم أعدّل أي كود أو DTO أو قاعدة بيانات ضمن مهمة التوثيق. الأولوية تعكس أثرها على تكامل العميل أو Swagger.

## High

| المشكلة | الدليل في المصدر | الأثر |
|---|---|---|
| نجاح 201/204 في AppResponse لا يتحول إلى HTTP status المقابل في أغلب Controllers | WebAPI/Controllers/BaseApiController.cs:16-24 يعيد Ok(response) لكل Success | العميل قد يرى HTTP 200 مع body.statusCode=201 أو 204؛ لا تعتمد على statusCode الداخلي وحده. |
| إعداد Swagger المخصص قد يستبعد كل Controllers من المستندات الثلاثة | WebAPI/Extensions/SwaggerExtensions.cs:16 يقبل فقط apiDesc.GroupName == documentName؛ لا توجد ApiExplorer GroupName attributes في Controllers الحالية | مستند Pharma/Inventory/v1 قد يكون فارغًا. تأكد عند نشر Swagger؛ مشروع OpenAPI المدمج منفصل ومربوط ببيئة Development فقط. |
| اختيار الصيدلية داخل تسجيل الدخول ثابت على PharmacyId=1 | WebAPI/Controllers/Auth/AuthController.cs:62 | الـJWT وLogin audit قد ينسبان كل الحسابات إلى tenant واحد، ما لا يناسب تعدد الصيدليات. |
| مفتاح JWT قديم موجود كنص داخل تعليق | WebAPI/Extensions/ServiceExtensions.cs:209 | التعليق لا يستخدم المفتاح في التشغيل الحالي، لكن أي قيمة توقيع حقيقية سابقة ينبغي معاملتها كسر مكشوف والتحقق من تدويرها. لم أنقل القيمة إلى هذا التقرير. |

## Medium

| المشكلة | الدليل في المصدر | الأثر |
|---|---|---|
| AppErrorCode يختلف نوعه بين Responses | Program.cs يضيف JsonStringEnumConverter لـMVC، بينما ExceptionHandlingMiddleware.cs:60 ينشئ JsonSerializerOptions منفصلًا دون converter | errorCode قد يكون نصًا في أخطاء MVC ورقمًا في 500 غير المتوقع. |
| null-omission مسجل عبر HTTP JSON options لا MVC JSON options | WebAPI/Program.cs:51-57 و83-87 | لا تضمن أن خصائص null تختفي من ردود Controllers؛ بعض العملاء يجب أن يقبلوا null. |
| Response shape لا يوحد كل المسارات | BaseApiController يلف AppResponse؛ Authentication/Authorization middleware وDevImages تعيد أجسامًا/ملفات مباشرة | على عميل MAUI فحص Content-Type وstatus قبل Deserialize إلى AppResponse<T>. |
| حالات كثيرة في Requests/Responses معرفة byte بدل Enum موحد | OrderStatusUpdateDto وPrescriptionStatusUpdateDto وPayment DTOs وQuery DTOs | التكامل يعتمد أرقامًا، وأسماء الحالات وانتقالاتها ليست موحدة بعقد واحد. |
| Page/pageSize مختلفان بين الوحدات، وبعض القوائم المرجعية غير paged | RequestParameters وDTOs المتخصصة وServices القائمة | يجب تثبيت pagination لكل شاشة حسب Endpoint؛ لا تستخدم Query DTO موحدًا عشوائيًا. |
| حقول RequestParameters موثقة جزئيًا في الدليل السابق | Shared/Models/RequestFeatures/RequestParameters.cs يعرّف pageNumber افتراضيًا 1 وpageSize افتراضيًا 10 وحده الأعلى 50 | تم توثيق حقول هذا النموذج في request-models.md؛ تذكر أن حد 50 يخص هذا النموذج فقط. |
| Checkout يملك Transaction وحجز مصدر لمنع إعادة التأكيد، لكنه لا يستخدم Idempotency-Key | Service/OrderService.cs:148 و179 وTryClaimCheckoutSourceAsync | لا ترسل المفتاح على أنه مطلوب. عند انقطاع الاستجابة بعد commit، أعد قراءة الطلب/المصدر قبل إعادة إرسال checkout؛ المصدر بعد المطالبة يمنع تكرارًا عاديًا، لكنه ليس عقد idempotency عامًا. |
| rememberMe موجود في LoginRequestDto ولا يظهر مستخدمًا في AuthService | Shared/Models/Auth/LoginRequestDto.cs وService/Auth/AuthService.cs | إرسال true لا يمدد Access Token ولا ينشئ Refresh Token؛ مدة JWT تظل من إعداد AccessTokenMinutes. |
| رفع الوصفة يبدأ بكتابة prescription ثم يحفظ الصور على القرص والسجلات، ويعوض الإخفاق بتنظيف best-effort | Service/PrescriptionService.cs:62-145 | لا توجد Transaction موحدة تجمع SQL ونظام الملفات؛ فشل تنظيف الملف قد يترك ملفًا يتيمًا. |
| مسارات dev images غير ظاهرة في Swagger عمدًا | DevImagesController.cs:9-12 | يجب عدم استنتاج غيابها من OpenAPI، وعدم استخدامها كتدفق إنتاج؛ API الصور الإدارية هي المسارات الأخرى. |

## Low / توثيق وتشغيل

| الملاحظة | الدليل | الأثر |
|---|---|---|
| endpoint لتأكيد البريد موجود بصيغتي GET وPOST، وGET يضع token في Query | AuthController | روابط البريد قد تسجل token في سجلات الوكيل/المتصفح؛ قلل مدة صلاحية رمز Identity واحرص على عدم تسجيل query الحساسة. |
| Categories القديمة ونسخ Actions السابقة في Products/AdminCategories موجودة داخل التعليقات | CategoriesController.cs وProductsController.cs وAdminCategoriesController.cs | ليست Routes فعالة ولا تدخل جرد الـAPI الحالي؛ يجب عدم استدعائها من العملاء. |
| Tag route يستخدم حالة أحرف كبيرة في المسار | TagsController.cs:9 | استخدم المسار كما هو في دليل الإدارة، وتحقق من إعدادات proxy التي قد تكون حساسة لحالة الأحرف. |
| لا يوجد Refresh Token أو OTP أو logout/revocation | Auth DTOs/Controller/Service | دورة الجلسة الحالية تعتمد انتهاء Access Token ثم تسجيل دخول جديد. |
| الدفع يمثل سجلات محاولات واسترداد فقط ولا يتكامل مع مزود | Payment DTOs/Service | لا تعتبر إنشاء محاولة دفع إثباتًا أن الأموال سُحبت أو أعيدت. |

### تفاصيل Pagination التي يجب أن يراعيها العميل

- `RequestParameters` يستخدم `pageNumber` (افتراضي 1) و`pageSize` (افتراضي 10، ويقص setter القيم الأكبر من 50). هذا النمط لا ينطبق تلقائيًا على جميع endpoints.
- توجد Query DTOs أخرى بأسماء `page` أو `pageNumber`، وقيم افتراضية وأقصى مختلفة؛ مثل خدمات منتجات/عروض/وصفات تقبل حتى 200، وفئات/توصيل/مخزون وإشعارات ونقاط ومرتجعات تقبل عادة حتى 100.
- بعض الخدمات تطبّع page/pageSize إلى مجال مقبول، بينما خدمة التصنيفات تتحقق من القيم وتعيد validation error عندما تكون خارج المجال. لا تفترض سلوكًا موحدًا للقيم صفر أو السالبة.
- استخدم لكل endpoint اسم query parameters وحدودها المبينة في صفه ضمن [API_ENDPOINTS_DETAILS.md](API_ENDPOINTS_DETAILS.md)، واقرأ بيانات `pagination` من الاستجابة بدل حساب الصفحات من حجم القائمة الحالية.

## OpenAPI export

المشروع يسجل Microsoft.AspNetCore.OpenApi، ويستدعي MapOpenApi فقط عندما تكون البيئة Development؛ المسار المتوقع وقت التشغيل هو /openapi/v1.json. حاولت توليده من التطبيق في بيئة العمل، لكن النظام منع الاتصال المحلي بـ127.0.0.1 بسبب قيود socket في sandbox. لذلك لم أضع ملف openapi-export.json مضللًا أو بعقود ناقصة. عند تشغيل WebAPI محليًا في Development يمكن حفظ JSON من هذا المسار مباشرة؛ يجب بعدها مقارنة count ومساراته مع الجرد في API_ENDPOINTS_OVERVIEW.md، مع ملاحظة أن /api/dev/images مستبعد صراحة من ApiExplorer.

## ما لم أعدّله

لم أصلح الملاحظات، ولم أغير Checkout أو السلة أو العروض أو الوصفات، ولم أغير أي DTO أو Endpoint. المستندات توثق السلوك الحالي فقط.

