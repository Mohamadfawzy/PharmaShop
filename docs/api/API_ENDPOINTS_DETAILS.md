# API Endpoints Details

## منهج الفحص وحدوده

فُحصت كل Action فعالة ذات HTTP attribute في Controllers، ثم تبع مسار الاستدعاء إلى الـService والـRepository/DbContext والـDTO قدر ما يظهر في التنفيذ. لا يستخدم المشروع نمط CQRS موحدًا ولا توجد طبقات Command/Query/Handler لكل العمليات؛ أغلب الـControllers تستدعي Interfaces من `Contracts.IServices` ثم تنفيذًا في مجلد `Service`. لذلك لا أذكر Handler غير موجود.

| الوحدة | Controllers | مسار التنفيذ المعتاد |
|---|---|---|
| المصادقة | `AuthController`, `CurrentUserController`, Controllers الأدوار والصلاحيات | Controller → `IAuthService`/`IUserRoleService`/`IUserPermissionService` → Service → ASP.NET Identity وJWT |
| العناوين والعملاء | `AddressesController` | Controller → `ICustomerAddressService` → `CustomerAddressService` → بيانات العميل والعنوان والموقع |
| المواقع | `LocationsController`, `AdminGovernoratesController`, `AdminAreasController` | Controller → `ILocationService` → `LocationService` → Governorates/GovernorateAreas |
| الكتالوج | `PublicProductsController`, `ProductsController`, `AdminCategoriesController`, `CategoriesController` | Controller → `IProductService`/`ICategoryService` → Service → مستودع/DbContext وMapster للمنتج |
| الشركات والوحدات | `ReferenceDataController`, `AdminReferenceDataController` | Controller → `IReferenceDataService` → `ReferenceDataService` |
| الوسوم | `PublicTagsController`, `TagsController`, `AdminProductTagsController` | Controller → `ITagService` → `TagService` → Tags/ProductTags |
| العروض | `PublicPromotionsController`, `PromotionsController` | Controller → `IPromotionService` → `PromotionService` → Promotions/PromotionProducts |
| الصيدليات والمتاجر | `StoresController`, `AdminStoresController`, `AdminPharmaciesController` | Controller → `IStoreService`/`IPharmacyService` → Service → المتجر والصيدلية |
| الموظفون | `CurrentEmployeeController`, `AdminEmployeesController` | Controller → `IEmployeeService` → `EmployeeService` وIdentity عند إنشاء الحساب |
| السلة | `CartsController` | Controller → `ICartService` → `CartService` → السلة، المنتجات، العروض والنقاط |
| الوصفات | `PrescriptionsController`, `EmployeePrescriptionsController` | Controller → `IPrescriptionService` → `PrescriptionService` ومستودع الوصفات وخدمة تخزين الصور |
| الطلبات وCheckout | `OrdersController`, `AdminOrdersController` | Controller → `IOrderService` → `OrderService` → السلة/الوصفة/النقاط/العنوان والمخزون والعرض |
| المخزون | `AdminInventoryController` | Controller → `IInventoryService` → `InventoryService` → سجلات حركة المخزون والمنتج |
| النقاط | `PointsController`, `AdminPointsController` | Controller → `IPointsService` → `PointsService` ومستودع النقاط؛ توجد خدمة خلفية لانتهاء النقاط |
| الإرجاع | `CustomerOrderReturnsController`, `AdminReturnsController` | Controller → `IReturnRequestService` → `ReturnRequestService` والطلب/المخزون/النقاط |
| التوصيل | `DeliveryOrdersController`, `AdminDeliveryController`, Controllers مناطق التوصيل | Controller → `IDeliveryService`/`IDeliveryZoneService` → Service → إسناد التوصيل ومناطق المتجر |
| المدفوعات والإشعارات | `AdminPaymentsController`, `NotificationsController` | Controller → `IPaymentService`/`INotificationService` → Service وسجلات الدفع/الإشعار |
| الصور | `AdminProductAssetsController`, `DevImagesController` | Controller → `IImageService`/`IMyImageService` → معالجة ImageSharp وتخزين محلي في `wwwroot/uploads` |

## جميع العمليات الفعلية

الجداول أدناه في دليل الهاتف والإدارة تشمل لكل عملية method والمسار واسم Action والمدخلات. يستخدم [مرجع النماذج](request-models.md) لتعريف الحقول الفعلية لأنواع الطلب والـQuery. هذا الربط يمنع نسخ DTO قديم إلى توثيق جديد:

