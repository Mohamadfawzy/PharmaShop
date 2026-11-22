# API تطبيق الهاتف والمستخدمين

هذا الدليل يغطي الكتالوج العام، العميل، الموظف ومندوب التوصيل. معظم واجهات العميل الخاصة تتطلب JWT بدور Customer. راجع [القواعد المشتركة](common.md) و[مرجع حقول DTOs وQuery models](request-models.md) للمصادقة والاستجابات والصلاحيات.

## تدفق العميل المقترح

1. اجلب المحافظات والمناطق العامة ثم المتاجر ومناطق التوصيل.
2. اعرض المنتجات والتصنيفات والوسوم والشركات والوحدات والعروض.
3. سجّل العميل أو سجّل دخوله، ثم خزّن accessToken بأمان. أرسل التوكن في كل طلب محمي.
4. أنشئ عنوانًا بإرسال معرف المحافظة والمنطقة؛ الخادم يسترجع اسميهما. يكتب المستخدم الشارع والعلامة المميزة واسم العنوان title، ويمكنه تعيينه افتراضيًا.
5. أضف المنتجات للسلة أو ارفع وصفة، ثم استخدم معاينة السلة/Checkout قبل التأكيد.
6. بعد مراجعة السعر والتحذيرات، أرسل Checkout. اعرض الطلب وحالته، واستخدم الإلغاء أو الإرجاع فقط ضمن قواعد حالة الطلب.

### عنوان توصيل — CustomerAddressWriteDto

~~~json
{
  "title": "المنزل",
  "governorateId": 1,
  "governorateAreaId": 1,
  "street": "شارع التحرير، مبنى 10، شقة 4",
  "landmark": "بجوار الصيدلية",
  "latitude": 30.0444,
  "longitude": 31.2357,
  "isDefault": true
}
~~~

### إضافة عنصر للسلة — CartAddItemDto

~~~json
{
  "storeId": 1,
  "productId": 1,
  "unitLevel": "Outer",
  "quantity": 2,
  "deviceId": "android-device-01",
  "appInstanceId": "pharmashop-mobile"
}
~~~

### معاينة/تأكيد Checkout

~~~json
{
  "sourceType": "Cart",
  "sourceId": 12,
  "addressId": 4,
  "paymentMethod": "COD"
}
~~~

sourceType يحدد Cart أو Prescription أو PointsRedemption. sourceId هو معرف المصدر الموافق. اعرض للمستخدم subtotal والخصومات ورسوم التوصيل وإجمالي الطلب والتحذيرات وتغيرات السعر التي ترجعها المعاينة قبل تأكيد الطلب. لا ترسل CustomerId؛ هو مستنتج من التوكن. هذا التوثيق يصف العقد الموجود ولا يغير Checkout.

### رفع وصفة طبية

POST /api/v1/prescriptions باستخدام multipart/form-data: الحقول storeId، files من 1 إلى 5 صور، primaryIndex اختياري، notes اختياري. يقبل JPG/JPEG أو PNG أو WebP بحد 10 MB لكل صورة و25 MB إجماليًا. الصور والبيانات متاحة للمالك، ويتيح مسار الموظف الوصول ضمن صلاحية المراجعة.

## جميع Endpoints غير الإدارية

