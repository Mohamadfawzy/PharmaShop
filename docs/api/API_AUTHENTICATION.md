# API Authentication and Authorization

## الآلية

- التنفيذ يستخدم ASP.NET Core Identity لتسجيل المستخدم والتحقق من كلمة المرور والأدوار، وJWT Bearer لحماية الطلبات.
- أرسل `Authorization: Bearer {accessToken}` إلى المسارات المحمية. تحقق الخادم من issuer وaudience ومدة الصلاحية ومفتاح التوقيع، مع `ClockSkew` قدره 30 ثانية.
- تسجيل الدخول في `POST /api/auth/login` يقبل البريد أو اسم المستخدم عبر `identifier`، ثم يصدر Access Token فقط.
- مدة التوكن تأتي من إعداد `Jwt:AccessTokenMinutes`؛ لا توجد قيمة ثابتة في DTO، لذلك لا يمكن إعطاء مدة رقمية مستقلة عن إعداد التشغيل.
- لا يُصدر Refresh Token؛ لا يوجد Refresh أو Logout أو Token revocation endpoint. لا توجد OTP endpoints في Controllers الحالية.
- `rememberMe` موجود في `LoginRequestDto`، لكن لا يوجد Refresh Token ضمن `LoginResponseDto` ولا توجد آلية تجديد جلسة ظاهرة في الخدمة.
- البريد غير المؤكد يمنع الدخول حسب إعداد Identity وخدمة المصادقة؛ التسجيل ينشئ دور Customer ويرسل رابط تأكيد البريد.

## تدفق الحساب

| Method | Route | Auth | السلوك |
|---|---|---|---|
| POST | `/api/auth/register` | Public | ينشئ حسابًا غير مؤكد، يضيف دور Customer، وينشئ رمز تأكيد ويرسل بريدًا؛ الرد يحمل `RegisterResponseDto`. |
| POST | `/api/auth/confirm-email` | Public | يأخذ `userId` و`token` من JSON ويؤكد البريد؛ التأكيد المتكرر يرجع نجاحًا idempotent حسب `AuthService`. |
| GET | `/api/auth/confirm-email?userId=…&token=…` | Public | مسار بديل يؤكد البريد من Query. |
| POST | `/api/auth/login` | Public | يتحقق من الحساب وكلمة المرور والحالة وتأكيد البريد؛ يسجل Audit لمحاولة الدخول، ثم يصدر Access Token. |
| POST | `/api/auth/resend-confirm-email` | Public | يطلب إعادة إرسال التأكيد إلى البريد. |
| POST | `/api/auth/forgot-password` | Public | يولد رمز استعادة كلمة المرور ويرسل رسالة. |
| POST | `/api/auth/reset-password` | Public | يعيد تعيين كلمة المرور باستعمال userId والرمز وكلمة المرور الجديدة. |
| POST | `/api/auth/change-password` | Authenticated | يتطلب JWT وكلمة المرور الحالية والجديدة. |

نماذج الطلب الدقيقة موثقة في [request-models.md](request-models.md)؛ أمثلة المصادقة في [API_REQUEST_RESPONSE_EXAMPLES.md](API_REQUEST_RESPONSE_EXAMPLES.md).

## شكل بيانات Login

`LoginResponseDto` حقوله: `userId` عدد صحيح، و`accessToken` نص JWT، و`expiresAtUtc` تاريخ UTC. لا يوجد `refreshToken` أو `tokenType` فعليًا في النموذج.

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
  "traceId": "00-00000000000000000000000000000000-0000000000000000-00",
  "timestamp": "2026-11-20T10:00:00Z",
  "errors": [],
  "fieldErrors": null,
  "data": {
    "userId": 42,
    "accessToken": "<JWT issued by the API>",
    "expiresAtUtc": "2026-11-20T10:30:00Z"
  },
  "pagination": null
}
~~~

القيم أعلاه توضح شكل البيانات؛ قيمة انتهاء التوكن تعتمد إعدادات البيئة، والـJWT المثال ليس توكنًا حقيقيًا.

## Claims