- [Endpoints غير الإدارية وعددها 72](mobile-client.md#جميع-endpoints-غير-الإدارية)
- [Endpoints الإدارية وعددها 119](admin-dashboard.md#جميع-endpoints-الإدارية)
- [قائمة مختصرة شاملة وعدد الوحدات](API_ENDPOINTS_OVERVIEW.md)
- [تعريفات DTOs وخصائص الأنواع المتداخلة](API_RESPONSE_MODELS.md)
- [حقول Request DTOs والـQuery models](request-models.md)

## قواعد تنفيذ مشتركة مثبتة في المصدر

1. `BaseApiController.FromAppResponse` يعيد `Ok(response)` عند النجاح، لذلك الـHTTP status يكون 200 حتى إن كان `AppResponse.StatusCode` داخليًا 201 أو 204. أما الفشل فيرجع `StatusCode(response.StatusCode, response)`.
2. `AuthController` يتجاوز المساعد ويرجع HTTP status الموجود في نتيجة `AuthService` مباشرة؛ التسجيل الناجح قد يرجع 201.
3. أغلب Controllers تربط المستخدم الحالي من JWT، ولا ينبغي أن يرسل تطبيق الهاتف CustomerId إذا كان الـDTO يخفيه أو يستنتجه الخادم.
4. صلاحيات الموظفين تتوزع بين دور Identity وpermission claim. تحديث الدور أو الصلاحيات لا يغير Claims الموجودة في JWT الحالي؛ يلزم تسجيل دخول جديد حتى يحصل العميل على توكن محدث.
5. مسارات الصور والملفات قد تعيد ملفًا أو DTO مباشرًا، ولا تمر كلها عبر `AppResponse`.
6. معاملات قاعدة البيانات وقواعد إعادة المحاولة تختلف حسب Service؛ لا توجد ضمانة عامة بأن كل Route يشارك في Transaction أو Idempotency.
7. Routes التي لا يوجد لها DTO/Validation في المصدر يجب أن تُعامل وفق قواعد الخدمة الفعلية، لا بناءً على جدول المثال فقط.

## شرح أهم التدفقات

### عنوان العميل

`POST /api/v1/addresses` و`PUT /api/v1/addresses/{id}` يستقبلان `CustomerAddressWriteDto`. الخدمة تتحقق من علاقة العميل الحالي بالعنوان، وتستخدم معرف المحافظة والمنطقة، وتدير العنوان الافتراضي. نموذج البيانات الحالي يحتفظ بـ`GovernorateAreaId` وبحقول النص للشارع/العلامة، ولا يعرّف مضلعًا جغرافيًا لمنطقة التوصيل. لا يرسل تطبيق الهاتف `customerId` في Body.

### البحث في المنتجات

`GET /api/v1/products` يستقبل `ProductSearchQueryParams` ويتيح البحث/الترشيح بمتجر وفئة وشركة ووحدة ووسوم ونطاق أسعار ومخزون ووصفة وعرض ومميز وقابل للتجزئة ونقاط وترتيب وصفحة. القيم المقبولة للترتيب والوسوم معرفة في `API_ENUMS.md`. مسار الإدارة `/api/v1/admin/Products/search` منفصل ويتطلب Admin.

### الصور والوصفات

رفع صورة المنتج عبر `/api/v1/admin/products/{productId}/images` يستخدم multipart و`IFormFile image`، وتخزين الصور الفعلي في خدمة الصور. رفع الوصفة عبر `/api/v1/prescriptions` يستخدم multipart و`PrescriptionCreateFormData`؛ الخدمة تتحقق من مجموعة الصور، تحفظها وتربط بياناتها بسجل الوصفة. القيود الرقمية المثبتة ومخاطر التنظيف الجزئي موضحة في `API_REQUEST_RESPONSE_EXAMPLES.md` و`API_INTEGRATION_ISSUES.md`.

### Checkout والطلبات

`POST /api/v1/orders/checkout/preview` يستدعي مسار معاينة `OrderService` ويحسب بيانات المصدر والعنوان وطريقة الدفع. `POST /api/v1/orders/checkout` ينشئ الطلب من `CheckoutRequestDto`. حقول CustomerId موسومة `JsonIgnore`، لذا مصدر هوية العميل التوكن. لا يوجد `PreviewId` أو `Idempotency-Key` في DTOs الحالية. لا تفترض أن معاينة سابقة تُقفل السعر أو المخزون؛ التأكيد يعيد المرور بقواعد الخدمة. تحقق التفاصيل والمخاطر من `OrderService` مذكورة في تقرير المشكلات، ولا تُعرض بوصفها ضمانًا غير موجود.

## مصادر الكود الأساسية

| الموضوع | الملف/الكلاس |
|---|---|
| تسجيل Routes وخيارات JSON | `WebAPI/Program.cs` |
| توحيد استجابات Controllers | `WebAPI/Controllers/BaseApiController.cs` |
| استجابة الأخطاء غير المتوقعة | `WebAPI/Middleware/ExceptionHandlingMiddleware.cs` |
| تتبع الطلب | `WebAPI/Filters/TraceIdResultFilter.cs` |
| نماذج الطلب والاستجابة | `Shared/Models/**`, `Shared/Responses/**` |
| صلاحيات JWT والسياسات | `WebAPI/Extensions/ServiceExtensions.cs`, `Service/Auth/JwtTokenService.cs`, `Service/Auth/UserPermissionService.cs` |
| قواعد الأعمال | Service المقابل لكل Controller في مجلد `Service` |


## Route-by-Route Controller Contract

الجدول التالي مولّد من HTTP attributes النشطة بعد حذف التعليقات؛ Arguments مأخوذة من توقيع Action. صيغ الـRequest DTO وحقولها منشورة في request-models.md، وخصائص DTOs في API_RESPONSE_MODELS.md.

| Feature | Method | Route | Action | Action parameters | Authorization | Source |
|---|---|---|---|---|---|---|
| Addresses | GET | `/api/v1/addresses` | `GetAll` | `CancellationToken ct` | Customer | `WebAPI/Controllers/AddressesController.cs` |
| Addresses | GET | `/api/v1/addresses/{id:int}` | `GetById` | `int id, CancellationToken ct` | Customer | `WebAPI/Controllers/AddressesController.cs` |
| Addresses | POST | `/api/v1/addresses` | `Create` | `[FromBody] CustomerAddressWriteDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/AddressesController.cs` |
| Addresses | PUT | `/api/v1/addresses/{id:int}` | `Update` | `int id, [FromBody] CustomerAddressWriteDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/AddressesController.cs` |
| Addresses | DELETE | `/api/v1/addresses/{id:int}` | `Delete` | `int id, CancellationToken ct` | Customer | `WebAPI/Controllers/AddressesController.cs` |
| Addresses | PATCH | `/api/v1/addresses/{id:int}/default` | `SetDefault` | `int id, CancellationToken ct` | Customer | `WebAPI/Controllers/AddressesController.cs` |
| Cart | POST | `/api/v1/cart/items` | `AddNormalItem` | ` [FromBody] CartAddItemDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/CartsController.cs` |
| Cart | POST | `/api/v1/cart/redeem/items` | `AddRedeemItem` | ` [FromBody] CartAddItemDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/CartsController.cs` |
| Cart | GET | `/api/v1/cart` | `GetMyCart` | `CancellationToken ct` | Customer | `WebAPI/Controllers/CartsController.cs` |
| Cart | PATCH | `/api/v1/cart/items/{cartItemId:int}` | `UpdateItemQty` | `int cartItemId, [FromBody] CartUpdateQtyDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/CartsController.cs` |
| Cart | DELETE | `/api/v1/cart/items/{cartItemId:int}` | `RemoveItem` | `int cartItemId, CancellationToken ct` | Customer | `WebAPI/Controllers/CartsController.cs` |
| Cart | DELETE | `/api/v1/cart` | `ClearCart` | `CancellationToken ct` | Customer | `WebAPI/Controllers/CartsController.cs` |
| Cart | POST | `/api/v1/cart/preview` | `Preview` | `[FromBody] CartPreviewRequestDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/CartsController.cs` |
| Categories | GET | `/api/v1/categories` | `GetAll` | `[FromQuery] RequestParameters query, CancellationToken ct` | Public | `WebAPI/Controllers/CategoriesController.cs` |
| Categories | GET | `/api/v1/categories/{categoryId:int}` | `GetById` | `int categoryId, CancellationToken ct` | Public | `WebAPI/Controllers/CategoriesController.cs` |
| Categories | GET | `/api/v1/categories/root` | `GetRoots` | `CancellationToken ct` | Public | `WebAPI/Controllers/CategoriesController.cs` |
| Categories | GET | `/api/v1/categories/tree` | `GetTree` | `CancellationToken ct` | Public | `WebAPI/Controllers/CategoriesController.cs` |
| Employees | GET | `/api/v1/me/employee` | `Get` | `CancellationToken ct` | Employee or DeliveryAgent role | `WebAPI/Controllers/CurrentEmployeeController.cs` |
| Returns | PATCH | `/api/v1/orders/{orderId:int}/cancel` | `Cancel` | `int orderId, [FromBody] CancelOrderDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/CustomerOrderReturnsController.cs` |
| Returns | POST | `/api/v1/orders/{orderId:int}/returns` | `CreateReturn` | `int orderId, [FromBody] CreateReturnRequestDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/CustomerOrderReturnsController.cs` |
| Returns | GET | `/api/v1/orders/{orderId:int}/returns/{returnRequestId:int}` | `GetReturn` | `int orderId, int returnRequestId, CancellationToken ct` | Customer | `WebAPI/Controllers/CustomerOrderReturnsController.cs` |
| Delivery | GET | `/api/v1/delivery/orders` | `GetMine` | `[FromQuery] DeliveryOrdersQueryDto query, CancellationToken ct` | DeliveryAgent + CanDeliverOrders | `WebAPI/Controllers/DeliveryOrdersController.cs` |
| Delivery | GET | `/api/v1/delivery/orders/{id:int}` | `GetById` | `int id, CancellationToken ct` | DeliveryAgent + CanDeliverOrders | `WebAPI/Controllers/DeliveryOrdersController.cs` |
| Delivery | PATCH | `/api/v1/delivery/orders/{id:int}/status` | `UpdateStatus` | `int id, [FromBody] UpdateDeliveryOrderStatusDto dto, CancellationToken ct` | DeliveryAgent + CanDeliverOrders | `WebAPI/Controllers/DeliveryOrdersController.cs` |
| Delivery zones | GET | `/api/v1/delivery-zones` | `GetByStore` | `[FromQuery] int storeId, CancellationToken ct` | Public | `WebAPI/Controllers/DeliveryZonesController.cs` |
| Images | POST | `/api/dev/images/save` | `?` | `` | Admin | `WebAPI/Controllers/DevImagesController.cs` |
| Images | PUT | `/api/dev/images/{imageId}/replace` | `?` | `` | Admin | `WebAPI/Controllers/DevImagesController.cs` |
| Images | DELETE | `/api/dev/images/{imageId}` | `Delete` | `string imageId, CancellationToken ct` | Admin | `WebAPI/Controllers/DevImagesController.cs` |
| Images | POST | `/api/dev/images/batch` | `?` | `` | Admin | `WebAPI/Controllers/DevImagesController.cs` |
| Images | GET | `/api/dev/images/{imageId}/{variant}` | `DownloadVariant` | ` string imageId, string variant, [FromQuery] string formatHint = "webp"` | Admin | `WebAPI/Controllers/DevImagesController.cs` |
| Prescriptions | GET | `/api/v1/employee/prescriptions` | `GetQueue` | `[FromQuery] AdminPrescriptionListQueryDto query, CancellationToken ct` | CanReviewPrescriptions | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| Prescriptions | GET | `/api/v1/employee/prescriptions/{id:int}` | `GetDetails` | `int id, CancellationToken ct` | CanReviewPrescriptions | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| Prescriptions | GET | `/api/v1/employee/prescriptions/{id:int}/images/{imageIdentifier}` | `GetImage` | `int id, string imageIdentifier, CancellationToken ct` | CanReviewPrescriptions | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| Prescriptions | GET | `/api/v1/employee/prescriptions/{id:int}/items` | `GetItems` | `int id, CancellationToken ct` | CanReviewPrescriptions | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| Prescriptions | POST | `/api/v1/employee/prescriptions/{id:int}/items` | `AddItem` | `int id, [FromBody] PrescriptionItemCreateDto dto, CancellationToken ct` | CanReviewPrescriptions | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| Prescriptions | POST | `/api/v1/employee/prescriptions/{id:int}/items/batch` | `AddItemsBatch` | `int id, [FromBody] PrescriptionItemsBatchCreateDto dto, CancellationToken ct` | CanReviewPrescriptions | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| Prescriptions | DELETE | `/api/v1/employee/prescriptions/{id:int}/items/{itemId:int}` | `DeleteItem` | `int id, int itemId, CancellationToken ct` | CanReviewPrescriptions | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| Prescriptions | PATCH | `/api/v1/employee/prescriptions/{id:int}/status` | `UpdateStatus` | `int id, [FromBody] PrescriptionStatusUpdateDto dto, CancellationToken ct` | CanReviewPrescriptions | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| Locations | GET | `/api/v1/locations/governorates` | `GetGovernorates` | `CancellationToken ct` | Public | `WebAPI/Controllers/LocationsController.cs` |
| Locations | GET | `/api/v1/locations/governorates/{id:int}/areas` | `GetAreas` | `int id, CancellationToken ct` | Public | `WebAPI/Controllers/LocationsController.cs` |
| Notifications | GET | `/api/v1/notifications` | `GetMine` | `[FromQuery] NotificationQueryDto query, CancellationToken ct` | Customer | `WebAPI/Controllers/NotificationsController.cs` |
| Notifications | PATCH | `/api/v1/notifications/{id:int}/read` | `MarkRead` | `int id, CancellationToken ct` | Customer | `WebAPI/Controllers/NotificationsController.cs` |
| Notifications | PATCH | `/api/v1/notifications/read-all` | `MarkAllRead` | `CancellationToken ct` | Customer | `WebAPI/Controllers/NotificationsController.cs` |
| Notifications | GET | `/api/v1/notifications/preferences` | `GetPreferences` | `CancellationToken ct` | Customer | `WebAPI/Controllers/NotificationsController.cs` |
| Notifications | PUT | `/api/v1/notifications/preferences` | `UpdatePreferences` | `[FromBody] NotificationPreferencesDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/NotificationsController.cs` |
| Orders / Checkout | POST | `/api/v1/orders/checkout` | `Checkout` | `[FromBody] CheckoutRequestDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/OrdersController.cs` |
| Orders / Checkout | POST | `/api/v1/orders/checkout/preview` | `Preview` | `[FromBody] CheckoutPreviewRequestDto dto, CancellationToken ct` | Customer | `WebAPI/Controllers/OrdersController.cs` |
| Orders / Checkout | GET | `/api/v1/orders` | `GetMine` | `[FromQuery] CustomerOrderListQueryDto query, CancellationToken ct` | Customer | `WebAPI/Controllers/OrdersController.cs` |
| Orders / Checkout | GET | `/api/v1/orders/{id:int}` | `GetMineById` | `int id, CancellationToken ct` | Customer | `WebAPI/Controllers/OrdersController.cs` |
| Points | GET | `/api/v1/points/balance` | `GetBalance` | `CancellationToken ct` | Customer | `WebAPI/Controllers/PointsController.cs` |
| Points | GET | `/api/v1/points/lots` | `GetLots` | `[FromQuery] PointsListQueryDto query, CancellationToken ct` | Customer | `WebAPI/Controllers/PointsController.cs` |
| Points | GET | `/api/v1/points/transactions` | `GetTransactions` | `[FromQuery] PointsListQueryDto query, CancellationToken ct` | Customer | `WebAPI/Controllers/PointsController.cs` |
| Points | GET | `/api/v1/points/redeemable-products` | `GetRedeemableProducts` | `[FromQuery] int? storeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default` | Customer | `WebAPI/Controllers/PointsController.cs` |
| Prescriptions | POST | `/api/v1/prescriptions` | `?` | `` | Customer | `WebAPI/Controllers/PrescriptionsController.cs` |
| Prescriptions | GET | `/api/v1/prescriptions` | `GetMine` | `[FromQuery] CustomerPrescriptionListQueryDto query, CancellationToken ct` | Customer | `WebAPI/Controllers/PrescriptionsController.cs` |
| Prescriptions | GET | `/api/v1/prescriptions/{id:int}` | `GetMineById` | `int id, CancellationToken ct` | Customer | `WebAPI/Controllers/PrescriptionsController.cs` |
| Prescriptions | GET | `/api/v1/prescriptions/{id:int}/images/{imageIdentifier}` | `GetMyImage` | `int id, string imageIdentifier, CancellationToken ct` | Customer | `WebAPI/Controllers/PrescriptionsController.cs` |
| Products | POST | `/api/v1/admin/Products` | `Create` | `[FromBody] ProductCreateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/ProductsController.cs` |
| Products | PUT | `/api/v1/admin/Products/{id:int}` | `Update` | `int id, [FromBody] ProductCreateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/ProductsController.cs` |
| Products | GET | `/api/v1/admin/Products/{id:int}` | `GetById` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/ProductsController.cs` |
| Products | GET | `/api/v1/admin/Products/deleted` | `GetDeleted` | `[FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default` | Admin (inherited controller policy) | `WebAPI/Controllers/ProductsController.cs` |
| Products | PATCH | `/api/v1/admin/Products/{id:int}/activate` | `Activate` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/ProductsController.cs` |
| Products | PATCH | `/api/v1/admin/Products/{id:int}/deactivate` | `Deactivate` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/ProductsController.cs` |
| Products | PATCH | `/api/v1/admin/Products/{id:int}/restore` | `Restore` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/ProductsController.cs` |
| Products | DELETE | `/api/v1/admin/Products/{id:int}` | `Delete` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/ProductsController.cs` |
| Products | GET | `/api/v1/admin/Products/search` | `Search` | `[FromQuery] ProductSearchQueryParams query, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/ProductsController.cs` |
| Promotions | POST | `/api/v1/admin/promotions` | `Create` | `[FromBody] PromotionCreateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/PromotionsController.cs` |
| Promotions | GET | `/api/v1/admin/promotions` | `GetAll` | `[FromQuery] PromotionListQueryDto query, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/PromotionsController.cs` |
| Promotions | GET | `/api/v1/admin/promotions/{id:int}` | `GetById` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/PromotionsController.cs` |
| Promotions | PUT | `/api/v1/admin/promotions/{id:int}` | `Update` | `int id, [FromBody] PromotionUpdateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/PromotionsController.cs` |
| Promotions | PATCH | `/api/v1/admin/promotions/{id:int}/activate` | `Activate` | `int id, [FromBody] PromotionActivateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/PromotionsController.cs` |
| Promotions | PATCH | `/api/v1/admin/promotions/{id:int}/deactivate` | `Deactivate` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/PromotionsController.cs` |
| Promotions | POST | `/api/v1/admin/promotions/{id:int}/products` | `AddProducts` | `int id, [FromBody] PromotionAddProductsDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/PromotionsController.cs` |
| Promotions | DELETE | `/api/v1/admin/promotions/{id:int}/products/{promotionProductId:int}` | `RemoveProduct` | `int id, int promotionProductId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/PromotionsController.cs` |
| Promotions | PUT | `/api/v1/admin/promotions/{id:int}/products` | `ReplaceProducts` | `int id, [FromBody] PromotionReplaceProductsDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/PromotionsController.cs` |
| Products | GET | `/api/v1/products` | `Search` | `[FromQuery] ProductSearchQueryParams query, CancellationToken ct` | Public | `WebAPI/Controllers/PublicProductsController.cs` |
| Products | GET | `/api/v1/products/{id:int}` | `Get` | `int id, CancellationToken ct` | Public | `WebAPI/Controllers/PublicProductsController.cs` |
| Promotions | GET | `/api/v1/promotions/active` | `GetActive` | `[FromQuery] PromotionListQueryDto query, CancellationToken ct` | Public | `WebAPI/Controllers/PublicPromotionsController.cs` |
| Promotions | GET | `/api/v1/promotions/{id:int}` | `GetActiveById` | `int id, CancellationToken ct` | Public | `WebAPI/Controllers/PublicPromotionsController.cs` |
| Tags | GET | `/api/v1/tags` | `List` | `[FromQuery] TagQueryDto query, CancellationToken ct` | Public | `WebAPI/Controllers/PublicTagsController.cs` |
| Companies / Units | GET | `/api/v1/companies` | `Companies` | `CancellationToken ct` | Public | `WebAPI/Controllers/ReferenceDataController.cs` |
| Companies / Units | GET | `/api/v1/units` | `Units` | `CancellationToken ct` | Public | `WebAPI/Controllers/ReferenceDataController.cs` |
| Stores / Pharmacies | GET | `/api/v1/stores` | `GetAll` | `[FromQuery] int? pharmacyId, CancellationToken ct` | Public | `WebAPI/Controllers/StoresController.cs` |
| Stores / Pharmacies | GET | `/api/v1/stores/{id:int}` | `GetById` | `int id, CancellationToken ct` | Public | `WebAPI/Controllers/StoresController.cs` |
| Tags | POST | `/api/v1/admin/Tags` | `Create` | `[FromBody] TagCreateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/TagsController.cs` |
| Tags | GET | `/api/v1/admin/Tags` | `GetAll` | `[FromQuery] TagQueryDto query, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/TagsController.cs` |
| Tags | GET | `/api/v1/admin/Tags/{id:int}` | `GetById` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/TagsController.cs` |
| Tags | PUT | `/api/v1/admin/Tags/{id:int}` | `Update` | `int id, [FromBody] TagCreateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/TagsController.cs` |
| Tags | DELETE | `/api/v1/admin/Tags/{id:int}` | `Delete` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/TagsController.cs` |
| Tags | PATCH | `/api/v1/admin/Tags/{id:int}/activate` | `Activate` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/TagsController.cs` |
| Locations | GET | `/api/v1/admin/areas` | `GetAll` | `[FromQuery] int? governorateId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| Locations | GET | `/api/v1/admin/areas/{id:int}` | `GetById` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| Locations | POST | `/api/v1/admin/areas` | `Create` | `[FromBody] GovernorateAreaWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| Locations | PUT | `/api/v1/admin/areas/{id:int}` | `Update` | `int id, [FromBody] GovernorateAreaWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| Locations | PATCH | `/api/v1/admin/areas/{id:int}/active` | `SetActive` | `int id, [FromBody] LocationActiveDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| Categories | GET | `/api/v1/admin/categories` | `GetAll` | `[FromQuery] RequestParameters param, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Categories | GET | `/api/v1/admin/categories/{categoryId:int}` | `GetById` | `int categoryId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Categories | POST | `/api/v1/admin/categories` | `Create` | `[FromBody] CategoryCreateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Categories | PUT | `/api/v1/admin/categories/{categoryId:int}` | `Update` | ` int categoryId, [FromBody] CategoryUpdateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Categories | PUT | `/api/v1/admin/categories/{categoryId:int}/parent` | `ChangeParent` | ` int categoryId, [FromQuery] int? newParentCategoryId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Categories | PUT | `/api/v1/admin/categories/{categoryId:int}/image` | `?` | `` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Categories | PATCH | `/api/v1/admin/categories/{categoryId:int}/activate` | `Activate` | `int categoryId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Categories | PATCH | `/api/v1/admin/categories/{categoryId:int}/deactivate` | `Deactivate` | `int categoryId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Categories | DELETE | `/api/v1/admin/categories/{categoryId:int}` | `Delete` | `int categoryId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Categories | PATCH | `/api/v1/admin/categories/{categoryId:int}/restore` | `Restore` | `int categoryId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| Delivery | GET | `/api/v1/admin/delivery-agents` | `GetAgents` | `[FromQuery] DeliveryAgentQueryDto query, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| Delivery | GET | `/api/v1/admin/orders/{orderId:int}/delivery-agent` | `GetAssignment` | `int orderId, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| Delivery | PUT | `/api/v1/admin/orders/{orderId:int}/delivery-agent` | `Assign` | `int orderId, [FromBody] AssignDeliveryAgentDto dto, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| Delivery | DELETE | `/api/v1/admin/orders/{orderId:int}/delivery-agent` | `Unassign` | `int orderId, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| Delivery | GET | `/api/v1/admin/orders/{orderId:int}/status-history` | `GetStatusHistory` | `int orderId, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| Delivery zones | GET | `/api/v1/admin/stores/{storeId:int}/delivery-zones` | `GetAll` | `int storeId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| Delivery zones | GET | `/api/v1/admin/stores/{storeId:int}/delivery-zones/{id:int}` | `GetById` | `int storeId, int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| Delivery zones | POST | `/api/v1/admin/stores/{storeId:int}/delivery-zones` | `Create` | `int storeId, [FromBody] DeliveryZoneWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| Delivery zones | PUT | `/api/v1/admin/stores/{storeId:int}/delivery-zones/{id:int}` | `Update` | `int storeId, int id, [FromBody] DeliveryZoneWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| Delivery zones | PATCH | `/api/v1/admin/stores/{storeId:int}/delivery-zones/{id:int}/active` | `SetActive` | `int storeId, int id, [FromBody] DeliveryZoneActiveDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| Employees | GET | `/api/v1/admin/employees` | `GetAll` | `[FromQuery] int? pharmacyId, [FromQuery] bool includeInactive = false, CancellationToken ct = default` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| Employees | GET | `/api/v1/admin/employees/deleted` | `GetDeleted` | `[FromQuery] int? pharmacyId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| Employees | GET | `/api/v1/admin/employees/{id:int}` | `GetById` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| Employees | POST | `/api/v1/admin/employees` | `Create` | `[FromBody] EmployeeCreateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| Employees | PUT | `/api/v1/admin/employees/{id:int}` | `Update` | `int id, [FromBody] EmployeeUpdateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| Employees | PATCH | `/api/v1/admin/employees/{id:int}/active` | `SetActive` | `int id, [FromBody] EmployeeActiveDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| Employees | DELETE | `/api/v1/admin/employees/{id:int}` | `Delete` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| Employees | POST | `/api/v1/admin/employees/{id:int}/restore` | `Restore` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| Locations | GET | `/api/v1/admin/governorates` | `GetAll` | `CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| Locations | GET | `/api/v1/admin/governorates/{id:int}` | `GetById` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| Locations | POST | `/api/v1/admin/governorates` | `Create` | `[FromBody] GovernorateWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| Locations | PUT | `/api/v1/admin/governorates/{id:int}` | `Update` | `int id, [FromBody] GovernorateWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| Locations | PATCH | `/api/v1/admin/governorates/{id:int}/active` | `SetActive` | `int id, [FromBody] LocationActiveDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| Inventory | POST | `/api/v1/admin/inventory/products/{productId:int}/receipts` | `Receive` | `int productId, [FromBody] ReceiveStockDto dto, CancellationToken ct` | CanManageInventory | `WebAPI/Controllers/Admin/AdminInventoryController.cs` |
| Inventory | POST | `/api/v1/admin/inventory/adjustments` | `Adjust` | `[FromBody] AdjustInventoryDto dto, CancellationToken ct` | CanManageInventory | `WebAPI/Controllers/Admin/AdminInventoryController.cs` |
| Inventory | GET | `/api/v1/admin/inventory/transactions` | `Search` | `[FromQuery] InventoryTransactionQueryDto query, CancellationToken ct` | CanManageInventory | `WebAPI/Controllers/Admin/AdminInventoryController.cs` |
| Orders / Checkout | GET | `/api/v1/admin/orders` | `GetAll` | `[FromQuery] AdminOrderListQueryDto query, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminOrdersController.cs` |
| Orders / Checkout | GET | `/api/v1/admin/orders/{id:int}` | `GetById` | `int id, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminOrdersController.cs` |
| Orders / Checkout | PATCH | `/api/v1/admin/orders/{id:int}/status` | `UpdateStatus` | `int id, [FromBody] OrderStatusUpdateDto dto, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminOrdersController.cs` |
| Payments | GET | `/api/v1/admin/orders/{orderId:int}/payments` | `GetOrderPayments` | `int orderId, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| Payments | POST | `/api/v1/admin/orders/{orderId:int}/payment-attempts` | `CreateAttempt` | `int orderId, [FromBody] CreatePaymentAttemptDto dto, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| Payments | PATCH | `/api/v1/admin/payment-attempts/{attemptId:int}` | `UpdateAttempt` | `int attemptId, [FromBody] UpdatePaymentAttemptDto dto, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| Payments | POST | `/api/v1/admin/orders/{orderId:int}/refunds` | `CreateRefund` | `int orderId, [FromBody] CreatePaymentRefundDto dto, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| Payments | PATCH | `/api/v1/admin/payment-refunds/{refundId:int}` | `UpdateRefund` | `int refundId, [FromBody] UpdatePaymentRefundDto dto, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| Stores / Pharmacies | GET | `/api/v1/admin/pharmacies` | `GetAll` | `CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminPharmaciesController.cs` |
| Stores / Pharmacies | GET | `/api/v1/admin/pharmacies/{id:int}` | `GetById` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminPharmaciesController.cs` |
| Stores / Pharmacies | POST | `/api/v1/admin/pharmacies` | `Create` | `[FromBody] PharmacyCreateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminPharmaciesController.cs` |
| Stores / Pharmacies | PUT | `/api/v1/admin/pharmacies/{id:int}` | `Update` | `int id, [FromBody] PharmacyUpdateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminPharmaciesController.cs` |
| Points | POST | `/api/v1/admin/points/customers/{customerId:int}/adjust` | `Adjust` | `int customerId, [FromBody] ManualPointsAdjustmentDto dto, CancellationToken ct` | CanManagePoints | `WebAPI/Controllers/Admin/AdminPointsController.cs` |
| Images | GET | `/api/v1/admin/products/{productId:int}/images` | `List` | `int productId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| Images | POST | `/api/v1/admin/products/{productId:int}/images` | `?` | `` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| Images | PUT | `/api/v1/admin/products/{productId:int}/images/{imageId:long}` | `Update` | `int productId, long imageId, [FromBody] ProductImageUpdateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| Images | PUT | `/api/v1/admin/products/{productId:int}/images/{imageId:long}/file` | `?` | `` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| Images | DELETE | `/api/v1/admin/products/{productId:int}/images/{imageId:long}` | `Delete` | `int productId, long imageId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| Product tags | GET | `/api/v1/admin/products/{productId:int}/tags` | `List` | `int productId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminProductTagsController.cs` |
| Product tags | PUT | `/api/v1/admin/products/{productId:int}/tags` | `Replace` | `int productId, ProductTagsUpdateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminProductTagsController.cs` |
| Companies / Units | GET | `/api/v1/admin/companies` | `Companies` | `[FromQuery] bool includeInactive = false, CancellationToken ct = default` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | GET | `/api/v1/admin/companies/{id:int}` | `Company` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | POST | `/api/v1/admin/companies` | `CreateCompany` | `CompanyWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | PUT | `/api/v1/admin/companies/{id:int}` | `UpdateCompany` | `int id, CompanyWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | DELETE | `/api/v1/admin/companies/{id:int}` | `DeleteCompany` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | PATCH | `/api/v1/admin/companies/{id:int}/restore` | `RestoreCompany` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | GET | `/api/v1/admin/units` | `Units` | `[FromQuery] bool includeInactive = false, CancellationToken ct = default` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | GET | `/api/v1/admin/units/{id:int}` | `Unit` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | POST | `/api/v1/admin/units` | `CreateUnit` | `UnitWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | PUT | `/api/v1/admin/units/{id:int}` | `UpdateUnit` | `int id, UnitWriteDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | DELETE | `/api/v1/admin/units/{id:int}` | `DeleteUnit` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Companies / Units | PATCH | `/api/v1/admin/units/{id:int}/activate` | `ActivateUnit` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| Returns | GET | `/api/v1/admin/returns` | `Search` | `[FromQuery] ReturnRequestQueryDto query, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminReturnsController.cs` |
| Returns | GET | `/api/v1/admin/returns/{id:int}` | `GetById` | `int id, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminReturnsController.cs` |
| Returns | PATCH | `/api/v1/admin/returns/{id:int}/decision` | `Decide` | `int id, [FromBody] ReviewReturnRequestDto dto, CancellationToken ct` | CanManageOrders | `WebAPI/Controllers/Admin/AdminReturnsController.cs` |
| Stores / Pharmacies | GET | `/api/v1/admin/stores` | `GetAll` | `[FromQuery] int? pharmacyId, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| Stores / Pharmacies | GET | `/api/v1/admin/stores/{id:int}` | `GetById` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| Stores / Pharmacies | POST | `/api/v1/admin/stores` | `Create` | `[FromBody] StoreCreateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| Stores / Pharmacies | PUT | `/api/v1/admin/stores/{id:int}` | `Update` | `int id, [FromBody] StoreUpdateDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| Stores / Pharmacies | PATCH | `/api/v1/admin/stores/{id:int}/active` | `SetActive` | `int id, [FromBody] StoreActiveDto dto, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| Stores / Pharmacies | PATCH | `/api/v1/admin/stores/{id:int}/default` | `SetDefault` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| Stores / Pharmacies | DELETE | `/api/v1/admin/stores/{id:int}` | `Delete` | `int id, CancellationToken ct` | Admin (inherited controller policy) | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| Authentication / Access | GET | `/api/admin/roles` | `GetAllRoles` | `CancellationToken ct` | Admin | `WebAPI/Controllers/Auth/AdminRolesController.cs` |
| Authentication / Access | GET | `/api/admin/users/{userId:int}/permissions` | `Get` | `int userId, CancellationToken ct` | Admin | `WebAPI/Controllers/Auth/AdminUsersPermissionsController.cs` |
| Authentication / Access | PUT | `/api/admin/users/{userId:int}/permissions` | `Set` | ` int userId, [FromBody] SetUserPermissionsRequestDto dto, CancellationToken ct` | Admin | `WebAPI/Controllers/Auth/AdminUsersPermissionsController.cs` |
| Authentication / Access | GET | `/api/admin/users/{userId:int}/roles` | `GetUserRoles` | `int userId, CancellationToken ct` | Admin | `WebAPI/Controllers/Auth/AdminUsersRolesController.cs` |
| Authentication / Access | PUT | `/api/admin/users/{userId:int}/roles` | `SetRoles` | `int userId, [FromBody] SetUserRolesRequestDto dto, CancellationToken ct` | Admin | `WebAPI/Controllers/Auth/AdminUsersRolesController.cs` |
| Authentication / Access | POST | `/api/admin/users/{userId:int}/roles` | `AddRoles` | `int userId, [FromBody] ModifyUserRolesRequestDto dto, CancellationToken ct` | Admin | `WebAPI/Controllers/Auth/AdminUsersRolesController.cs` |
| Authentication / Access | DELETE | `/api/admin/users/{userId:int}/roles` | `RemoveRoles` | `int userId, [FromBody] ModifyUserRolesRequestDto dto, CancellationToken ct` | Admin | `WebAPI/Controllers/Auth/AdminUsersRolesController.cs` |
| Authentication / Access | POST | `/api/auth/register` | `Register` | `[FromBody] RegisterRequestDto dto, CancellationToken ct` | Public | `WebAPI/Controllers/Auth/AuthController.cs` |
| Authentication / Access | POST | `/api/auth/confirm-email` | `ConfirmEmail` | `[FromBody] ConfirmEmailRequestDto dto, CancellationToken ct` | Public | `WebAPI/Controllers/Auth/AuthController.cs` |
| Authentication / Access | GET | `/api/auth/confirm-email` | `ConfirmEmailGet` | `[FromQuery] int userId,[FromQuery] string token,CancellationToken ct` | Public | `WebAPI/Controllers/Auth/AuthController.cs` |
| Authentication / Access | POST | `/api/auth/login` | `Login` | `[FromBody] LoginRequestDto dto, CancellationToken ct` | Public | `WebAPI/Controllers/Auth/AuthController.cs` |
| Authentication / Access | POST | `/api/auth/resend-confirm-email` | `ResendConfirmEmail` | `[FromBody] ResendConfirmEmailRequestDto dto, CancellationToken ct` | Public | `WebAPI/Controllers/Auth/AuthController.cs` |
| Authentication / Access | POST | `/api/auth/forgot-password` | `ForgotPassword` | `[FromBody] ForgotPasswordRequestDto dto, CancellationToken ct` | Public | `WebAPI/Controllers/Auth/AuthController.cs` |
| Authentication / Access | POST | `/api/auth/reset-password` | `ResetPassword` | `[FromBody] ResetPasswordRequestDto dto, CancellationToken ct` | Public | `WebAPI/Controllers/Auth/AuthController.cs` |
| Authentication / Access | POST | `/api/auth/change-password` | `ChangePassword` | `[FromBody] ChangePasswordRequestDto dto, CancellationToken ct` | Authenticated | `WebAPI/Controllers/Auth/AuthController.cs` |
| Users / Profile | GET | `/api/v1/me` | `Get` | `CancellationToken ct` | Authenticated; Update=Customer | `WebAPI/Controllers/Auth/CurrentUserController.cs` |
| Users / Profile | GET | `/api/v1/me/permissions` | `GetPermissions` | `` | Authenticated; Update=Customer | `WebAPI/Controllers/Auth/CurrentUserController.cs` |
| Users / Profile | PUT | `/api/v1/me` | `?` | `` | Authenticated; Update=Customer | `WebAPI/Controllers/Auth/CurrentUserController.cs` |

## Response contract map for every endpoint

Response types below are resolved from the concrete Service interface when the Controller delegates through AppResponse. For fields and nested DTO schemas, use API_RESPONSE_MODELS.md. Direct file/controller responses are marked separately. HTTP success behavior reflects BaseApiController, not only the body statusCode.

| Route | Action | Response type | HTTP success behavior | Return path | Controller source |
|---|---|---|---|---|---|
| `/api/v1/addresses` | `GET GetAll` | `AppResponse<List<CustomerAddressDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyAddressesAsync | `WebAPI/Controllers/AddressesController.cs` |
| `/api/v1/addresses/{id:int}` | `GET GetById` | `AppResponse<CustomerAddressDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyAddressAsync | `WebAPI/Controllers/AddressesController.cs` |
| `/api/v1/addresses` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateAsync | `WebAPI/Controllers/AddressesController.cs` |
| `/api/v1/addresses/{id:int}` | `PUT Update` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateAsync | `WebAPI/Controllers/AddressesController.cs` |
| `/api/v1/addresses/{id:int}` | `DELETE Delete` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SoftDeleteAsync | `WebAPI/Controllers/AddressesController.cs` |
| `/api/v1/addresses/{id:int}/default` | `PATCH SetDefault` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetDefaultAsync | `WebAPI/Controllers/AddressesController.cs` |
| `/api/v1/cart/items` | `POST AddNormalItem` | `AppResponse<AddToCartResponseDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/CartsController.cs` |
| `/api/v1/cart/redeem/items` | `POST AddRedeemItem` | `AppResponse<AddToCartResponseDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/CartsController.cs` |
| `/api/v1/cart` | `GET GetMyCart` | `AppResponse<CartViewDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/CartsController.cs` |
| `/api/v1/cart/items/{cartItemId:int}` | `PATCH UpdateItemQty` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/CartsController.cs` |
| `/api/v1/cart/items/{cartItemId:int}` | `DELETE RemoveItem` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/CartsController.cs` |
| `/api/v1/cart` | `DELETE ClearCart` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/CartsController.cs` |
| `/api/v1/cart/preview` | `POST Preview` | `AppResponse<CartPreviewResponseDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/CartsController.cs` |
| `/api/v1/categories` | `GET GetAll` | `AppResponse<List<CategoryDetailsDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAllCategoriesAsync | `WebAPI/Controllers/CategoriesController.cs` |
| `/api/v1/categories/{categoryId:int}` | `GET GetById` | `AppResponse<CategoryDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetCategoryByIdAsync | `WebAPI/Controllers/CategoriesController.cs` |
| `/api/v1/categories/root` | `GET GetRoots` | `AppResponse<List<CategoryDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetRootCategoriesAsync | `WebAPI/Controllers/CategoriesController.cs` |
| `/api/v1/categories/tree` | `GET GetTree` | `AppResponse<List<CategoryTreeDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetCategoryTreeAsync | `WebAPI/Controllers/CategoriesController.cs` |
| `/api/v1/me/employee` | `GET Get` | `AppResponse<EmployeeDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetCurrentAsync | `WebAPI/Controllers/CurrentEmployeeController.cs` |
| `/api/v1/orders/{orderId:int}/cancel` | `PATCH Cancel` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CancelMyOrderAsync | `WebAPI/Controllers/CustomerOrderReturnsController.cs` |
| `/api/v1/orders/{orderId:int}/returns` | `POST CreateReturn` | `AppResponse<ReturnRequestDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateMyReturnAsync | `WebAPI/Controllers/CustomerOrderReturnsController.cs` |
| `/api/v1/orders/{orderId:int}/returns/{returnRequestId:int}` | `GET GetReturn` | `AppResponse<ReturnRequestDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyReturnAsync | `WebAPI/Controllers/CustomerOrderReturnsController.cs` |
| `/api/v1/delivery/orders` | `GET GetMine` | `AppResponse<List<CustomerOrderListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyOrdersAsync | `WebAPI/Controllers/DeliveryOrdersController.cs` |
| `/api/v1/delivery/orders/{id:int}` | `GET GetById` | `AppResponse<CustomerOrderDetailsDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyOrderAsync | `WebAPI/Controllers/DeliveryOrdersController.cs` |
| `/api/v1/delivery/orders/{id:int}/status` | `PATCH UpdateStatus` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateMyOrderStatusAsync | `WebAPI/Controllers/DeliveryOrdersController.cs` |
| `/api/v1/delivery-zones` | `GET GetByStore` | `AppResponse<List<StoreDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetPublicAsync | `WebAPI/Controllers/DeliveryZonesController.cs` |
| `/api/dev/images/save` | `POST Save` | `SavedImageResult` | Direct result status | Direct ActionResult | `WebAPI/Controllers/DevImagesController.cs` |
| `/api/dev/images/{imageId}/replace` | `PUT Replace` | `SavedImageResult` | Direct result status | Direct ActionResult | `WebAPI/Controllers/DevImagesController.cs` |
| `/api/dev/images/{imageId}` | `DELETE Delete` | `No response body` | Direct result status | Direct Controller result | `WebAPI/Controllers/DevImagesController.cs` |
| `/api/dev/images/batch` | `POST Batch` | `IReadOnlyList<SavedImageResult` | Direct result status | Direct ActionResult | `WebAPI/Controllers/DevImagesController.cs` |
| `/api/dev/images/{imageId}/{variant}` | `GET DownloadVariant` | `File/binary response` | Direct result status | Direct Controller result | `WebAPI/Controllers/DevImagesController.cs` |
| `/api/v1/employee/prescriptions` | `GET GetQueue` | `AppResponse<List<AdminPrescriptionListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetEmployeePrescriptionsAsync | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| `/api/v1/employee/prescriptions/{id:int}` | `GET GetDetails` | `AppResponse<CustomerPrescriptionDetailsDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetEmployeePrescriptionAsync | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| `/api/v1/employee/prescriptions/{id:int}/images/{imageIdentifier}` | `GET GetImage` | `AppResponse<string>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetPrescriptionImageIdentifierAsync | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| `/api/v1/employee/prescriptions/{id:int}/items` | `GET GetItems` | `AppResponse<List<PrescriptionItemListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminPrescriptionItemsAsync | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| `/api/v1/employee/prescriptions/{id:int}/items` | `POST AddItem` | `AppResponse<PrescriptionItemCreatedDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method AddPrescriptionItemAsync | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| `/api/v1/employee/prescriptions/{id:int}/items/batch` | `POST AddItemsBatch` | `AppResponse<PrescriptionItemsBatchCreateResultDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method AddPrescriptionItemsBatchAsync | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| `/api/v1/employee/prescriptions/{id:int}/items/{itemId:int}` | `DELETE DeleteItem` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method DeletePrescriptionItemAsync | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| `/api/v1/employee/prescriptions/{id:int}/status` | `PATCH UpdateStatus` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdatePrescriptionStatusAsync | `WebAPI/Controllers/EmployeePrescriptionsController.cs` |
| `/api/v1/locations/governorates` | `GET GetGovernorates` | `AppResponse<List<GovernorateDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetGovernoratesAsync | `WebAPI/Controllers/LocationsController.cs` |
| `/api/v1/locations/governorates/{id:int}/areas` | `GET GetAreas` | `AppResponse<List<GovernorateAreaDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetPublicAreasAsync | `WebAPI/Controllers/LocationsController.cs` |
| `/api/v1/notifications` | `GET GetMine` | `AppResponse<List<NotificationDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMineAsync | `WebAPI/Controllers/NotificationsController.cs` |
| `/api/v1/notifications/{id:int}/read` | `PATCH MarkRead` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method MarkReadAsync | `WebAPI/Controllers/NotificationsController.cs` |
| `/api/v1/notifications/read-all` | `PATCH MarkAllRead` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method MarkAllReadAsync | `WebAPI/Controllers/NotificationsController.cs` |
| `/api/v1/notifications/preferences` | `GET GetPreferences` | `AppResponse<NotificationPreferencesDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetPreferencesAsync | `WebAPI/Controllers/NotificationsController.cs` |
| `/api/v1/notifications/preferences` | `PUT UpdatePreferences` | `AppResponse<NotificationPreferencesDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdatePreferencesAsync | `WebAPI/Controllers/NotificationsController.cs` |
| `/api/v1/orders/checkout` | `POST Checkout` | `AppResponse<CheckoutResultDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CheckoutAsync | `WebAPI/Controllers/OrdersController.cs` |
| `/api/v1/orders/checkout/preview` | `POST Preview` | `AppResponse<CheckoutPreviewResponseDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method PreviewAsync | `WebAPI/Controllers/OrdersController.cs` |
| `/api/v1/orders` | `GET GetMine` | `AppResponse<List<CustomerOrderListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyOrdersAsync | `WebAPI/Controllers/OrdersController.cs` |
| `/api/v1/orders/{id:int}` | `GET GetMineById` | `AppResponse<CustomerOrderDetailsDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyOrderAsync | `WebAPI/Controllers/OrdersController.cs` |
| `/api/v1/points/balance` | `GET GetBalance` | `AppResponse<PointsBalanceDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyBalanceAsync | `WebAPI/Controllers/PointsController.cs` |
| `/api/v1/points/lots` | `GET GetLots` | `AppResponse<List<CustomerPointLotDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyLotsAsync | `WebAPI/Controllers/PointsController.cs` |
| `/api/v1/points/transactions` | `GET GetTransactions` | `AppResponse<List<CustomerPointTransactionDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyTransactionsAsync | `WebAPI/Controllers/PointsController.cs` |
| `/api/v1/points/redeemable-products` | `GET GetRedeemableProducts` | `AppResponse<List<RedeemablePointsProductDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetRedeemableProductsAsync | `WebAPI/Controllers/PointsController.cs` |
| `/api/v1/prescriptions` | `POST Create` | `AppResponse<PrescriptionCreateResultDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateWithImagesAsync | `WebAPI/Controllers/PrescriptionsController.cs` |
| `/api/v1/prescriptions` | `GET GetMine` | `AppResponse<List<CustomerPrescriptionListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyPrescriptionsAsync | `WebAPI/Controllers/PrescriptionsController.cs` |
| `/api/v1/prescriptions/{id:int}` | `GET GetMineById` | `AppResponse<CustomerPrescriptionDetailsDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetMyPrescriptionAsync | `WebAPI/Controllers/PrescriptionsController.cs` |
| `/api/v1/prescriptions/{id:int}/images/{imageIdentifier}` | `GET GetMyImage` | `AppResponse<string>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetPrescriptionImageIdentifierAsync | `WebAPI/Controllers/PrescriptionsController.cs` |
| `/api/v1/admin/Products` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateProductAsync | `WebAPI/Controllers/ProductsController.cs` |
| `/api/v1/admin/Products/{id:int}` | `PUT Update` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateProductAsync | `WebAPI/Controllers/ProductsController.cs` |
| `/api/v1/admin/Products/{id:int}` | `GET GetById` | `AppResponse<ProductDetailsDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetProductAsync | `WebAPI/Controllers/ProductsController.cs` |
| `/api/v1/admin/Products/deleted` | `GET GetDeleted` | `AppResponse<List<ProductListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetDeletedProductsAsync | `WebAPI/Controllers/ProductsController.cs` |
| `/api/v1/admin/Products/{id:int}/activate` | `PATCH Activate` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetProductStateAsync | `WebAPI/Controllers/ProductsController.cs` |
| `/api/v1/admin/Products/{id:int}/deactivate` | `PATCH Deactivate` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetProductStateAsync | `WebAPI/Controllers/ProductsController.cs` |
| `/api/v1/admin/Products/{id:int}/restore` | `PATCH Restore` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetProductStateAsync | `WebAPI/Controllers/ProductsController.cs` |
| `/api/v1/admin/Products/{id:int}` | `DELETE Delete` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetProductStateAsync | `WebAPI/Controllers/ProductsController.cs` |
| `/api/v1/admin/Products/search` | `GET Search` | `AppResponse<List<ProductListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SearchProductsAsync | `WebAPI/Controllers/ProductsController.cs` |
| `/api/v1/admin/promotions` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/PromotionsController.cs` |
| `/api/v1/admin/promotions` | `GET GetAll` | `AppResponse<List<PromotionListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/PromotionsController.cs` |
| `/api/v1/admin/promotions/{id:int}` | `GET GetById` | `AppResponse<PromotionDetailsDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/PromotionsController.cs` |
| `/api/v1/admin/promotions/{id:int}` | `PUT Update` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/PromotionsController.cs` |
| `/api/v1/admin/promotions/{id:int}/activate` | `PATCH Activate` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/PromotionsController.cs` |
| `/api/v1/admin/promotions/{id:int}/deactivate` | `PATCH Deactivate` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/PromotionsController.cs` |
| `/api/v1/admin/promotions/{id:int}/products` | `POST AddProducts` | `AppResponse<PromotionAddProductsResultDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/PromotionsController.cs` |
| `/api/v1/admin/promotions/{id:int}/products/{promotionProductId:int}` | `DELETE RemoveProduct` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/PromotionsController.cs` |
| `/api/v1/admin/promotions/{id:int}/products` | `PUT ReplaceProducts` | `AppResponse<PromotionReplaceProductsResultDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/PromotionsController.cs` |
| `/api/v1/products` | `GET Search` | `AppResponse<List<ProductListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SearchProductsAsync | `WebAPI/Controllers/PublicProductsController.cs` |
| `/api/v1/products/{id:int}` | `GET Get` | `AppResponse<ProductDetailsDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetProductAsync | `WebAPI/Controllers/PublicProductsController.cs` |
| `/api/v1/promotions/active` | `GET GetActive` | `AppResponse<List<PublicPromotionListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetActivePromotionsAsync | `WebAPI/Controllers/PublicPromotionsController.cs` |
| `/api/v1/promotions/{id:int}` | `GET GetActiveById` | `AppResponse<PublicPromotionDetailsDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetActivePromotionByIdAsync | `WebAPI/Controllers/PublicPromotionsController.cs` |
| `/api/v1/tags` | `GET List` | `AppResponse<List<TagListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetTagsAsync | `WebAPI/Controllers/PublicTagsController.cs` |
| `/api/v1/companies` | `GET Companies` | `AppResponse<List<CompanyDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetCompaniesAsync | `WebAPI/Controllers/ReferenceDataController.cs` |
| `/api/v1/units` | `GET Units` | `AppResponse<List<UnitDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetUnitsAsync | `WebAPI/Controllers/ReferenceDataController.cs` |
| `/api/v1/stores` | `GET GetAll` | `AppResponse<List<StoreDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetPublicAsync | `WebAPI/Controllers/StoresController.cs` |
| `/api/v1/stores/{id:int}` | `GET GetById` | `AppResponse<StoreDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetPublicByIdAsync | `WebAPI/Controllers/StoresController.cs` |
| `/api/v1/admin/Tags` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateTagAsync | `WebAPI/Controllers/TagsController.cs` |
| `/api/v1/admin/Tags` | `GET GetAll` | `AppResponse<List<TagListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetTagsAsync | `WebAPI/Controllers/TagsController.cs` |
| `/api/v1/admin/Tags/{id:int}` | `GET GetById` | `AppResponse<TagListItemDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetTagAsync | `WebAPI/Controllers/TagsController.cs` |
| `/api/v1/admin/Tags/{id:int}` | `PUT Update` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateTagAsync | `WebAPI/Controllers/TagsController.cs` |
| `/api/v1/admin/Tags/{id:int}` | `DELETE Delete` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetTagActiveAsync | `WebAPI/Controllers/TagsController.cs` |
| `/api/v1/admin/Tags/{id:int}/activate` | `PATCH Activate` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetTagActiveAsync | `WebAPI/Controllers/TagsController.cs` |
| `/api/v1/admin/areas` | `GET GetAll` | `AppResponse<List<GovernorateAreaDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminAreasAsync | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| `/api/v1/admin/areas/{id:int}` | `GET GetById` | `AppResponse<GovernorateAreaDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAreaByIdAsync | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| `/api/v1/admin/areas` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateAreaAsync | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| `/api/v1/admin/areas/{id:int}` | `PUT Update` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateAreaAsync | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| `/api/v1/admin/areas/{id:int}/active` | `PATCH SetActive` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetAreaActiveAsync | `WebAPI/Controllers/Admin/AdminAreasController.cs` |
| `/api/v1/admin/categories` | `GET GetAll` | `AppResponse<List<CategoryDetailsDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAllCategoriesAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/categories/{categoryId:int}` | `GET GetById` | `AppResponse<CategoryDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetCategoryByIdAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/categories` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateCategoryAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/categories/{categoryId:int}` | `PUT Update` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/categories/{categoryId:int}/parent` | `PUT ChangeParent` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method ChangeCategoryParentAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/categories/{categoryId:int}/image` | `PUT UpdateImage` | `AppResponse<string>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateCategoryImageAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/categories/{categoryId:int}/activate` | `PATCH Activate` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetCategoryActiveStatusAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/categories/{categoryId:int}/deactivate` | `PATCH Deactivate` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetCategoryActiveStatusAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/categories/{categoryId:int}` | `DELETE Delete` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetCategoryDeletedAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/categories/{categoryId:int}/restore` | `PATCH Restore` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetCategoryDeletedAsync | `WebAPI/Controllers/Admin/AdminCategoriesController.cs` |
| `/api/v1/admin/delivery-agents` | `GET GetAgents` | `AppResponse<List<DeliveryAgentDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAgentsAsync | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| `/api/v1/admin/orders/{orderId:int}/delivery-agent` | `GET GetAssignment` | `AppResponse<DeliveryAgentAssignmentDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAssignmentAsync | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| `/api/v1/admin/orders/{orderId:int}/delivery-agent` | `PUT Assign` | `AppResponse<DeliveryAgentAssignmentDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method AssignAsync | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| `/api/v1/admin/orders/{orderId:int}/delivery-agent` | `DELETE Unassign` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UnassignAsync | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| `/api/v1/admin/orders/{orderId:int}/status-history` | `GET GetStatusHistory` | `AppResponse<List<OrderStatusHistoryDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetOrderStatusHistoryAsync | `WebAPI/Controllers/Admin/AdminDeliveryController.cs` |
| `/api/v1/admin/stores/{storeId:int}/delivery-zones` | `GET GetAll` | `AppResponse<List<StoreDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminAsync | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| `/api/v1/admin/stores/{storeId:int}/delivery-zones/{id:int}` | `GET GetById` | `AppResponse<StoreDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminByIdAsync | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| `/api/v1/admin/stores/{storeId:int}/delivery-zones` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateAsync | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| `/api/v1/admin/stores/{storeId:int}/delivery-zones/{id:int}` | `PUT Update` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateAsync | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| `/api/v1/admin/stores/{storeId:int}/delivery-zones/{id:int}/active` | `PATCH SetActive` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetActiveAsync | `WebAPI/Controllers/Admin/AdminDeliveryZonesController.cs` |
| `/api/v1/admin/employees` | `GET GetAll` | `AppResponse<List<PharmacyDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAllAsync | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| `/api/v1/admin/employees/deleted` | `GET GetDeleted` | `AppResponse<List<EmployeeDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetDeletedAsync | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| `/api/v1/admin/employees/{id:int}` | `GET GetById` | `AppResponse<PharmacyDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetByIdAsync | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| `/api/v1/admin/employees` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateAsync | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| `/api/v1/admin/employees/{id:int}` | `PUT Update` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateAsync | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| `/api/v1/admin/employees/{id:int}/active` | `PATCH SetActive` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetActiveAsync | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| `/api/v1/admin/employees/{id:int}` | `DELETE Delete` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SoftDeleteAsync | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| `/api/v1/admin/employees/{id:int}/restore` | `POST Restore` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method RestoreAsync | `WebAPI/Controllers/Admin/AdminEmployeesController.cs` |
| `/api/v1/admin/governorates` | `GET GetAll` | `AppResponse<List<GovernorateDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetGovernoratesAsync | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| `/api/v1/admin/governorates/{id:int}` | `GET GetById` | `AppResponse<GovernorateDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetGovernorateByIdAsync | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| `/api/v1/admin/governorates` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateGovernorateAsync | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| `/api/v1/admin/governorates/{id:int}` | `PUT Update` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateGovernorateAsync | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| `/api/v1/admin/governorates/{id:int}/active` | `PATCH SetActive` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetGovernorateActiveAsync | `WebAPI/Controllers/Admin/AdminGovernoratesController.cs` |
| `/api/v1/admin/inventory/products/{productId:int}/receipts` | `POST Receive` | `AppResponse<InventoryTransactionDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method ReceiveAsync | `WebAPI/Controllers/Admin/AdminInventoryController.cs` |
| `/api/v1/admin/inventory/adjustments` | `POST Adjust` | `AppResponse<InventoryTransactionDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method AdjustAsync | `WebAPI/Controllers/Admin/AdminInventoryController.cs` |
| `/api/v1/admin/inventory/transactions` | `GET Search` | `AppResponse<InventoryTransactionPageDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SearchAsync | `WebAPI/Controllers/Admin/AdminInventoryController.cs` |
| `/api/v1/admin/orders` | `GET GetAll` | `AppResponse<List<AdminOrderListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminOrdersAsync | `WebAPI/Controllers/Admin/AdminOrdersController.cs` |
| `/api/v1/admin/orders/{id:int}` | `GET GetById` | `AppResponse<AdminOrderDetailsDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminOrderAsync | `WebAPI/Controllers/Admin/AdminOrdersController.cs` |
| `/api/v1/admin/orders/{id:int}/status` | `PATCH UpdateStatus` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateOrderStatusAsync | `WebAPI/Controllers/Admin/AdminOrdersController.cs` |
| `/api/v1/admin/orders/{orderId:int}/payments` | `GET GetOrderPayments` | `AppResponse<List<PaymentAttemptDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetOrderPaymentsAsync | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| `/api/v1/admin/orders/{orderId:int}/payment-attempts` | `POST CreateAttempt` | `AppResponse<PaymentAttemptDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateAttemptAsync | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| `/api/v1/admin/payment-attempts/{attemptId:int}` | `PATCH UpdateAttempt` | `AppResponse<PaymentAttemptDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateAttemptAsync | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| `/api/v1/admin/orders/{orderId:int}/refunds` | `POST CreateRefund` | `AppResponse<PaymentRefundDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateRefundAsync | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| `/api/v1/admin/payment-refunds/{refundId:int}` | `PATCH UpdateRefund` | `AppResponse<PaymentRefundDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateRefundAsync | `WebAPI/Controllers/Admin/AdminPaymentsController.cs` |
| `/api/v1/admin/pharmacies` | `GET GetAll` | `AppResponse<List<PharmacyDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAllAsync | `WebAPI/Controllers/Admin/AdminPharmaciesController.cs` |
| `/api/v1/admin/pharmacies/{id:int}` | `GET GetById` | `AppResponse<PharmacyDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetByIdAsync | `WebAPI/Controllers/Admin/AdminPharmaciesController.cs` |
| `/api/v1/admin/pharmacies` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateAsync | `WebAPI/Controllers/Admin/AdminPharmaciesController.cs` |
| `/api/v1/admin/pharmacies/{id:int}` | `PUT Update` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateAsync | `WebAPI/Controllers/Admin/AdminPharmaciesController.cs` |
| `/api/v1/admin/points/customers/{customerId:int}/adjust` | `POST Adjust` | `AppResponse<ManualPointsAdjustmentResultDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method AdjustCustomerPointsAsync | `WebAPI/Controllers/Admin/AdminPointsController.cs` |
| `/api/v1/admin/products/{productId:int}/images` | `GET List` | `List<ProductImageDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| `/api/v1/admin/products/{productId:int}/images` | `POST Add` | `ProductImageDto` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| `/api/v1/admin/products/{productId:int}/images/{imageId:long}` | `PUT Update` | `ProductImageDto` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| `/api/v1/admin/products/{productId:int}/images/{imageId:long}/file` | `PUT ReplaceFile` | `File/binary response` | 200 on FromAppResponse success; error uses wrapper StatusCode | Direct Controller result | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| `/api/v1/admin/products/{productId:int}/images/{imageId:long}` | `DELETE Delete` | `No response body` | 200 on FromAppResponse success; error uses wrapper StatusCode | Direct Controller result | `WebAPI/Controllers/Admin/AdminProductAssetsController.cs` |
| `/api/v1/admin/products/{productId:int}/tags` | `GET List` | `List<anonymous {tagId, nameAr, nameEn, isActive}>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/Admin/AdminProductTagsController.cs` |
| `/api/v1/admin/products/{productId:int}/tags` | `PUT Replace` | `anonymous {productId, tagIds}` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/Admin/AdminProductTagsController.cs` |
| `/api/v1/admin/companies` | `GET Companies` | `AppResponse<List<CompanyDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetCompaniesAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/companies/{id:int}` | `GET Company` | `AppResponse<CompanyDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetCompanyAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/companies` | `POST CreateCompany` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateCompanyAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/companies/{id:int}` | `PUT UpdateCompany` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateCompanyAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/companies/{id:int}` | `DELETE DeleteCompany` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method DeleteCompanyAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/companies/{id:int}/restore` | `PATCH RestoreCompany` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method DeleteCompanyAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/units` | `GET Units` | `AppResponse<List<UnitDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetUnitsAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/units/{id:int}` | `GET Unit` | `AppResponse<UnitDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetUnitAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/units` | `POST CreateUnit` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateUnitAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/units/{id:int}` | `PUT UpdateUnit` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateUnitAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/units/{id:int}` | `DELETE DeleteUnit` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetUnitActiveAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/units/{id:int}/activate` | `PATCH ActivateUnit` | `AppResponse<bool>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetUnitActiveAsync | `WebAPI/Controllers/Admin/AdminReferenceDataController.cs` |
| `/api/v1/admin/returns` | `GET Search` | `AppResponse<List<ReturnRequestListItemDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminReturnsAsync | `WebAPI/Controllers/Admin/AdminReturnsController.cs` |
| `/api/v1/admin/returns/{id:int}` | `GET GetById` | `AppResponse<ReturnRequestDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminReturnAsync | `WebAPI/Controllers/Admin/AdminReturnsController.cs` |
| `/api/v1/admin/returns/{id:int}/decision` | `PATCH Decide` | `AppResponse<ReturnReviewResultDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method ReviewAsync | `WebAPI/Controllers/Admin/AdminReturnsController.cs` |
| `/api/v1/admin/stores` | `GET GetAll` | `AppResponse<List<StoreDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminAsync | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| `/api/v1/admin/stores/{id:int}` | `GET GetById` | `AppResponse<StoreDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAdminByIdAsync | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| `/api/v1/admin/stores` | `POST Create` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method CreateAsync | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| `/api/v1/admin/stores/{id:int}` | `PUT Update` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method UpdateAsync | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| `/api/v1/admin/stores/{id:int}/active` | `PATCH SetActive` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetActiveAsync | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| `/api/v1/admin/stores/{id:int}/default` | `PATCH SetDefault` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetDefaultAsync | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| `/api/v1/admin/stores/{id:int}` | `DELETE Delete` | `AppResponse<int>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SoftDeleteAsync | `WebAPI/Controllers/Admin/AdminStoresController.cs` |
| `/api/admin/roles` | `GET GetAllRoles` | `AppResponse<List<RoleDto>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetAllRolesAsync | `WebAPI/Controllers/Auth/AdminRolesController.cs` |
| `/api/admin/users/{userId:int}/permissions` | `GET Get` | `AppResponse<List<string>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetUserPermissionsAsync | `WebAPI/Controllers/Auth/AdminUsersPermissionsController.cs` |
| `/api/admin/users/{userId:int}/permissions` | `PUT Set` | `AppResponse<List<string>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetUserPermissionsAsync | `WebAPI/Controllers/Auth/AdminUsersPermissionsController.cs` |
| `/api/admin/users/{userId:int}/roles` | `GET GetUserRoles` | `AppResponse<UserRolesResponseDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method GetUserRolesAsync | `WebAPI/Controllers/Auth/AdminUsersRolesController.cs` |
| `/api/admin/users/{userId:int}/roles` | `PUT SetRoles` | `AppResponse<UserRolesResponseDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method SetRolesAsync | `WebAPI/Controllers/Auth/AdminUsersRolesController.cs` |
| `/api/admin/users/{userId:int}/roles` | `POST AddRoles` | `AppResponse<UserRolesResponseDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method AddRolesAsync | `WebAPI/Controllers/Auth/AdminUsersRolesController.cs` |
| `/api/admin/users/{userId:int}/roles` | `DELETE RemoveRoles` | `AppResponse<UserRolesResponseDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Service method RemoveRolesAsync | `WebAPI/Controllers/Auth/AdminUsersRolesController.cs` |
| `/api/auth/register` | `POST Register` | `AppResponse<RegisterResponseDto>` | result.StatusCode | Service method RegisterAsync | `WebAPI/Controllers/Auth/AuthController.cs` |
| `/api/auth/confirm-email` | `POST ConfirmEmail` | `AppResponse<ConfirmEmailResponseDto>` | result.StatusCode | Service method ConfirmEmailAsync | `WebAPI/Controllers/Auth/AuthController.cs` |
| `/api/auth/confirm-email` | `GET ConfirmEmailGet` | `AppResponse<ConfirmEmailResponseDto>` | result.StatusCode | Service method ConfirmEmailAsync | `WebAPI/Controllers/Auth/AuthController.cs` |
| `/api/auth/login` | `POST Login` | `AppResponse<LoginResponseDto>` | result.StatusCode | Service method LoginAsync | `WebAPI/Controllers/Auth/AuthController.cs` |
| `/api/auth/resend-confirm-email` | `POST ResendConfirmEmail` | `AppResponse` | result.StatusCode | Service method ResendConfirmEmailAsync | `WebAPI/Controllers/Auth/AuthController.cs` |
| `/api/auth/forgot-password` | `POST ForgotPassword` | `AppResponse` | result.StatusCode | Service method ForgotPasswordAsync | `WebAPI/Controllers/Auth/AuthController.cs` |
| `/api/auth/reset-password` | `POST ResetPassword` | `AppResponse` | result.StatusCode | Service method ResetPasswordAsync | `WebAPI/Controllers/Auth/AuthController.cs` |
| `/api/auth/change-password` | `POST ChangePassword` | `AppResponse` | result.StatusCode | Service method ChangePasswordAsync | `WebAPI/Controllers/Auth/AuthController.cs` |
| `/api/v1/me` | `GET Get` | `AppResponse<CurrentUserDto>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/Auth/CurrentUserController.cs` |
| `/api/v1/me/permissions` | `GET GetPermissions` | `AppResponse<List<string>>` | 200 on FromAppResponse success; error uses wrapper StatusCode | Concrete response from Controller/Service | `WebAPI/Controllers/Auth/CurrentUserController.cs` |
| `/api/v1/me` | `PUT UpdateCustomerProfile` | `File/binary response` | 200 on FromAppResponse success; error uses wrapper StatusCode | Direct Controller result | `WebAPI/Controllers/Auth/CurrentUserController.cs` |
