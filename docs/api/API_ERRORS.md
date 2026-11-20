# API Errors and Response Wrappers

## أنواع الاستجابة الموجودة

| النوع | الخصائص الفعلية |
|---|---|
| `AppResponseBase` | `isSuccess`, `title`, `type`, `statusCode`, `detail`, `instance`, `location`, `errorCode`, `traceId`, `timestamp`, `errors`, `fieldErrors` |
| `AppResponse` | الخصائص الموروثة من `AppResponseBase`؛ لا يوجد `data` أو `pagination`. |
| `AppResponse<T>` | الخصائص الموروثة، ثم `data` و`pagination`. |
| `PagedResult<T>` | `items`, `totalCount`. يستخدم كبيانات داخل الاستجابة عندما تختاره الخدمة. |
| `PaginationInfo` | `currentPage`, `pageSize`, `totalCount`, `totalPages`, `startRecord`, `endRecord`, `hasPrevious`, `hasNext`, `previousPage`, `nextPage`. |

لا يوجد Wrapper واحد لكل Routes: المسارات المبنية على `BaseApiController` تستخدم غالبًا AppResponse؛ `AuthController` يلف نتيجة الخدمة لكنه يمرر status الفعلي؛ مسارات الصور التطويرية تُرجع DTO أو ملفًا أو نصًا مباشرة؛ Middleware المصادقة قد ينشئ جسمًا افتراضيًا من ASP.NET بدل AppResponse.

## JSON فعلي لـ AppResponse<T>

`Program.cs` يضيف `JsonStringEnumConverter` إلى MVC، لذلك Enums داخل ردود Controllers تُسلسل نصيًا مع قبول الأرقام. إعداد `DefaultIgnoreCondition.WhenWritingNull` مسجل عبر `ConfigureHttpJsonOptions` (خدمات HTTP JSON)، وليس في `AddJsonOptions` الخاص بـ MVC Controllers؛ لذلك حقول null في Controller JSON ليست مضمونة الحذف.

~~~json
{
  "isSuccess": false,
  "title": "Validation Failed",
  "type": null,
  "statusCode": 400,
  "detail": "Validation failed",
  "instance": "/api/v1/addresses",
  "location": null,
  "errorCode": "ValidationError",
  "traceId": "0HABCDEF123456:00000001",
  "timestamp": "2026-11-20T10:00:00Z",
  "errors": [
    "governorateAreaId: The selected area is invalid"
  ],
  "fieldErrors": {
    "governorateAreaId": [
      "The selected area is invalid"
    ]
  },
  "data": null,
  "pagination": null
}
~~~

`data` و`pagination` موجودتان فقط في النوع العام `AppResponse<T>`. قد تعرضهما MVC كـ`null` بدل الحذف.

## JSON الفعلي لأخطاء التحقق الآلي

`[ApiController]` يخصص `InvalidModelStateResponseFactory` في `Program.cs` ويحول أخطاء ModelState إلى `AppResponse.ValidationErrors` مع HTTP 400. يحفظ مفاتيح الحقول كما أتت من ModelState ولا يفرض camelCase عليها.

~~~json
{
  "isSuccess": false,
  "title": "Validation Failed",
  "type": "https://httpstatuses.com/400",
  "statusCode": 400,
  "detail": "Validation failed",
  "instance": "/api/auth/register",
  "location": null,
  "errorCode": "ValidationError",
  "traceId": "0HABCDEF123456:00000001",
  "timestamp": "2026-11-20T10:00:00Z",
  "errors": [
    "Email: The Email field is required.",
    "Password: The Password field is required."
  ],
  "fieldErrors": {
    "Email": ["The Email field is required."],
    "Password": ["The Password field is required."]
  }
}
~~~

## JSON للاستثناء غير المتوقع

`ExceptionHandlingMiddleware` يرجع HTTP 500 و`AppResponse`؛ في Development تكون `detail` مساوية لرسالة الاستثناء، وفي Production رسالة عامة. هذا المسار ينشئ `JsonSerializerOptions` مستقلًا مع CamelCase فقط، ولا يضيف `JsonStringEnumConverter`، لذا `errorCode` هنا قد يظهر رقمًا بينما يظهر نصًا في ردود MVC.