`JwtTokenService.CreateAccessTokenAsync` يضيف claims التالية:

| Claim | المحتوى |
|---|---|
| `sub` | معرف المستخدم كنص. |
| `username` | اسم المستخدم. |
| `email` | البريد. |
| `email_verified` | `true` أو `false`. |
| `pharmacy_id` | معرف الصيدلية؛ تسجيل الدخول الحالي يمرر 1 ثابتًا. |
| `permission` | الصلاحيات المعروفة فقط من `AppPermissions`. |
| `ClaimTypes.Role` / role claim | أدوار Identity للحساب. |

## الأدوار والسياسات

| Role/Policy | شرط الوصول الفعلي |
|---|---|
| `Admin` | دور Admin. Controllers التي ترث `AdminBaseApiController` محمية به. |
| `Customer` | دور Customer. يحمي العناوين والسلة والطلبات والوصفات ونقاط العميل والإشعارات. |
| `Employee` | دور Employee. يستخدم للملف الحالي للموظف؛ بعض العمليات تحتاج Permission إضافية. |
| `DeliveryAgent` | دور DeliveryAgent. واجهات المندوب تشترط أيضًا `CanDeliverOrders`. |
| `CanManageProducts` | Admin أو claim permission بهذا الاسم. |
| `CanManagePromotions` | Admin أو claim permission بهذا الاسم. |
| `CanReviewPrescriptions` | Admin أو claim permission؛ توجد قيود دور موظف عند إسناد صلاحية الموظف. |
| `CanManageOrders` | Admin أو claim permission؛ إدارة الطلبات والإرجاعات والدفع والإسناد. |
| `CanDeliverOrders` | Admin أو claim permission، ومع ذلك Controller المندوب يطلب أيضًا DeliveryAgent role. |
| `CanManagePoints` | Admin أو claim permission؛ نقاط الإدارة اليدوية. |
| `CanManageInventory` | Admin أو claim permission؛ إدارة حركة المخزون. |
| `CanManageCatalog` | Admin أو claim permission وفق تسجيل السياسات. |

Endpoints الأدوار والصلاحيات تحت `/api/admin` تتطلب Admin. خدمة إدارة permission تتحقق من الصلاحيات المعروفة ومن ملاءمة Role الموظف أو المندوب.

## الحساب على الأجهزة

- لا يوجد نموذج Refresh Token أو تخزين جلسة/جهاز في LoginResponse.
- لا يوجد Endpoint لإلغاء توكن بعينه؛ JWT الصادر يظل صالحًا إلى انتهاء مدته ما لم تفشل مصادقة الحساب عند نقطة أخرى.
- لا يوجد `DeviceToken` أو تسجيل جهاز في نماذج المصادقة التي فُحصت.
- تغيير الأدوار/permissions لا يبدل JWT قديمًا؛ سجّل الدخول مجددًا للحصول على Claims محدثة.
- محاولات الدخول تخضع لتسجيل `LoginAuditService`؛ مدة قفل الحساب/سياسة Identity تتبع الإعدادات الفعلية.

## 401 و403

- `401` يصدر من JWT Bearer عند غياب/عدم صلاحية المصادقة؛ لا يمر هذا التحدي بالضرورة عبر `AppResponse`، لأن Middleware لا يعالج إلا الاستثناءات.
- `403` يصدر عندما يكون المستخدم موثقًا لكنه لا يحقق الدور أو policy. نفس ملاحظة شكل الجسم تنطبق على Authorization middleware.
- في `AuthService` فشل بيانات الدخول يرجع `AppResponse` بخطأ Unauthorized؛ الحساب المعطل يرجع Forbidden، ورسالة بيانات الدخول موحدة لتفادي كشف وجود المستخدم.

## إعدادات حساسة

لا تُدرج هنا JWT key أو كلمات مرور أو أسرار SMTP. إعدادات issuer/audience/key/مدة التوكن تُقرأ من إعداد `Jwt` عند التشغيل. يجب إدارة المفتاح خارج الوثائق والمستودع وعدم استخدام قيمة المفتاح الموجودة في أي تعليق قديم داخل المصدر.

