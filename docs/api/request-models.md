# مرجع نماذج الطلب (Request DTOs)

الحقول أدناه مأخوذة من DTOs المرتبطة بتواقيع Controllers. صيغة JSON تستخدم camelCase؛ أسماء الأنواع بأسمائها في C#. علامة ? تعني أن الحقل nullable. القيود هي Data Annotations؛ قد توجد قواعد عمل إضافية في Service أو Validators.

## AdjustInventoryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| productId | int | — |
| quantityDelta | decimal | — |
| reason | string | — |

## AdminOrderListQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| storeId | int? | — |
| status | byte? | — |
| paymentStatus | byte? | — |
| customerId | int? | — |
| orderNumber | string? | — |
| from | DateTime? | — |
| to | DateTime? | — |
| page | int | — |
| pageSize | int | — |

## AdminPrescriptionListQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| storeId | int? | — |
| status | byte? | — |
| customerId | int? | — |
| reviewedBy | int? | — |
| from | DateTime? | — |
| to | DateTime? | — |
| sort | PrescriptionSortOption | — |
| page | int | — |
| pageSize | int | — |

## AssignDeliveryAgentDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| deliveryAgentUserId | int | — |
| notes | string? | — |

## CancelOrderDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| reason | string | — |

## CartAddItemDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| storeId | int | — |
| productId | int | — |
| unitLevel | UnitLevel | — |
| quantity | decimal | — |
| deviceId | string? | — |
| appInstanceId | string? | — |

## CartPreviewRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| cartId | int | — |
| addressId | int | — |
| paymentMethod | PaymentMethod | — |

## CartUpdateQtyDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| quantity | decimal | — |

## CategoryCreateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| name | string | Required, MaxLength(30) |
| nameEn | string | — |
| description | string? | — |
| parentCategoryId | int? | — |

## CategoryUpdateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| id | int | — |
| name | string | — |
| nameEn | string | — |
| description | string? | — |
| parentCategoryId | int? | — |

## ChangePasswordRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| currentPassword | string | Required |
| newPassword | string | Required |

## CheckoutPreviewRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| customerId | int | JsonIgnore |
| sourceType | CheckoutSourceType | — |
| sourceId | int | — |
| addressId | int | — |
| paymentMethod | PaymentMethod | — |

## CheckoutRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| customerId | int | JsonIgnore |
| sourceType | CheckoutSourceType | — |
| sourceId | int | — |
| addressId | int | — |
| paymentMethod | PaymentMethod | — |

## ConfirmEmailRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| userId | int | Required Range(1, int.MaxValue) |
| token | string | Required MinLength(10) |

## CreatePaymentAttemptDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| provider | string | — |

## CreatePaymentRefundDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| paymentAttemptId | int | — |
| amount | decimal | — |
| reason | string | — |

## CreateReturnRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| reason | string | — |
| items | List<CreateReturnRequestItemDto> | — |

## CustomerAddressWriteDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| title | string | Required, MaxLength(100) |
| governorateId | int | Range(1, int.MaxValue) |
| governorateAreaId | int | Range(1, int.MaxValue) |
| street | string | Required, MaxLength(300) |
| landmark | string? | MaxLength(200) |
| latitude | double? | Range(-90, 90) |
| longitude | double? | Range(-180, 180) |
| isDefault | bool | — |

## CustomerOrderListQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte? | — |
| page | int | — |
| pageSize | int | — |
| newestFirst | bool | — |

## CustomerPrescriptionListQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte? | — |
| page | int | — |
| pageSize | int | — |
| newestFirst | bool | — |

## CustomerProfileUpdateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| fullNameAr | string | Required, MaxLength(200) |
| fullNameEn | string | Required, MaxLength(200) |
| phoneNumber | string? | MaxLength(20) |
| gender | string? | MaxLength(10) |
| dateOfBirth | DateOnly? | — |

## DeliveryAgentQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| storeId | int | — |
| page | int | — |
| pageSize | int | — |

## DeliveryOrdersQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte? | — |
| page | int | — |
| pageSize | int | — |
| newestFirst | bool | — |

## DeliveryZoneActiveDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| isActive | bool | — |

## DeliveryZoneWriteDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| nameAr | string | Required, MaxLength(150) |
| nameEn | string | Required, MaxLength(150) |
| deliveryFee | decimal | Range(typeof(decimal), "0", "10000000") |
| freeDeliveryMinimumAmount | decimal? | Range(typeof(decimal), "0", "10000000") |
| governorateAreaIds | List<int> | Required, MinLength(1) |

## EmployeeActiveDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| isActive | bool | — |

## EmployeeCreateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| pharmacyId | int | Range(1, int.MaxValue) |
| fullNameAr | string | Required, MaxLength(200) |
| fullNameEn | string? | MaxLength(200) |
| phoneNumber | string? | MaxLength(20) |
| email | string | Required, EmailAddress, MaxLength(256) |
| nationalId | string? | MaxLength(20) |
| jobTitle | string? | MaxLength(100) |
| employeeCode | string? | MaxLength(50) |
| userName | string? | MaxLength(256) |
| initialPassword | string | Required, MinLength(8), MaxLength(100) |
| roleName | string | Required |

## EmployeeUpdateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| pharmacyId | int | Range(1, int.MaxValue) |
| fullNameAr | string | Required, MaxLength(200) |
| fullNameEn | string? | MaxLength(200) |
| phoneNumber | string? | MaxLength(20) |
| nationalId | string? | MaxLength(20) |
| jobTitle | string? | MaxLength(100) |
| employeeCode | string? | MaxLength(50) |

## ForgotPasswordRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| email | string | Required, EmailAddress |

## GovernorateAreaWriteDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| governorateId | int | Range(1, int.MaxValue) |
| nameAr | string | Required, MaxLength(100) |
| nameEn | string | Required, MaxLength(100) |
| displayOrder | int | Range(0, int.MaxValue) |

## GovernorateWriteDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| nameAr | string | Required, MaxLength(100) |
| nameEn | string | Required, MaxLength(100) |
| displayOrder | int | Range(0, int.MaxValue) |

## InventoryTransactionQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| productId | int? | — |
| storeId | int? | — |
| employeeId | int? | — |
| orderId | int? | — |
| transactionType | byte? | — |
| fromUtc | DateTime? | — |
| toUtc | DateTime? | — |
| page | int | — |
| pageSize | int | — |

## LocationActiveDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| isActive | bool | — |

## LoginRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| identifier | string | Required MaxLength(256) |
| password | string | Required MaxLength(100) |
| rememberMe | bool | — |

## ManualPointsAdjustmentDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| pointsDelta | int | — |
| referenceId | int | — |
| reason | string | — |

## ModifyUserRolesRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| roles | List<string> | Required MinLength(1) |

## NotificationPreferencesDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| orderUpdatesEnabled | bool | — |
| prescriptionUpdatesEnabled | bool | — |
| pointsUpdatesEnabled | bool | — |
| emailEnabled | bool | — |

## NotificationQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| unreadOnly | bool? | — |
| page | int | — |
| pageSize | int | — |

## OrderStatusUpdateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte | — |
| notes | string? | — |

## PharmacyCreateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| name | string | Required, MaxLength(200) |
| nameEn | string | Required, MaxLength(200) |
| ownerName | string? | MaxLength(150) |
| licenseNumber | string? | MaxLength(100) |
| phoneNumber | string? | MaxLength(20) |
| email | string? | EmailAddress, MaxLength(150) |
| address | string? | MaxLength(300) |
| latitude | decimal? | — |
| longitude | decimal? | — |

## PharmacyUpdateDto

يرث من PharmacyCreateDto؛ راجع حقول النوع الأب أيضًا.

| JSON field | C# type | Validation attributes |
|---|---|---|
| isActive | bool | — |

## PointsListQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte? | — |
| transactionType | byte? | — |
| from | DateTime? | — |
| to | DateTime? | — |
| page | int | — |
| pageSize | int | — |

## PrescriptionCreateFormData

| JSON field | C# type | Validation attributes |
|---|---|---|
| storeId | int | — |
| notes | string? | — |
| primaryIndex | int? | — |
| files | List<IFormFile> | — |

## PrescriptionItemCreateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| productId | int? | — |
| requestedName | string | — |
| requestedQuantity | decimal? | — |
| notes | string? | — |

## PrescriptionItemsBatchCreateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| items | List<PrescriptionItemCreateDto> | — |

## PrescriptionStatusUpdateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte | — |
| rejectReason | string? | — |
| notes | string? | — |

## ProductCreateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| storeId | int | Required |
| categoryId | int | Required |
| companyId | int? | — |
| erpProductId | decimal? | — |
| erpStoreId | int? | — |
| internationalCode | string? | StringLength(50) |
| nameAr | string | Required, StringLength(250) |
| nameEn | string? | StringLength(250) |
| descriptionAr | string? | — |
| descriptionEn | string? | — |
| searchKeywords | string? | StringLength(500) |
| outerUnitId | int | Required |
| innerUnitId | int? | — |
| innerPerOuter | int? | — |
| outerUnitPrice | decimal | Range(0, double.MaxValue) |
| innerUnitPrice | decimal? | Range(0, double.MaxValue) |
| minOrderQty | int | Range(1, int.MaxValue) |
| maxOrderQty | int? | — |
| maxPerDayQty | int? | — |
| isReturnable | bool | — |
| isRedeemableByPoints | bool | — |
| allowSplitSale | bool | — |
| quantity | decimal | Range(0, double.MaxValue) |
| hasExpiry | bool | — |
| nearestExpiryDate | DateOnly? | — |
| lastStockSyncAt | DateTime? | — |
| hasPromotion | bool | — |
| promotionDiscountPercent | decimal | Range(0, 100) |
| promotionStartsAt | DateTime? | — |
| promotionEndsAt | DateTime? | — |
| isFeatured | bool | — |
| points | int | Range(0, int.MaxValue) |
| requiresPrescription | bool | — |
| isAvailable | bool | — |
| isActive | bool | — |

## ProductSearchQueryParams

| JSON field | C# type | Validation attributes |
|---|---|---|
| storeId | int? | — |
| q | string? | — |
| categoryId | int? | — |
| companyId | int? | — |
| unitId | int? | — |
| isFeatured | bool? | — |
| allowSplitSale | bool? | — |
| minQuantity | decimal? | — |
| maxQuantity | decimal? | — |
| minPoints | int? | — |
| maxPoints | int? | — |
| requiresPrescription | bool? | — |
| hasPromotion | bool? | — |
| isAvailable | bool? | — |
| isActive | bool? | — |
| minPrice | decimal? | — |
| maxPrice | decimal? | — |
| tagIds | List<int>? | — |
| tagMatch | TagMatchMode | — |
| sort | ProductSortOption | — |
| page | int | — |
| pageSize | int | — |

## PromotionActivateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| isActive | bool | — |

## PromotionAddProductsDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| items | List<PromotionProductCreateItemDto> | — |

## PromotionCreateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| erpPgoId | decimal? | — |
| name | string? | — |
| notes | string? | — |
| startAt | DateTime? | — |
| endAt | DateTime? | — |
| totalAmount | int? | — |
| basicAmount | int? | — |
| offerAmount | int? | — |
| discountPercent | decimal? | — |
| isActive | bool? | — |

## PromotionListQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| q | string? | — |
| isActive | bool? | — |
| from | DateTime? | — |
| to | DateTime? | — |
| onlyRunningNow | bool? | — |
| sort | PromotionSortOption | — |
| page | int | — |
| pageSize | int | — |

## PromotionReplaceProductsDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| items | List<PromotionProductReplaceItemDto> | — |

## PromotionUpdateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| erpPgoId | decimal? | — |
| name | string? | — |
| notes | string? | — |
| startAt | DateTime? | — |
| endAt | DateTime? | — |
| totalAmount | int? | — |
| basicAmount | int? | — |
| offerAmount | int? | — |
| discountPercent | decimal? | — |
| isActive | bool? | — |
| rowVersion | string? | — |

## ReceiveStockDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| storeId | int | — |
| productUnitId | int | — |
| quantity | int | — |
| batchNumber | string | — |
| expirationDate | DateTime? | — |
| costPrice | decimal? | — |
| reason | string? | — |

## RegisterRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| email | string | Required EmailAddress MaxLength(256) |
| userName | string? | MaxLength(50) |
| password | string | Required MinLength(8) MaxLength(100) |

## RequestParameters

