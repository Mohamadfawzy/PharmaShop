# API لوحة الإدارة والتشغيل

هذا المرجع لمطور لوحة المدير، سواء كانت WPF أو Web Dashboard أو أداة تشغيل. يتصل العميل بالـ API عبر HTTPS/JWT؛ لا يتصل مباشرة بقاعدة البيانات. راجع [القواعد المشتركة](common.md) و[مرجع حقول DTOs وQuery models](request-models.md) للمصادقة، والاستجابات، والصلاحيات.

## الصلاحيات

- أغلب /api/v1/admin/* ترث AdminBaseApiController وتتطلب دور Admin.
- /api/admin/users و/api/admin/roles تتطلب Admin.
- الطلبات والإرجاعات والدفع وإسناد المندوب تعتمد CanManageOrders؛ المخزون CanManageInventory؛ تعديل النقاط CanManagePoints؛ مراجعة الوصفات CanReviewPrescriptions.
- السياسات التشغيلية قد تسمح لمستخدم يحمل permission claim، لكن بعض المسارات تجمع أكثر من policy. مثال تطبيق المندوب يتطلب DeliveryAgent وCanDeliverOrders.
- حذف الموظفين والمتاجر والمنتجات وبعض البيانات المرجعية حذف منطقي حيث توفر API استعادة. اعرض عمليات الاستعادة وفق endpoint المتاح لكل مجال.

## جلسة المستخدم الإداري

بعد تسجيل الدخول، استخدم GET /api/v1/me لمعرفة المستخدم والأدوار والـ permissions الحالية، وGET /api/v1/me/permissions لقراءة permissions الفعلية في التوكن. هذان المساران موثقان ضمن [قائمة تطبيق الهاتف والحساب](mobile-client.md)، ويعملان لأي مستخدم موثق. لا تستخدم PUT /api/v1/me من لوحة الإدارة؛ هذا المسار مخصص لتحديث ملف العميل بدور Customer.

## تدفقات إعداد وتشغيل لوحة الإدارة

### إعداد المتجر والبيانات المرجعية

1. أنشئ الصيدلية والمتجر.
2. أضف المحافظات والمناطق، ثم عرّف مناطق توصيل المتجر بربط governorateAreaIds. حدود المناطق تختلف حسب المتجر.
3. جهز الشركات والوحدات والتصنيفات والوسوم، ثم أنشئ المنتجات واربط صورها ووسومها.
4. سجل المخزون عبر receipts أو adjustments؛ اقرأ سجل transactions للتحقق من الأثر.

### الموظفون والوصول

- أنشئ موظفًا مرتبطًا بصيدلية، ثم فعّل/عطل الحساب أو احذفه منطقيًا. deleted تعرض المحذوفين وrestore يعيد السجل.
- إدارة بيانات الموظف مستقلة عن ضبط أدوار حساب المستخدم وpermission claims.
- عند تغيير الدور/permissions قد يحتاج العميل إلى توكن جديد كي تظهر claims المحدّثة في JWT.

### الطلبات والمرتجعات والمدفوعات

- ابحث في الطلبات، افتح التفاصيل وتاريخ الحالة، ثم حدثها مع اتباع انتقالات الخدمة.
- إسناد المندوب يتم إداريًا؛ تحديث المندوب لحالة التسليم يمر عبر Delivery API.
- قرار المرتجع قد يؤثر على المخزون والنقاط؛ اعرض نتيجة الرد للمسؤول.
- الدفع الحالي سجل محاولات واستردادات يدوي فقط؛ لا توجد بوابة أو webhook لمزود خارجي محدد.

## جميع Endpoints الإدارية

| Method | Endpoint | العملية (اسم الطلب في Postman) | المدخلات |
|---|---|---|---|
| GET | /api/v1/admin/areas | GET GetAll | Query: governorateId |
| GET | /api/v1/admin/areas/{id} | GET GetById | Path: id |
| POST | /api/v1/admin/areas | POST Create | JSON: GovernorateAreaWriteDto |
| PUT | /api/v1/admin/areas/{id} | PUT Update | Path: id<br>JSON: GovernorateAreaWriteDto |
| PATCH | /api/v1/admin/areas/{id}/active | PATCH SetActive | Path: id<br>JSON: LocationActiveDto |
| GET | /api/v1/admin/categories | GET GetAll | Query DTO: RequestParameters (pageNumber, pageSize, isActive, isDeleted, orderDescending) |
| GET | /api/v1/admin/categories/{categoryId} | GET GetById | Path: categoryId |
| POST | /api/v1/admin/categories | Create category | JSON: CategoryCreateDto |
| PUT | /api/v1/admin/categories/{categoryId} | Update category | Path: categoryId<br>JSON: CategoryUpdateDto |
| PUT | /api/v1/admin/categories/{categoryId}/parent | PUT ChangeParent | Path: categoryId<br>Query: newParentCategoryId |
| PUT | /api/v1/admin/categories/{categoryId}/image | Admin - replace category image | Path: categoryId<br>Query: format (ImageOutputFormat)<br>Multipart field: image (file) |
| PATCH | /api/v1/admin/categories/{categoryId}/activate | PATCH Activate | Path: categoryId |
| PATCH | /api/v1/admin/categories/{categoryId}/deactivate | Deactivate category | Path: categoryId |
| DELETE | /api/v1/admin/categories/{categoryId} | Delete category (soft) | Path: categoryId |
| PATCH | /api/v1/admin/categories/{categoryId}/restore | Restore category | Path: categoryId |
| GET | /api/v1/admin/delivery-agents | Admin - list agents for store | Query: storeId, page, pageSize (DeliveryAgentQueryDto) |
| GET | /api/v1/admin/orders/{orderId}/delivery-agent | Admin - get active assignment | Path: orderId |
| PUT | /api/v1/admin/orders/{orderId}/delivery-agent | Admin - assign agent to preparing/out-for-delivery order | Path: orderId<br>JSON: AssignDeliveryAgentDto |
| DELETE | /api/v1/admin/orders/{orderId}/delivery-agent | Admin - unassign agent before delivery | Path: orderId |
| GET | /api/v1/admin/orders/{orderId}/status-history | Admin - order status history | Path: orderId |
| GET | /api/v1/admin/stores/{storeId}/delivery-zones | GET GetAll | Path: storeId |
| GET | /api/v1/admin/stores/{storeId}/delivery-zones/{id} | GET GetById | Path: storeId, id |
| POST | /api/v1/admin/stores/{storeId}/delivery-zones | POST Create | Path: storeId<br>JSON: DeliveryZoneWriteDto |
| PUT | /api/v1/admin/stores/{storeId}/delivery-zones/{id} | PUT Update | Path: storeId, id<br>JSON: DeliveryZoneWriteDto |
| PATCH | /api/v1/admin/stores/{storeId}/delivery-zones/{id}/active | PATCH SetActive | Path: storeId, id<br>JSON: DeliveryZoneActiveDto |
| GET | /api/v1/admin/employees | GET GetAll | Query: pharmacyId, includeInactive |
| GET | /api/v1/admin/employees/deleted | GET GetDeleted | Query: pharmacyId |
| GET | /api/v1/admin/employees/{id} | GET GetById | Path: id |
| POST | /api/v1/admin/employees | POST Create | JSON: EmployeeCreateDto |
| PUT | /api/v1/admin/employees/{id} | PUT Update | Path: id<br>JSON: EmployeeUpdateDto |
| PATCH | /api/v1/admin/employees/{id}/active | PATCH SetActive | Path: id<br>JSON: EmployeeActiveDto |
| DELETE | /api/v1/admin/employees/{id} | DELETE Delete | Path: id |
| POST | /api/v1/admin/employees/{id}/restore | POST Restore | Path: id |
| GET | /api/v1/admin/governorates | GET GetAll | — |
| GET | /api/v1/admin/governorates/{id} | GET GetById | Path: id |
| POST | /api/v1/admin/governorates | POST Create | JSON: GovernorateWriteDto |
| PUT | /api/v1/admin/governorates/{id} | PUT Update | Path: id<br>JSON: GovernorateWriteDto |
| PATCH | /api/v1/admin/governorates/{id}/active | PATCH SetActive | Path: id<br>JSON: LocationActiveDto |
| POST | /api/v1/admin/inventory/products/{productId}/receipts | Receive stock | Path: productId<br>JSON: ReceiveStockDto |
| POST | /api/v1/admin/inventory/adjustments | Increase stock adjustment | JSON: AdjustInventoryDto |
| GET | /api/v1/admin/inventory/transactions | Search inventory transactions | Query: productId, page, pageSize, fromUtc, toUtc (InventoryTransactionQueryDto) |
| GET | /api/v1/admin/orders | Search all orders (optional filters) | Query: storeId, status, paymentStatus, orderNumber, page, pageSize (AdminOrderListQueryDto) |
| GET | /api/v1/admin/orders/{id} | Get order details for administrator | Path: id |
| PATCH | /api/v1/admin/orders/{id}/status | Confirm order (deduct once) | Path: id<br>JSON: OrderStatusUpdateDto |
| GET | /api/v1/admin/orders/{orderId}/payments | Admin - list order payment attempts and refunds | Path: orderId |
| POST | /api/v1/admin/orders/{orderId}/payment-attempts | Admin - record pending payment attempt | Path: orderId<br>JSON: CreatePaymentAttemptDto |
| PATCH | /api/v1/admin/payment-attempts/{attemptId} | Admin - manually record payment result | Path: attemptId<br>JSON: UpdatePaymentAttemptDto |
| POST | /api/v1/admin/orders/{orderId}/refunds | Admin - request a refund | Path: orderId<br>JSON: CreatePaymentRefundDto |
| PATCH | /api/v1/admin/payment-refunds/{refundId} | Admin - record completed external refund | Path: refundId<br>JSON: UpdatePaymentRefundDto |
| GET | /api/v1/admin/pharmacies | GET GetAll | — |
| GET | /api/v1/admin/pharmacies/{id} | GET GetById | Path: id |
| POST | /api/v1/admin/pharmacies | POST Create | JSON: PharmacyCreateDto |
| PUT | /api/v1/admin/pharmacies/{id} | PUT Update | Path: id<br>JSON: PharmacyUpdateDto |
| POST | /api/v1/admin/points/customers/{customerId}/adjust | Grant points to customer | Path: customerId<br>JSON: ManualPointsAdjustmentDto |
| GET | /api/v1/admin/products/{productId}/images | Admin - list product images | Path: productId |
| POST | /api/v1/admin/products/{productId}/images | Admin - add product image | Path: productId<br>Query: format (ImageOutputFormat)<br>Multipart fields: image (file), altText (optional), isPrimary (optional, default false) |
| PUT | /api/v1/admin/products/{productId}/images/{imageId} | Admin - update product image metadata | Path: productId, imageId<br>JSON: ProductImageUpdateDto |
| PUT | /api/v1/admin/products/{productId}/images/{imageId}/file | Admin - replace product image file | Path: productId, imageId<br>Query: format (ImageOutputFormat)<br>Multipart field: image (file) |
| DELETE | /api/v1/admin/products/{productId}/images/{imageId} | Admin - soft-delete product image | Path: productId, imageId |
| GET | /api/v1/admin/products/{productId}/tags | GET List | Path: productId |
| PUT | /api/v1/admin/products/{productId}/tags | Replace product tags | Path: productId<br>JSON: ProductTagsUpdateDto |
| GET | /api/v1/admin/companies | GET Companies | Query: includeInactive |
| GET | /api/v1/admin/companies/{id} | GET Company | Path: id |
| POST | /api/v1/admin/companies | Create company | — |
| PUT | /api/v1/admin/companies/{id} | Update company | Path: id |
| DELETE | /api/v1/admin/companies/{id} | Delete company (soft) | Path: id |
| PATCH | /api/v1/admin/companies/{id}/restore | Restore company | Path: id |
| GET | /api/v1/admin/units | GET Units | Query: includeInactive |
| GET | /api/v1/admin/units/{id} | GET Unit | Path: id |
| POST | /api/v1/admin/units | Create unit | — |
| PUT | /api/v1/admin/units/{id} | Update unit | Path: id |
| DELETE | /api/v1/admin/units/{id} | Deactivate unit | Path: id |
| PATCH | /api/v1/admin/units/{id}/activate | Activate unit | Path: id |
| GET | /api/v1/admin/returns | Admin/employee - search return requests | Query: status, page, pageSize, fromUtc, toUtc (ReturnRequestQueryDto) |
| GET | /api/v1/admin/returns/{id} | Admin/employee - return details | Path: id |
| PATCH | /api/v1/admin/returns/{id}/decision | Admin/employee - approve return | Path: id<br>JSON: ReviewReturnRequestDto |
| GET | /api/v1/admin/stores | GET GetAll | Query: pharmacyId |
| GET | /api/v1/admin/stores/{id} | GET GetById | Path: id |
| POST | /api/v1/admin/stores | POST Create | JSON: StoreCreateDto |
| PUT | /api/v1/admin/stores/{id} | PUT Update | Path: id<br>JSON: StoreUpdateDto |
| PATCH | /api/v1/admin/stores/{id}/active | PATCH SetActive | Path: id<br>JSON: StoreActiveDto |
| PATCH | /api/v1/admin/stores/{id}/default | PATCH SetDefault | Path: id |
| DELETE | /api/v1/admin/stores/{id} | DELETE Delete | Path: id |
| GET | /api/admin/roles | GET GetAllRoles | — |
| GET | /api/admin/users/{userId}/permissions | GET Get | Path: userId |
| PUT | /api/admin/users/{userId}/permissions | PUT Set | Path: userId<br>JSON: SetUserPermissionsRequestDto |
| GET | /api/admin/users/{userId}/roles | GET GetUserRoles | Path: userId |
| PUT | /api/admin/users/{userId}/roles | PUT SetRoles | Path: userId<br>JSON: SetUserRolesRequestDto |
| POST | /api/admin/users/{userId}/roles | POST AddRoles | Path: userId<br>JSON: ModifyUserRolesRequestDto |
| DELETE | /api/admin/users/{userId}/roles | DELETE RemoveRoles | Path: userId<br>JSON: ModifyUserRolesRequestDto |
| POST | /api/dev/images/save | POST Save | Query: prefix, format (ImageOutputFormat)<br>File: file (multipart) |
| PUT | /api/dev/images/{imageId}/replace | PUT Replace | Path: imageId<br>Query: format (ImageOutputFormat)<br>File: file (multipart) |
| DELETE | /api/dev/images/{imageId} | DELETE Delete | Path: imageId |
| POST | /api/dev/images/batch | POST Batch | Query: prefix, format (ImageOutputFormat)<br>Multipart field: files (up to 5 files) |
| GET | /api/dev/images/{imageId}/{variant} | GET DownloadVariant | Path: imageId, variant<br>Query: formatHint |
| POST | /api/v1/admin/Products | Create product | JSON: ProductCreateDto |
| PUT | /api/v1/admin/Products/{id} | Update product | Path: id<br>JSON: ProductCreateDto |
| GET | /api/v1/admin/Products/{id} | Admin product details | Path: id |
| GET | /api/v1/admin/Products/deleted | List deleted products | Query: page, pageSize |
| PATCH | /api/v1/admin/Products/{id}/activate | Activate product | Path: id |
| PATCH | /api/v1/admin/Products/{id}/deactivate | Deactivate product | Path: id |
| PATCH | /api/v1/admin/Products/{id}/restore | Restore product | Path: id |
| DELETE | /api/v1/admin/Products/{id} | Soft-delete product | Path: id |
| GET | /api/v1/admin/Products/search | Admin search | Query: storeId, q, page, pageSize, sort (ProductSearchQueryParams) |
| POST | /api/v1/admin/promotions | Create group promotion (Buy 2, get offer 1) | JSON: PromotionCreateDto |
| GET | /api/v1/admin/promotions | Search / filter promotions | Query: q, isActive, from, to, onlyRunningNow, sort, page, pageSize (PromotionListQueryDto) |
| GET | /api/v1/admin/promotions/{id} | Get promotion details and products | Path: id |
| PUT | /api/v1/admin/promotions/{id} | Update promotion | Path: id<br>JSON: PromotionUpdateDto |
| PATCH | /api/v1/admin/promotions/{id}/activate | Activate promotion | Path: id<br>JSON: PromotionActivateDto |
| PATCH | /api/v1/admin/promotions/{id}/deactivate | Deactivate promotion | Path: id |
| POST | /api/v1/admin/promotions/{id}/products | Add products | Path: id<br>JSON: PromotionAddProductsDto |
| DELETE | /api/v1/admin/promotions/{id}/products/{promotionProductId} | Remove product from promotion | Path: id, promotionProductId |
| PUT | /api/v1/admin/promotions/{id}/products | Replace product set | Path: id<br>JSON: PromotionReplaceProductsDto |
| POST | /api/v1/admin/Tags | Create tag | JSON: TagCreateDto |
| GET | /api/v1/admin/Tags | GET GetAll | Query DTO: TagQueryDto (q, isActive, sort, page, pageSize) |
| GET | /api/v1/admin/Tags/{id} | GET GetById | Path: id |
| PUT | /api/v1/admin/Tags/{id} | Update tag | Path: id<br>JSON: TagCreateDto |
| DELETE | /api/v1/admin/Tags/{id} | Deactivate tag | Path: id |
| PATCH | /api/v1/admin/Tags/{id}/activate | PATCH Activate | Path: id |

## رفع صور المنتجات

لإضافة صورة استخدم multipart fields: image كملف، altText اختياري حتى 200 حرف، وisPrimary اختياري. استبدال محتوى ملف الصورة يستخدم image فقط؛ تحديث altText/sortOrder/isPrimary يتم عبر JSON في ProductImageUpdateDto. format يقبل Auto أو Jpeg أو Png أو Webp.

## نماذج طلبات إدارية

### إنشاء موظف — EmployeeCreateDto

~~~json
{
  "pharmacyId": 1,
  "fullNameAr": "أحمد محمد",
  "fullNameEn": "Ahmed Mohamed",
  "phoneNumber": "+201000000001",
  "email": "employee@example.com",
  "nationalId": "12345678901234",
  "jobTitle": "صيدلي",
  "employeeCode": "EMP-001",
  "userName": "ahmed.pharmacist",
  "initialPassword": "ChangeMe123!",
  "roleName": "Employee"
}
~~~

### استبدال أدوار مستخدم

~~~json
{ "roles": ["Employee"] }
~~~

PUT يستبدل القائمة كاملة. POST يضيف الأدوار المحددة وDELETE يزيلها.

### استبدال صلاحيات مستخدم

~~~json
{ "permissions": ["CanReviewPrescriptions", "CanManageInventory"] }
~~~

صلاحيات معروفة تشمل CanManageProducts وCanManagePromotions وCanReviewPrescriptions وCanManageOrders وCanDeliverOrders وCanManagePoints وCanManageInventory وCanManageCatalog. تحقق من policy التي يحمي بها كل controller.

### إعداد منطقة توصيل — DeliveryZoneWriteDto

~~~json
{
  "nameAr": "وسط المدينة",
  "nameEn": "City Center",
  "deliveryFee": 25.00,
  "freeDeliveryMinimumAmount": 300.00,
  "governorateAreaIds": [1, 2, 3]
}
~~~

### إسناد مندوب إلى طلب — AssignDeliveryAgentDto

~~~json
{
  "deliveryAgentUserId": 5,
  "notes": "تم إسناد الطلب للمندوب المناوب"
}
~~~

### تسجيل محاولة دفع/طلب استرداد يدوي

~~~json
{ "provider": "Manual" }
~~~

~~~json
{
  "paymentAttemptId": 12,
  "amount": 100.00,
  "reason": "إرجاع معتمد"
}
~~~

هذه requests تسجل بيانات تشغيلية، ولا تعني تنفيذ عملية لدى مزود دفع.

## مسارات تطوير الصور

المسارات تحت /api/dev/images تتطلب Admin وتعمل فقط في بيئة Development. لا تعرضها في واجهة الإنتاج. رفع صور الإنتاج يتم من مسارات صور المنتجات والتصنيفات العادية.