| Method | Endpoint | العملية (اسم الطلب في Postman) | المدخلات |
|---|---|---|---|
| GET | /api/v1/addresses | GET GetAll | — |
| GET | /api/v1/addresses/{id} | GET GetById | Path: id |
| POST | /api/v1/addresses | POST Create | JSON: CustomerAddressWriteDto |
| PUT | /api/v1/addresses/{id} | PUT Update | Path: id<br>JSON: CustomerAddressWriteDto |
| DELETE | /api/v1/addresses/{id} | DELETE Delete | Path: id |
| PATCH | /api/v1/addresses/{id}/default | PATCH SetDefault | Path: id |
| POST | /api/auth/register | POST Register | JSON: RegisterRequestDto |
| POST | /api/auth/confirm-email | POST ConfirmEmail | JSON: ConfirmEmailRequestDto |
| GET | /api/auth/confirm-email | GET ConfirmEmailGet | Query: userId, token |
| POST | /api/auth/login | POST Login | JSON: LoginRequestDto |
| POST | /api/auth/resend-confirm-email | POST ResendConfirmEmail | JSON: ResendConfirmEmailRequestDto |
| POST | /api/auth/forgot-password | POST ForgotPassword | JSON: ForgotPasswordRequestDto |
| POST | /api/auth/reset-password | POST ResetPassword | JSON: ResetPasswordRequestDto |
| POST | /api/auth/change-password | POST ChangePassword | JSON: ChangePasswordRequestDto |
| GET | /api/v1/me | GET Get | — |
| GET | /api/v1/me/permissions | GET GetPermissions | — |
| PUT | /api/v1/me | PUT UpdateCustomerProfile | JSON: CustomerProfileUpdateDto |
| POST | /api/v1/cart/items | Add normal cart item | JSON: CartAddItemDto |
| POST | /api/v1/cart/redeem/items | Add points-redemption item | JSON: CartAddItemDto |
| GET | /api/v1/cart | Get my active cart | — |
| PATCH | /api/v1/cart/items/{cartItemId} | Update cart item quantity | Path: cartItemId<br>JSON: CartUpdateQtyDto |
| DELETE | /api/v1/cart/items/{cartItemId} | Remove one item | Path: cartItemId |
| DELETE | /api/v1/cart | Clear my active cart | — |
| POST | /api/v1/cart/preview | Preview cart and delivery | JSON: CartPreviewRequestDto |
| GET | /api/v1/categories | Categories list | Query: pageNumber, pageSize (RequestParameters) |
| GET | /api/v1/categories/{categoryId} | GET GetById | Path: categoryId |
| GET | /api/v1/categories/root | GET GetRoots | — |
| GET | /api/v1/categories/tree | Category tree | — |
| GET | /api/v1/me/employee | GET Get | — |
| PATCH | /api/v1/orders/{orderId}/cancel | Customer - cancel order before shipping | Path: orderId<br>JSON: CancelOrderDto |
| POST | /api/v1/orders/{orderId}/returns | Customer - request partial return | Path: orderId<br>JSON: CreateReturnRequestDto |
| GET | /api/v1/orders/{orderId}/returns/{returnRequestId} | Customer - view return request | Path: orderId, returnRequestId |
| GET | /api/v1/delivery/orders | Delivery agent - list assigned orders | Query: status, page, pageSize, newestFirst (DeliveryOrdersQueryDto) |
| GET | /api/v1/delivery/orders/{id} | Delivery agent - order details | Path: id |
| PATCH | /api/v1/delivery/orders/{id}/status | Delivery agent - mark delivered | Path: id<br>JSON: UpdateDeliveryOrderStatusDto |
| GET | /api/v1/delivery-zones | GET GetByStore | Query: storeId |
| GET | /api/v1/employee/prescriptions | Search review queue | Query: status, storeId, page, pageSize, sort (AdminPrescriptionListQueryDto) |
| GET | /api/v1/employee/prescriptions/{id} | Get prescription review details and images | Path: id |
| GET | /api/v1/employee/prescriptions/{id}/images/{imageIdentifier} | Download prescription image | Path: id, imageIdentifier |
| GET | /api/v1/employee/prescriptions/{id}/items | Get prescription items | Path: id |
| POST | /api/v1/employee/prescriptions/{id}/items | Add one mapped medicine | Path: id<br>JSON: PrescriptionItemCreateDto |
| POST | /api/v1/employee/prescriptions/{id}/items/batch | Add several medicines | Path: id<br>JSON: PrescriptionItemsBatchCreateDto |
| DELETE | /api/v1/employee/prescriptions/{id}/items/{itemId} | Delete an item while InReview | Path: id, itemId |
| PATCH | /api/v1/employee/prescriptions/{id}/status | Start review (Submitted to InReview) | Path: id<br>JSON: PrescriptionStatusUpdateDto |
| GET | /api/v1/locations/governorates | GET GetGovernorates | — |
| GET | /api/v1/locations/governorates/{id}/areas | GET GetAreas | Path: id |
| GET | /api/v1/notifications | List my notifications | Query: unreadOnly, page, pageSize (NotificationQueryDto) |
| PATCH | /api/v1/notifications/{id}/read | Mark notification as read | Path: id |
| PATCH | /api/v1/notifications/read-all | Mark all notifications as read | — |
| GET | /api/v1/notifications/preferences | Get notification preferences | — |
| PUT | /api/v1/notifications/preferences | Update notification preferences | JSON: NotificationPreferencesDto |
| POST | /api/v1/orders/checkout | Confirm regular cart checkout | JSON: CheckoutRequestDto |
| POST | /api/v1/orders/checkout/preview | Preview regular cart checkout | JSON: CheckoutPreviewRequestDto |
| GET | /api/v1/orders | List my orders | Query: status, page, pageSize, newestFirst (CustomerOrderListQueryDto) |
| GET | /api/v1/orders/{id} | Get my order details | Path: id |
| GET | /api/v1/points/balance | Get my point balance | — |
| GET | /api/v1/points/lots | List my point lots | Query: status, page, pageSize (PointsListQueryDto) |
| GET | /api/v1/points/transactions | List my points ledger | Query: transactionType, from, to, page, pageSize (PointsListQueryDto) |
| GET | /api/v1/points/redeemable-products | List redeemable products | Query: storeId, page, pageSize |
| POST | /api/v1/prescriptions | Customer - upload prescription images | Multipart: PrescriptionCreateFormData |
| GET | /api/v1/prescriptions | List my prescriptions (optional status/page filters) | Query: status, page, pageSize, newestFirst (CustomerPrescriptionListQueryDto) |
| GET | /api/v1/prescriptions/{id} | Get my prescription details | Path: id |
| GET | /api/v1/prescriptions/{id}/images/{imageIdentifier} | Customer - view own prescription image | Path: id, imageIdentifier |
| GET | /api/v1/products | Search products (advanced) | Query: storeId, q, categoryId, companyId, unitId, tagIds, tagMatch, minPrice, maxPrice, requiresPrescription, hasPromotion, isAvailable, isFeatured, allowSplitSale, minPoints, sort, page, pageSize (ProductSearchQueryParams) |
| GET | /api/v1/products/{id} | Product details | Path: id |
| GET | /api/v1/promotions/active | List active promotions | Query: page, pageSize, sort (PromotionListQueryDto) |
| GET | /api/v1/promotions/{id} | Get active promotion detail | Path: id |
| GET | /api/v1/banners | List current public banners | Query: placement (Home, Offers, Categories; optional) |
| GET | /api/v1/tags | Tags list | Query: page, pageSize, q (TagQueryDto) |
| GET | /api/v1/companies | Companies list | — |
| GET | /api/v1/units | Units list | — |
| GET | /api/v1/stores | GET GetAll | Query: pharmacyId |
| GET | /api/v1/stores/{id} | GET GetById | Path: id |

