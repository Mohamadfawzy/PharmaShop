# API Endpoints Overview

هذا الجرد مأخوذ من Actions الفعلية في `WebAPI/Controllers` بعد استبعاد أسطر التعليقات. العدد **199 عملية HTTP**. يشمل المسارات المخفية عن Swagger ما دامت Action حية، ويستبعد الكود القديم المعلّق.

## الوحدات وعدد العمليات

| الوحدة | العدد | مسارات/Controllers رئيسية |
|---|---:|---|
| Authentication / Access | 15 | `AuthController`, `AdminRolesController`, `AdminUsersRolesController`, `AdminUsersPermissionsController` |
| Users / Profile | 3 | `CurrentUserController` |
| Products | 11 | كتالوج العميل، تفاصيل المنتج، وإدارة المنتجات |
| Product tags | 2 | ربط الوسوم بالمنتج |
| Categories | 14 | استعراض العميل وإدارة شجرة التصنيفات |
| Companies / Units | 14 | `ReferenceDataController`, `AdminReferenceDataController` |
| Tags | 7 | واجهة عامة وإدارة الوسوم |
| Promotions | 11 | عروض العميل وإدارة العروض وربط المنتجات |
| Promotional banners | 8 | بنرات الهاتف واستهداف المنتج/العرض/التصنيف/الشركة |
| Locations | 12 | المحافظات والمناطق: قراءة عامة وإدارة إدارية |
| Addresses | 6 | عناوين العميل |
| Stores / Pharmacies | 13 | القراءة العامة وإدارة الصيدليات والمتاجر |
| Delivery zones | 6 | القراءة العامة وإدارة مناطق التوصيل حسب المتجر |
| Employees | 9 | ملف الموظف الحالي وإدارة الموظفين |
| Prescriptions | 12 | وصفات العميل ومراجعة الموظف |
| Cart | 7 | عناصر السلة ومعاينتها |
| Orders / Checkout | 7 | Checkout والطلبات للعميل والإدارة |
| Returns | 6 | طلبات إرجاع العميل ومراجعتها إداريًا |
| Notifications | 5 | إشعارات العميل وتفضيلاته |
| Points | 5 | رصيد العميل وسجله وإدارة التعديل |
| Inventory | 3 | استلام وتسوية وبحث الحركات |
| Delivery | 8 | قوائم المندوبين، الإسناد، وحالة التوصيل |
| Payments | 5 | تسجيل محاولات الدفع والاسترداد يدويًا |
| Images | 10 | صور المنتجات ومسارات أدوات الصور التطويرية |
| **الإجمالي** | **199** | |

## الفهرس الكامل

كل مسار وطريقة HTTP واسم Action ومدخلاتها مدرج في الجداول التالية، بواقع 73 عملية غير إدارية و126 عملية إدارية:

- [جداول تطبيق الهاتف والعميل والموظف والمندوب](mobile-client.md#جميع-endpoints-غير-الإدارية)
- [جداول لوحة الإدارة](admin-dashboard.md#جميع-endpoints-الإدارية)
- [التفاصيل التنفيذية لكل Route وController/Service](API_ENDPOINTS_DETAILS.md)

## التصنيف حسب الوصول

| النوع | المعنى |
|---|---|
| Public | لا يوجد `[Authorize]` على Controller/Action؛ قد تبقى قيود مجال في الخدمة. |
| Authenticated | يلزم JWT صالح دون دور محدد. |
| Customer | policy تتطلب دور `Customer`. |
| Admin | دور `Admin` عبر `AdminBaseApiController` أو policy صريحة. |
| Permission | policy مثل `CanManageOrders`؛ Admin مسموح أيضًا وفق إعداد السياسات. |
| Employee review | `CanReviewPrescriptions` أو Admin. |
| Delivery | يتطلب دور `DeliveryAgent` وpolicy `CanDeliverOrders`. |

## النطاقات غير المتاحة أو المؤجلة

- لا توجد حاليًا واجهات ERP أو مزامنة؛ تكامل ERP مؤجل.
- لا توجد واجهة OTP أو Refresh Token أو Logout/Revoke Token في التنفيذ الحالي.
- الدفع الحالي إدارة سجلات داخلية للمحاولات والاستردادات، ولا يتصل ببوابة دفع خارجية.
- لا توجد وحدة Brands مستقلة؛ الكيان المستخدم للجهة المصنعة هو Company.
- خمس عمليات `/api/dev/images/*` فعلية لكنها تحمل `ApiExplorerSettings(IgnoreApi = true)` وتتطلب Admin وتعيد 404 خارج Development.


## جدول Route مختصر شامل

الجدول يجمع كل صفوف الـendpoint من جداول الهاتف والإدارة السابقة، ويضيف التصنيف الوظيفي. أسماء Action والمداخل لا تغيرها هذه القائمة.

| الوحدة | Method | Route | العملية |
|---|---|---|---|
| Addresses | GET | `/api/v1/addresses` | GET GetAll |
| Addresses | GET | `/api/v1/addresses/{id}` | GET GetById |
| Addresses | POST | `/api/v1/addresses` | POST Create |
| Addresses | PUT | `/api/v1/addresses/{id}` | PUT Update |
| Addresses | DELETE | `/api/v1/addresses/{id}` | DELETE Delete |
| Addresses | PATCH | `/api/v1/addresses/{id}/default` | PATCH SetDefault |
| Authentication / Access | POST | `/api/auth/register` | POST Register |
| Authentication / Access | POST | `/api/auth/confirm-email` | POST ConfirmEmail |
| Authentication / Access | GET | `/api/auth/confirm-email` | GET ConfirmEmailGet |
| Authentication / Access | POST | `/api/auth/login` | POST Login |
| Authentication / Access | POST | `/api/auth/resend-confirm-email` | POST ResendConfirmEmail |
| Authentication / Access | POST | `/api/auth/forgot-password` | POST ForgotPassword |
| Authentication / Access | POST | `/api/auth/reset-password` | POST ResetPassword |
| Authentication / Access | POST | `/api/auth/change-password` | POST ChangePassword |
| Users / Profile | GET | `/api/v1/me` | GET Get |
| Users / Profile | GET | `/api/v1/me/permissions` | GET GetPermissions |
| Users / Profile | PUT | `/api/v1/me` | PUT UpdateCustomerProfile |
| Cart | POST | `/api/v1/cart/items` | Add normal cart item |
| Cart | POST | `/api/v1/cart/redeem/items` | Add points-redemption item |
| Cart | GET | `/api/v1/cart` | Get my active cart |
| Cart | PATCH | `/api/v1/cart/items/{cartItemId}` | Update cart item quantity |
| Cart | DELETE | `/api/v1/cart/items/{cartItemId}` | Remove one item |
| Cart | DELETE | `/api/v1/cart` | Clear my active cart |
| Cart | POST | `/api/v1/cart/preview` | Preview cart and delivery |
| Categories | GET | `/api/v1/categories` | Categories list |
| Categories | GET | `/api/v1/categories/{categoryId}` | GET GetById |
| Categories | GET | `/api/v1/categories/root` | GET GetRoots |
| Categories | GET | `/api/v1/categories/tree` | Category tree |
| Employees | GET | `/api/v1/me/employee` | GET Get |
| Returns | PATCH | `/api/v1/orders/{orderId}/cancel` | Customer - cancel order before shipping |
| Returns | POST | `/api/v1/orders/{orderId}/returns` | Customer - request partial return |
| Returns | GET | `/api/v1/orders/{orderId}/returns/{returnRequestId}` | Customer - view return request |
| Delivery | GET | `/api/v1/delivery/orders` | Delivery agent - list assigned orders |
| Delivery | GET | `/api/v1/delivery/orders/{id}` | Delivery agent - order details |
| Delivery | PATCH | `/api/v1/delivery/orders/{id}/status` | Delivery agent - mark delivered |
| Delivery zones | GET | `/api/v1/delivery-zones` | GET GetByStore |
| Prescriptions | GET | `/api/v1/employee/prescriptions` | Search review queue |
| Prescriptions | GET | `/api/v1/employee/prescriptions/{id}` | Get prescription review details and images |
| Prescriptions | GET | `/api/v1/employee/prescriptions/{id}/images/{imageIdentifier}` | Download prescription image |
| Prescriptions | GET | `/api/v1/employee/prescriptions/{id}/items` | Get prescription items |
| Prescriptions | POST | `/api/v1/employee/prescriptions/{id}/items` | Add one mapped medicine |
| Prescriptions | POST | `/api/v1/employee/prescriptions/{id}/items/batch` | Add several medicines |
| Prescriptions | DELETE | `/api/v1/employee/prescriptions/{id}/items/{itemId}` | Delete an item while InReview |
| Prescriptions | PATCH | `/api/v1/employee/prescriptions/{id}/status` | Start review (Submitted to InReview) |
| Locations | GET | `/api/v1/locations/governorates` | GET GetGovernorates |
| Locations | GET | `/api/v1/locations/governorates/{id}/areas` | GET GetAreas |
| Notifications | GET | `/api/v1/notifications` | List my notifications |
| Notifications | PATCH | `/api/v1/notifications/{id}/read` | Mark notification as read |
| Notifications | PATCH | `/api/v1/notifications/read-all` | Mark all notifications as read |
| Notifications | GET | `/api/v1/notifications/preferences` | Get notification preferences |
| Notifications | PUT | `/api/v1/notifications/preferences` | Update notification preferences |
| Orders / Checkout | POST | `/api/v1/orders/checkout` | Confirm regular cart checkout |
| Orders / Checkout | POST | `/api/v1/orders/checkout/preview` | Preview regular cart checkout |
| Orders / Checkout | GET | `/api/v1/orders` | List my orders |
| Orders / Checkout | GET | `/api/v1/orders/{id}` | Get my order details |
| Points | GET | `/api/v1/points/balance` | Get my point balance |
| Points | GET | `/api/v1/points/lots` | List my point lots |
| Points | GET | `/api/v1/points/transactions` | List my points ledger |
| Points | GET | `/api/v1/points/redeemable-products` | List redeemable products |
| Prescriptions | POST | `/api/v1/prescriptions` | Customer - upload prescription images |
| Prescriptions | GET | `/api/v1/prescriptions` | List my prescriptions (optional status/page filters) |
| Prescriptions | GET | `/api/v1/prescriptions/{id}` | Get my prescription details |
| Prescriptions | GET | `/api/v1/prescriptions/{id}/images/{imageIdentifier}` | Customer - view own prescription image |
| Products | GET | `/api/v1/products` | Search products (advanced) |
| Products | GET | `/api/v1/products/{id}` | Product details |
| Promotions | GET | `/api/v1/promotions/active` | List active promotions |
| Promotions | GET | `/api/v1/promotions/{id}` | Get active promotion detail |
| Promotional banners | GET | `/api/v1/banners` | List active banners by optional placement |
| Tags | GET | `/api/v1/tags` | Tags list |
| Companies / Units | GET | `/api/v1/companies` | Companies list |
| Companies / Units | GET | `/api/v1/units` | Units list |
| Stores / Pharmacies | GET | `/api/v1/stores` | GET GetAll |
| Stores / Pharmacies | GET | `/api/v1/stores/{id}` | GET GetById |
| Locations | GET | `/api/v1/admin/areas` | GET GetAll |
| Locations | GET | `/api/v1/admin/areas/{id}` | GET GetById |
| Locations | POST | `/api/v1/admin/areas` | POST Create |
| Locations | PUT | `/api/v1/admin/areas/{id}` | PUT Update |
| Locations | PATCH | `/api/v1/admin/areas/{id}/active` | PATCH SetActive |
| Categories | GET | `/api/v1/admin/categories` | GET GetAll |
| Categories | GET | `/api/v1/admin/categories/{categoryId}` | GET GetById |
| Categories | POST | `/api/v1/admin/categories` | Create category |
| Categories | PUT | `/api/v1/admin/categories/{categoryId}` | Update category |
| Categories | PUT | `/api/v1/admin/categories/{categoryId}/parent` | PUT ChangeParent |
| Categories | PUT | `/api/v1/admin/categories/{categoryId}/image` | Admin - replace category image |
| Categories | PATCH | `/api/v1/admin/categories/{categoryId}/activate` | PATCH Activate |
| Categories | PATCH | `/api/v1/admin/categories/{categoryId}/deactivate` | Deactivate category |
| Categories | DELETE | `/api/v1/admin/categories/{categoryId}` | Delete category (soft) |
| Categories | PATCH | `/api/v1/admin/categories/{categoryId}/restore` | Restore category |
| Delivery | GET | `/api/v1/admin/delivery-agents` | Admin - list agents for store |
| Delivery | GET | `/api/v1/admin/orders/{orderId}/delivery-agent` | Admin - get active assignment |
| Delivery | PUT | `/api/v1/admin/orders/{orderId}/delivery-agent` | Admin - assign agent to preparing/out-for-delivery order |
| Delivery | DELETE | `/api/v1/admin/orders/{orderId}/delivery-agent` | Admin - unassign agent before delivery |
| Delivery | GET | `/api/v1/admin/orders/{orderId}/status-history` | Admin - order status history |
| Delivery zones | GET | `/api/v1/admin/stores/{storeId}/delivery-zones` | GET GetAll |
| Delivery zones | GET | `/api/v1/admin/stores/{storeId}/delivery-zones/{id}` | GET GetById |
| Delivery zones | POST | `/api/v1/admin/stores/{storeId}/delivery-zones` | POST Create |
| Delivery zones | PUT | `/api/v1/admin/stores/{storeId}/delivery-zones/{id}` | PUT Update |
| Delivery zones | PATCH | `/api/v1/admin/stores/{storeId}/delivery-zones/{id}/active` | PATCH SetActive |
| Employees | GET | `/api/v1/admin/employees` | GET GetAll |
| Employees | GET | `/api/v1/admin/employees/deleted` | GET GetDeleted |
| Employees | GET | `/api/v1/admin/employees/{id}` | GET GetById |
| Employees | POST | `/api/v1/admin/employees` | POST Create |
| Employees | PUT | `/api/v1/admin/employees/{id}` | PUT Update |
| Employees | PATCH | `/api/v1/admin/employees/{id}/active` | PATCH SetActive |
| Employees | DELETE | `/api/v1/admin/employees/{id}` | DELETE Delete |
| Employees | POST | `/api/v1/admin/employees/{id}/restore` | POST Restore |
| Locations | GET | `/api/v1/admin/governorates` | GET GetAll |
| Locations | GET | `/api/v1/admin/governorates/{id}` | GET GetById |
| Locations | POST | `/api/v1/admin/governorates` | POST Create |
| Locations | PUT | `/api/v1/admin/governorates/{id}` | PUT Update |
| Locations | PATCH | `/api/v1/admin/governorates/{id}/active` | PATCH SetActive |
| Inventory | POST | `/api/v1/admin/inventory/products/{productId}/receipts` | Receive stock |
| Inventory | POST | `/api/v1/admin/inventory/adjustments` | Increase stock adjustment |
| Inventory | GET | `/api/v1/admin/inventory/transactions` | Search inventory transactions |
| Orders / Checkout | GET | `/api/v1/admin/orders` | Search all orders (optional filters) |
| Orders / Checkout | GET | `/api/v1/admin/orders/{id}` | Get order details for administrator |
| Orders / Checkout | PATCH | `/api/v1/admin/orders/{id}/status` | Confirm order (deduct once) |
| Payments | GET | `/api/v1/admin/orders/{orderId}/payments` | Admin - list order payment attempts and refunds |
| Payments | POST | `/api/v1/admin/orders/{orderId}/payment-attempts` | Admin - record pending payment attempt |
| Payments | PATCH | `/api/v1/admin/payment-attempts/{attemptId}` | Admin - manually record payment result |
| Payments | POST | `/api/v1/admin/orders/{orderId}/refunds` | Admin - request a refund |
| Payments | PATCH | `/api/v1/admin/payment-refunds/{refundId}` | Admin - record completed external refund |
| Stores / Pharmacies | GET | `/api/v1/admin/pharmacies` | GET GetAll |
| Stores / Pharmacies | GET | `/api/v1/admin/pharmacies/{id}` | GET GetById |
| Stores / Pharmacies | POST | `/api/v1/admin/pharmacies` | POST Create |
| Stores / Pharmacies | PUT | `/api/v1/admin/pharmacies/{id}` | PUT Update |
| Points | POST | `/api/v1/admin/points/customers/{customerId}/adjust` | Grant points to customer |
| Images | GET | `/api/v1/admin/products/{productId}/images` | Admin - list product images |
| Images | POST | `/api/v1/admin/products/{productId}/images` | Admin - add product image |
| Images | PUT | `/api/v1/admin/products/{productId}/images/{imageId}` | Admin - update product image metadata |
| Images | PUT | `/api/v1/admin/products/{productId}/images/{imageId}/file` | Admin - replace product image file |
| Images | DELETE | `/api/v1/admin/products/{productId}/images/{imageId}` | Admin - soft-delete product image |
| Product tags | GET | `/api/v1/admin/products/{productId}/tags` | GET List |
| Product tags | PUT | `/api/v1/admin/products/{productId}/tags` | Replace product tags |
| Companies / Units | GET | `/api/v1/admin/companies` | GET Companies |
| Companies / Units | GET | `/api/v1/admin/companies/{id}` | GET Company |
| Companies / Units | POST | `/api/v1/admin/companies` | Create company |
| Companies / Units | PUT | `/api/v1/admin/companies/{id}` | Update company |
| Companies / Units | DELETE | `/api/v1/admin/companies/{id}` | Delete company (soft) |
| Companies / Units | PATCH | `/api/v1/admin/companies/{id}/restore` | Restore company |
| Companies / Units | GET | `/api/v1/admin/units` | GET Units |
| Companies / Units | GET | `/api/v1/admin/units/{id}` | GET Unit |
| Companies / Units | POST | `/api/v1/admin/units` | Create unit |
| Companies / Units | PUT | `/api/v1/admin/units/{id}` | Update unit |
| Companies / Units | DELETE | `/api/v1/admin/units/{id}` | Deactivate unit |
| Companies / Units | PATCH | `/api/v1/admin/units/{id}/activate` | Activate unit |
| Returns | GET | `/api/v1/admin/returns` | Admin/employee - search return requests |
| Returns | GET | `/api/v1/admin/returns/{id}` | Admin/employee - return details |
| Returns | PATCH | `/api/v1/admin/returns/{id}/decision` | Admin/employee - approve return |
| Stores / Pharmacies | GET | `/api/v1/admin/stores` | GET GetAll |
| Stores / Pharmacies | GET | `/api/v1/admin/stores/{id}` | GET GetById |
| Stores / Pharmacies | POST | `/api/v1/admin/stores` | POST Create |
| Stores / Pharmacies | PUT | `/api/v1/admin/stores/{id}` | PUT Update |
| Stores / Pharmacies | PATCH | `/api/v1/admin/stores/{id}/active` | PATCH SetActive |
| Stores / Pharmacies | PATCH | `/api/v1/admin/stores/{id}/default` | PATCH SetDefault |
| Stores / Pharmacies | DELETE | `/api/v1/admin/stores/{id}` | DELETE Delete |
| Authentication / Access | GET | `/api/admin/roles` | GET GetAllRoles |
| Authentication / Access | GET | `/api/admin/users/{userId}/permissions` | GET Get |
| Authentication / Access | PUT | `/api/admin/users/{userId}/permissions` | PUT Set |
| Authentication / Access | GET | `/api/admin/users/{userId}/roles` | GET GetUserRoles |
| Authentication / Access | PUT | `/api/admin/users/{userId}/roles` | PUT SetRoles |
| Authentication / Access | POST | `/api/admin/users/{userId}/roles` | POST AddRoles |
| Authentication / Access | DELETE | `/api/admin/users/{userId}/roles` | DELETE RemoveRoles |
| Images | POST | `/api/dev/images/save` | POST Save |
| Images | PUT | `/api/dev/images/{imageId}/replace` | PUT Replace |
| Images | DELETE | `/api/dev/images/{imageId}` | DELETE Delete |
| Images | POST | `/api/dev/images/batch` | POST Batch |
| Images | GET | `/api/dev/images/{imageId}/{variant}` | GET DownloadVariant |
| Products | POST | `/api/v1/admin/Products` | Create product |
| Products | PUT | `/api/v1/admin/Products/{id}` | Update product |
| Products | GET | `/api/v1/admin/Products/{id}` | Admin product details |
| Products | GET | `/api/v1/admin/Products/deleted` | List deleted products |
| Products | PATCH | `/api/v1/admin/Products/{id}/activate` | Activate product |
| Products | PATCH | `/api/v1/admin/Products/{id}/deactivate` | Deactivate product |
| Products | PATCH | `/api/v1/admin/Products/{id}/restore` | Restore product |
| Products | DELETE | `/api/v1/admin/Products/{id}` | Soft-delete product |
| Products | GET | `/api/v1/admin/Products/search` | Admin search |
| Promotions | POST | `/api/v1/admin/promotions` | Create group promotion (Buy 2, get offer 1) |
| Promotions | GET | `/api/v1/admin/promotions` | Search / filter promotions |
| Promotions | GET | `/api/v1/admin/promotions/{id}` | Get promotion details and products |
| Promotions | PUT | `/api/v1/admin/promotions/{id}` | Update promotion |
| Promotions | PATCH | `/api/v1/admin/promotions/{id}/activate` | Activate promotion |
| Promotions | PATCH | `/api/v1/admin/promotions/{id}/deactivate` | Deactivate promotion |
| Promotions | POST | `/api/v1/admin/promotions/{id}/products` | Add products |
| Promotions | DELETE | `/api/v1/admin/promotions/{id}/products/{promotionProductId}` | Remove product from promotion |
| Promotions | PUT | `/api/v1/admin/promotions/{id}/products` | Replace product set |
| Promotional banners | GET | `/api/v1/admin/banners` | Search admin banners |
| Promotional banners | GET | `/api/v1/admin/banners/{id}` | Get banner details |
| Promotional banners | POST | `/api/v1/admin/banners` | Create banner with multipart image |
| Promotional banners | PUT | `/api/v1/admin/banners/{id}` | Update banner and optionally replace image |
| Promotional banners | DELETE | `/api/v1/admin/banners/{id}` | Soft-delete banner |
| Promotional banners | PATCH | `/api/v1/admin/banners/{id}/active` | Activate or deactivate banner |
| Promotional banners | PATCH | `/api/v1/admin/banners/reorder` | Change display order |
| Tags | POST | `/api/v1/admin/Tags` | Create tag |
| Tags | GET | `/api/v1/admin/Tags` | GET GetAll |
| Tags | GET | `/api/v1/admin/Tags/{id}` | GET GetById |
| Tags | PUT | `/api/v1/admin/Tags/{id}` | Update tag |
| Tags | DELETE | `/api/v1/admin/Tags/{id}` | Deactivate tag |
| Tags | PATCH | `/api/v1/admin/Tags/{id}/activate` | PATCH Activate |