| JSON field | C# type | Validation attributes |
|---|---|---|
| pageNumber | int | Default 1; no DataAnnotation range in this class |
| pageSize | int | Default 10; setter clamps values above 50 to 50; values below 1 are not clamped here |
| isActive | bool? | — |
| isDeleted | bool? | — |
| orderDescending | bool | — |

## PagingParameters

| JSON field | C# type | Validation attributes |
|---|---|---|
| pageNumber | int | Default 1, Range(1, int.MaxValue) |
| pageSize | int | Default 10, Range(1, 10) |

هذا النوع منفصل عن RequestParameters؛ لا تفترض أن حدود الصفحة واحدة في كل Endpoint.

## ResendConfirmEmailRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| email | string | Required, EmailAddress |

## ResetPasswordRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| userId | int | Required |
| token | string | Required |
| newPassword | string | Required |

## ReturnRequestQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte? | — |
| orderId | int? | — |
| customerId | int? | — |
| fromUtc | DateTime? | — |
| toUtc | DateTime? | — |
| page | int | — |
| pageSize | int | — |

## ReviewReturnRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| approve | bool | — |
| notes | string? | — |

## SetUserPermissionsRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| permissions | List<string> | — |

## SetUserRolesRequestDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| roles | List<string> | Required MinLength(1, ErrorMessage = "At least one role is required.") |

## StoreActiveDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| isActive | bool | — |

## StoreCreateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| pharmacyId | int | Range(1, int.MaxValue) |
| nameAr | string | Required, MaxLength(150) |
| nameEn | string | Required, MaxLength(150) |
| code | string? | MaxLength(30) |
| address | string? | MaxLength(300) |
| isDefault | bool | — |

## StoreUpdateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| nameAr | string | Required, MaxLength(150) |
| nameEn | string | Required, MaxLength(150) |
| code | string? | MaxLength(30) |
| address | string? | MaxLength(300) |
| isActive | bool | — |
| rowVersion | string? | — |

## TagCreateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| nameAr | string? | — |
| nameEn | string? | — |
| isActive | bool? | — |

## TagQueryDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| q | string? | — |
| isActive | bool? | — |
| sort | string? | — |
| page | int | — |
| pageSize | int | — |

## UpdateDeliveryOrderStatusDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte | — |
| notes | string? | — |

## UpdatePaymentAttemptDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte | — |
| externalReference | string? | — |
| failureReason | string? | — |

## UpdatePaymentRefundDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| status | byte | — |
| externalReference | string? | — |

## ImageOutputFormat

- Auto = 0
- Jpeg = 1
- Png = 2
- Webp = 3

## ProductImageUpdateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| altText | string? | — |
| sortOrder | int? | — |
| isPrimary | bool? | — |

## ProductTagsUpdateDto

| JSON field | C# type | Validation attributes |
|---|---|---|
| tagIds | List<int> | — |

# Enums used by request models

## CheckoutSourceType

- Cart = 1
- Prescription = 2
- PointsRedemption = 3

## PaymentMethod

- CashOnDelivery = 0
- Card = 1
- COD = 2

## PrescriptionSortOption

- Newest = 1
- Oldest = 2
- StatusUpdatedDesc = 3

## ProductSortOption

- Id = 6
- NameAsc = 0
- NameDesc = 1
- PriceAsc = 2
- PriceDesc = 3
- Newest = 4
- Oldest = 5

## PromotionSortOption

- Newest = 1
- Oldest = 2
- StartAtAsc = 3
- StartAtDesc = 4

## TagMatchMode

- Any = 0
- All = 1

## UnitLevel

- Outer = 1
- Inner = 2

## Promotional banners

Admin `POST /api/v1/admin/banners` and `PUT /api/v1/admin/banners/{id}` use multipart fields in `BannerAdminFormDto`: `name`, `placement`, `actionType`, `targetId`, `parentTargetId`, `actionUrl`, `image`, `altText`, `displayOrder`, `isActive`, `startsAt`, and `endsAt`. Create requires image; update accepts it optionally. See [promotional-banners.md](promotional-banners.md) for constraints and examples.

`PATCH /api/v1/admin/banners/{id}/active` uses `BannerSetActiveDto { isActive: bool }`. `PATCH /api/v1/admin/banners/reorder` uses `BannerReorderDto { items: [{ id, displayOrder }] }`.