## ملاحظات حسب الوظيفة

### المصادقة والحساب

- التسجيل: POST /api/auth/register. الحد الأدنى مثال: {"email":"user@example.com","userName":"customer","password":"ChangeMe123!"}.
- الدخول: POST /api/auth/login؛ الحقول identifier وpassword وrememberMe. اقرأ accessToken من data.
- التأكيد وإعادة ضبط كلمة المرور تستخدم رموزًا يرسلها الخادم؛ لا تثبت token تجريبيًا في التطبيق.
- PUT /api/v1/me لتحديث ملف العميل يتطلب Customer. GET /me وGET /me/permissions تتطلب JWT.

### السلة والعروض والنقاط

- العناصر العادية عبر POST /api/v1/cart/items، وعناصر استرداد النقاط عبر POST /api/v1/cart/redeem/items. المصدر مختلف فلا تستبدل أحدهما بالآخر.
- Cart Preview يعرض مراجعة السلة. Checkout Preview يراجع المصدر والعنوان والدفع والسعر النهائي قبل الإرسال الفعلي.
- العروض العامة للعرض، وإدارة العروض في دليل لوحة الإدارة.
- رصيد النقاط وسجل اللوتات والمعاملات والمنتجات القابلة للاسترداد تخص المستخدم الموثق.

### الموظف ومراجعة الوصفة

GET /api/v1/me/employee يعرض بيانات الموظف الحالي (Employee أو DeliveryAgent). مراجعة الوصفات تتطلب CanReviewPrescriptions أو Admin. التدفق: قائمة الانتظار، التفاصيل والصور، العناصر، ثم تحديث الحالة. راجع قيم الحالة المقبولة في DTO/خدمة المجال ولا تخمّن انتقال الحالة.

### مندوب التوصيل

واجهات /api/v1/delivery/orders تتطلب دور DeliveryAgent وصلاحية CanDeliverOrders. المندوب يرى طلباته المسندة إليه فقط ويحدث حالتها بعد تنفيذ انتقال مسموح. إسناد المندوب إلى الطلب وظيفة إدارية.