~~~json
{
  "isSuccess": false,
  "title": "Internal Server Error",
  "type": "https://httpstatuses.com/500",
  "statusCode": 500,
  "detail": "Something went wrong. Please try again later.",
  "instance": "/api/v1/products/1",
  "location": null,
  "errorCode": 900,
  "traceId": "0HABCDEF123456:00000001",
  "timestamp": "2026-11-20T10:00:00Z",
  "errors": ["An internal error occurred"],
  "fieldErrors": null
}
~~~

هذا مثال Production؛ تفاصيل الاستثناء لا تُرسل للعميل في Production. الميدلوير يسجل الاستثناء داخليًا مع المسار والطريقة وTraceId.

## HTTP Status Codes

| Status | متى يظهر في الكود |
|---|---|
| 200 | أغلب نجاحات `FromAppResponse`، نجاح login، ونجاحات API العامة. |
| 201 | Response factory يدعم `Created`، و`AuthController.Register` يمرر StatusCode من الخدمة مباشرة. لاحظ أن `BaseApiController` يعيد 200 حتى مع Wrapper يحمل 201. |
| 204 | Factory `AppResponse.NoContent` موجود، لكن `FromAppResponse` يعيد Ok مع جسم للنجاح؛ لا تستخدم 204 على أنه مضمون قبل فحص Action. |
| 400 | أخطاء ModelState والتحقق وقواعد العمل المعادة كـValidationError/BusinessRuleViolation. |
| 401 | JWT challenge أو AppResponse Unauthorized في خدمة المصادقة. جسم تحدي JWT ليس AppResponse مضمونًا. |
| 403 | مستخدم موثق يفتقد الدور/الصلاحية، أو حساب معطل في مسار الدخول. |
| 404 | مورد غير موجود في الخدمات، أو عمليات DevImages خارج Development. |
| 409 | تعارض موارد/تكرار عندما تعيده الخدمة صراحة. |
| 422 | ليس Default مستخدمًا في ResponseDefaults؛ لا تفترض وجوده. |
| 429 | لا تظهر Middleware/RateLimiter مفعلة في Program الحالي؛ وجود enum خطأ وحده لا يعني أن API تطبقه. |
| 500 | أخطاء غير متوقعة عبر ExceptionHandlingMiddleware؛ وبعض الخدمات تعيد AppResponse من نفسها. |
| 499 | العميل ألغى الاتصال؛ Middleware يحاول تعيين 499 ولا يكتب Wrapper. |

## Error codes

`Shared/Enums/AppErrorCode.cs` يحتوي قيمًا كثيرة: `None`, `ValidationFailed`, `ValidationError`, `RequiredFieldMissing`, `InvalidFormat`, `ExceedsMaxLength`, `BelowMinLength`, `InvalidRange`, `InvalidEnumValue`, `BusinessRuleViolation`, `Unauthorized`, `Forbidden`, `TokenExpired`, `InvalidCredentials`, `AccountLocked`, `AccountNotConfirmed`, `AccessDenied`, `NotFound`, `BadRequest`, `EntityNotFound`, `ResourceNotFound`, `UserNotFound`, `PageNotFound`, `Conflict`, `DuplicateRecord`, `AlreadyExists`, `EmailAlreadyExists`, `UsernameAlreadyExists`, `PhoneAlreadyExists`, `OperationConflict`, `OperationNotAllowed`, `InvalidOperation`, `PrerequisiteNotMet`, `BusinessRuleViolated`, `OrderAlreadyCompleted`, `SubscriptionExpired`, `DatabaseError`, `DbUpdateFailed`, `ConstraintViolation`, `RecordLocked`, `ExternalServiceUnavailable`, `ExternalServiceTimeout`, `ExternalServiceError`, `PaymentGatewayError`, `SmsProviderError`, `EmailProviderError`, `NetworkError`, `Timeout`, `ServiceUnavailable`, `TooManyRequests`, `InternalServerError`, `UnknownError`, `UnhandledException`, `NotImplemented`, `SerializationError`.

القيمة المعلنة في Enum لا تعني أن كل خدمة تستخدمها. `ResponseDefaults` يربط مجموعة منها بـ400/401/403/404/409/500، والافتراضي لأي رمز غير مربوط هو 400؛ راجع [API_ENUMS.md](API_ENUMS.md).

## TraceId

`TraceIdResultFilter` يضيف `X-Trace-Id` إلى Response MVC، ويعبئ `traceId` إن كان الجسم AppResponse. Exception middleware يضع TraceId في JSON بنفسه. Responses التي يكتبها Authentication/Authorization middleware أو ملفات مباشرة لا تضمن نفس Envelope.

