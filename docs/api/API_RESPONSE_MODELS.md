# DTO Response and Request Model Reference

هذا الفهرس المساعد يستخرج الأنواع والخصائص العامة من DTO source الحالي. يشمل نماذج Requests وResponses حتى يمكن تتبع الخصائص المتداخلة؛ اتجاه الاستخدام يحدده توقيع Action. أسماء JSON أدناه متوقعة وفق camelCase المسجل في MVC. الأنواع nullable موضحة بعلامة ?. راجع كل ملف مصدر عند وجود attributes أو mapping خاص.

## AddToCartResponseDto

Source: Shared/Models/Dtos/Cart/AddToCartResponseDto.cs

| JSON property | C# type |
|---|---|
| cartId | int |
| cartStatus | CartStatus |
| cartItemId | int |
| productId | int |
| unitLevel | UnitLevel |
| sourceType | CartItemSourceType |
| quantity | decimal |
| availableQty | decimal |
| exceedsAvailableQty | bool |
| requiresRefresh | bool |
| refreshReason | string? |
| priceChange | ItemPriceChangeDto? |

## ItemPriceChangeDto

Source: Shared/Models/Dtos/Cart/AddToCartResponseDto.cs

| JSON property | C# type |
|---|---|
| oldPrice | decimal |
| newPrice | decimal |
| changedAtUtc | DateTime |

## CartAddItemDto

Source: Shared/Models/Dtos/Cart/CartAddItemDto.cs

| JSON property | C# type |
|---|---|
| storeId | int |
| productId | int |
| unitLevel | UnitLevel |
| quantity | decimal |
| deviceId | string? |
| appInstanceId | string? |

## CartItemViewDto

Source: Shared/Models/Dtos/Cart/CartItemViewDto.cs

| JSON property | C# type |
|---|---|
| cartItemId | int |
| productId | int |
| sourceType | CartItemSourceType |
| unitLevel | byte |
| quantity | decimal |
| nameAr | string |
| nameEn | string? |
| primaryImageUrl | string? |
| currentUnitPrice | decimal? |
| currentUnitPriceSnapshot | decimal |
| availableQty | decimal |
| exceedsAvailableQty | bool |
| isRedeemableByPoints | bool |
| isActive | bool |
| isAvailable | bool |
| deletedAt | DateTime? |
| isValid | bool |
| invalidReason | string? |

## CartPreviewData

Source: Shared/Models/Dtos/Cart/CartPreviewData.cs

| JSON property | C# type |
|---|---|
| cartId | int |
| storeId | int |
| status | byte |
| items | List<CartPreviewItemData> |

## CartPreviewItemData

Source: Shared/Models/Dtos/Cart/CartPreviewItemData.cs

| JSON property | C# type |
|---|---|
| cartItemId | int |
| productId | int |
| unitLevel | byte |
| quantity | decimal |
| currentUnitPriceSnapshot | decimal |
| nameAr | string |
| nameEn | string? |
| primaryImageUrl | string? |
| outerUnitPrice | decimal |
| innerUnitPrice | decimal? |
| innerPerOuter | int? |
| stockOuterQty | decimal |
| isActive | bool |
| isAvailable | bool |
| deletedAt | DateTime? |
| hasPromotion | bool |
| promotionDiscountPercent | decimal |
| promotionStartsAt | DateTime? |
| promotionEndsAt | DateTime? |
| points | int |
| allowSplitSale | bool |
| isInGroupPromotion | bool |

## CartPreviewItemDto

Source: Shared/Models/Dtos/Cart/CartPreviewItemDto.cs

| JSON property | C# type |
|---|---|
| cartItemId | int |
| productId | int |
| unitLevel | byte |
| quantity | decimal |
| nameAr | string |
| nameEn | string? |
| primaryImageUrl | string? |
| currentUnitPrice | decimal |
| currentUnitPriceSnapshot | decimal |
| lineTotal | decimal |
| availableQty | decimal |
| exceedsAvailableQty | bool |
| isInGroupPromotion | bool |

## CartPreviewRequestDto

Source: Shared/Models/Dtos/Cart/CartPreviewRequestDto.cs

| JSON property | C# type |
|---|---|
| cartId | int |
| addressId | int |
| paymentMethod | PaymentMethod |

## CartPreviewResponseDto

Source: Shared/Models/Dtos/Cart/CartPreviewResponseDto.cs

| JSON property | C# type |
|---|---|
| cartId | int |
| canProceed | bool |
| subtotal | decimal |
| promotionDiscountTotal | decimal |
| subtotalAfterPromotions | decimal |
| maxRedeemablePoints | int |
| requestedRedeemPoints | int |
| appliedRedeemPoints | int |
| redeemValueMoney | decimal |
| deliveryFee | decimal |
| grandTotal | decimal |
| hasPriceChanges | bool |
| warnings | List<string> |
| items | List<CartPreviewItemDto> |

## CartUpdateQtyDto

Source: Shared/Models/Dtos/Cart/CartUpdateQtyDto.cs

| JSON property | C# type |
|---|---|
| quantity | decimal |

## CartViewDto

Source: Shared/Models/Dtos/Cart/CartViewDto.cs

| JSON property | C# type |
|---|---|
| cartId | int? |
| storeId | int? |
| status | byte? |
| statusUpdatedAt | DateTime? |
| subtotalCurrent | decimal |
| items | List<CartItemViewDto> |

## CategoryCreateDto

Source: Shared/Models/Dtos/Category/CategoryCreateDto.cs

| JSON property | C# type |
|---|---|
| name | string |
| nameEn | string |
| description | string? |
| parentCategoryId | int? |

## CategoryDetailsDto

Source: Shared/Models/Dtos/Category/CategoryDetailsDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| name | string |
| nameEn | string |
| description | string? |
| imageId | string? |
| imageFormat | byte? |
| imageVersion | int |
| isActive | bool |
| isDeleted | bool |
| createdAt | DateTime |
| updatedAt | DateTime? |
| imageUpdatedAt | DateTime? |
| parentCategoryId | int? |

## CategoryDto

Source: Shared/Models/Dtos/Category/CategoryDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| name | string |
| nameEn | string |
| description | string? |
| imageId | string? |
| imageFormat | byte? |
| imageVersion | int |
| parentCategoryId | int? |
| isActive | bool |

## CategoryTreeDto

Source: Shared/Models/Dtos/Category/CategoryTreeDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| name | string |
| nameEn | string |
| imageUrl | string? |
| isActive | bool |
| children | List<CategoryTreeDto> |

## CategoryUpdateDto

Source: Shared/Models/Dtos/Category/CategoryUpdateDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| name | string |
| nameEn | string |
| description | string? |
| parentCategoryId | int? |

## CustomerAddressDto

Source: Shared/Models/Dtos/Customer/CustomerAddressDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| title | string |
| governorateId | int |
| governorateNameAr | string |
| governorateAreaId | int |
| areaNameAr | string |
| areaNameEn | string |
| city | string |
| region | string? |
| street | string |
| landmark | string? |
| latitude | double? |
| longitude | double? |
| isDefault | bool |
| createdAt | DateTime |
| updatedAt | DateTime? |

## CustomerAddressWriteDto

Source: Shared/Models/Dtos/Customer/CustomerAddressDtos.cs

| JSON property | C# type |
|---|---|
| isDefault | bool |

## CustomerProfileDto

Source: Shared/Models/Dtos/Customer/CustomerProfileDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| fullNameAr | string |
| fullNameEn | string |
| phoneNumber | string? |
| email | string? |
| gender | string? |
| dateOfBirth | DateOnly? |

## CustomerProfileUpdateDto

Source: Shared/Models/Dtos/Customer/CustomerProfileDtos.cs

| JSON property | C# type |
|---|---|
| dateOfBirth | DateOnly? |

## AssignDeliveryAgentDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| deliveryAgentUserId | int |
| notes | string? |

## DeliveryAgentQueryDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| storeId | int |
| page | int |
| pageSize | int |

## DeliveryAgentDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| userId | int |
| employeeId | int |
| pharmacyId | int |
| fullNameAr | string |
| phoneNumber | string? |
| email | string? |

## DeliveryAgentAssignmentDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| orderId | int |
| deliveryAgentUserId | int |
| employeeId | int? |
| fullNameAr | string |
| phoneNumber | string? |
| assignedAt | DateTime |
| assignedByUserId | int? |
| notes | string? |

## DeliveryOrdersQueryDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| status | byte? |
| page | int |
| pageSize | int |
| newestFirst | bool |

## DeliveryOrderListItemDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| orderNumber | string |
| storeId | int |
| storeNameAr | string |
| status | byte |
| statusUpdatedAt | DateTime |
| deliveryTitle | string? |
| deliveryCity | string |
| deliveryRegion | string? |
| deliveryStreet | string |
| deliveryPhone | string? |
| assignedAt | DateTime |

## DeliveryOrderDetailsDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| notes | string? |
| items | List<DeliveryOrderItemDto> |

## DeliveryOrderItemDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| productId | int |
| productNameAr | string |
| unitLevel | byte |
| quantity | decimal |

## UpdateDeliveryOrderStatusDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| status | byte |
| notes | string? |

## OrderStatusHistoryDto

Source: Shared/Models/Dtos/Delivery/DeliveryDtos.cs

| JSON property | C# type |
|---|---|
| id | long |
| orderId | int |
| previousStatus | byte? |
| newStatus | byte |
| changedByUserId | int? |
| changedByUserName | string? |
| changedByRole | string |
| notes | string? |
| changedAt | DateTime |

## DeliveryZoneAreaItemDto

Source: Shared/Models/Dtos/Delivery/DeliveryZoneDtos.cs

| JSON property | C# type |
|---|---|
| governorateAreaId | int |
| governorateId | int |
| governorateNameAr | string |
| areaNameAr | string |
| areaNameEn | string |

## DeliveryZoneDto

Source: Shared/Models/Dtos/Delivery/DeliveryZoneDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| storeId | int |
| nameAr | string |
| nameEn | string |
| deliveryFee | decimal |
| freeDeliveryMinimumAmount | decimal? |
| isActive | bool |
| areas | List<DeliveryZoneAreaItemDto> |

## DeliveryZoneActiveDto

Source: Shared/Models/Dtos/Delivery/DeliveryZoneDtos.cs

| JSON property | C# type |
|---|---|
| isActive | bool |

## AdjustInventoryDto

Source: Shared/Models/Dtos/Inventory/InventoryDtos.cs

| JSON property | C# type |
|---|---|
| productId | int |
| quantityDelta | decimal |
| reason | string |

## InventoryTransactionQueryDto

Source: Shared/Models/Dtos/Inventory/InventoryDtos.cs

| JSON property | C# type |
|---|---|
| productId | int? |
| storeId | int? |
| employeeId | int? |
| orderId | int? |
| transactionType | byte? |
| fromUtc | DateTime? |
| toUtc | DateTime? |
| page | int |
| pageSize | int |

## InventoryTransactionDto

Source: Shared/Models/Dtos/Inventory/InventoryDtos.cs

| JSON property | C# type |
|---|---|
| id | long |
| productId | int |
| productName | string |
| storeId | int |
| employeeId | int? |
| employeeName | string? |
| orderId | int? |
| orderItemId | int? |
| transactionType | byte |
| productUnitId | int? |
| quantity | decimal |
| quantityBefore | decimal |
| quantityAfter | decimal |
| reason | string |
| batchNumber | string? |
| expirationDate | DateTime? |
| costPrice | decimal? |
| createdAt | DateTime |

## InventoryTransactionPageDto

Source: Shared/Models/Dtos/Inventory/InventoryDtos.cs

| JSON property | C# type |
|---|---|
| items | List<InventoryTransactionDto> |
| page | int |
| pageSize | int |
| totalCount | int |

## GovernorateDto

Source: Shared/Models/Dtos/Locations/LocationDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| nameAr | string |
| nameEn | string |
| isActive | bool |
| displayOrder | int |

## GovernorateAreaDto

Source: Shared/Models/Dtos/Locations/LocationDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| governorateId | int |
| governorateNameAr | string |
| nameAr | string |
| nameEn | string |
| isActive | bool |
| displayOrder | int |

## LocationActiveDto

Source: Shared/Models/Dtos/Locations/LocationDtos.cs

| JSON property | C# type |
|---|---|
| isActive | bool |

## NotificationQueryDto

Source: Shared/Models/Dtos/Notifications/NotificationDtos.cs

| JSON property | C# type |
|---|---|
| unreadOnly | bool? |
| page | int |
| pageSize | int |

## NotificationDto

Source: Shared/Models/Dtos/Notifications/NotificationDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| type | string |
| title | string |
| message | string |
| relatedEntityType | string? |
| relatedEntityId | long? |
| isRead | bool |
| createdAt | DateTime |
| readAt | DateTime? |

## NotificationPreferencesDto

Source: Shared/Models/Dtos/Notifications/NotificationDtos.cs

| JSON property | C# type |
|---|---|
| orderUpdatesEnabled | bool |
| prescriptionUpdatesEnabled | bool |
| pointsUpdatesEnabled | bool |
| emailEnabled | bool |

## ActiveGroupPromotionHit

Source: Shared/Models/Dtos/Order/ActiveGroupPromotionHit.cs

| JSON property | C# type |
|---|---|
| promotionId | int |
| basicAmount | int |
| offerAmount | int |
| discountPercent | decimal |

## CheckoutItemResultDto

Source: Shared/Models/Dtos/Order/CheckoutItemResultDto.cs

| JSON property | C# type |
|---|---|
| productId | int |
| unitLevel | byte |
| quantity | decimal |
| unitPrice | decimal |
| discountPercent | decimal |
| discountAmount | decimal |
| finalUnitPrice | decimal |
| lineTotal | decimal |
| appliedPromotionId | int? |

## CheckoutLineData

Source: Shared/Models/Dtos/Order/CheckoutLineData.cs

| JSON property | C# type |
|---|---|
| productId | int |
| unitLevel | byte |
| quantity | decimal |
| storeId | int |
| prescriptionId | int? |
| cartId | int? |
| outerUnitPrice | decimal |
| innerUnitPrice | decimal? |
| outerUnitId | int |
| innerUnitId | int? |
| innerPerOuter | int? |
| allowSplitSale | bool |
| points | int |
| isRedeemableByPoints | bool |
| cartItemSourceType | byte? |
| isProductActive | bool |
| isProductAvailable | bool |
| availableOuterQuantity | decimal |

## CheckoutPreviewRequestDto

Source: Shared/Models/Dtos/Order/CheckoutPreviewRequestDto.cs

| JSON property | C# type |
|---|---|
| customerId | int |
| sourceType | CheckoutSourceType |
| sourceId | int |
| addressId | int |
| paymentMethod | PaymentMethod |

## CheckoutPreviewResponseDto

Source: Shared/Models/Dtos/Order/CheckoutPreviewRequestDto.cs

| JSON property | C# type |
|---|---|
| sourceType | CheckoutSourceType |
| sourceId | int |
| subtotal | decimal |
| promotionDiscountTotal | decimal |
| subtotalAfterPromotions | decimal |
| maxRedeemablePoints | int |
| requestedRedeemPoints | int |
| appliedRedeemPoints | int |
| redeemValueMoney | decimal |
| deliveryFee | decimal |
| grandTotal | decimal |
| hasPriceChanges | bool |
| warnings | List<string> |
| items | List<CheckoutPreviewItemDto> |

## CheckoutPreviewItemDto

Source: Shared/Models/Dtos/Order/CheckoutPreviewRequestDto.cs

| JSON property | C# type |
|---|---|
| productId | int |
| unitLevel | byte |
| quantity | decimal |
| availableQuantity | decimal |
| isAvailable | bool |
| unitPrice | decimal |
| discountPercent | decimal |
| discountAmount | decimal |
| finalUnitPrice | decimal |
| lineTotal | decimal |
| appliedPromotionId | int? |
| snapshotPrice | decimal? |

## CheckoutRequestDto

Source: Shared/Models/Dtos/Order/CheckoutRequestDto.cs

| JSON property | C# type |
|---|---|
| customerId | int |
| sourceType | CheckoutSourceType |
| sourceId | int |
| addressId | int |
| paymentMethod | PaymentMethod |

## CheckoutResultDto

Source: Shared/Models/Dtos/Order/CheckoutResultDto.cs

| JSON property | C# type |
|---|---|
| orderId | int |
| orderNumber | string |
| subtotal | decimal |
| promotionDiscountTotal | decimal |
| subtotalAfterPromotions | decimal |
| redeemedPoints | int |
| redeemedAmount | decimal |
| discountAmount | decimal |
| deliveryFee | decimal |
| grandTotal | decimal |
| items | List<CheckoutItemResultDto> |

## CustomerAddressSnapshot

Source: Shared/Models/Dtos/Order/CustomerAddressSnapshot.cs

| JSON property | C# type |
|---|---|
| addressId | int |
| governorateAreaId | int |
| title | string? |
| city | string |
| region | string? |
| street | string |
| latitude | double? |
| longitude | double? |
| phone | string? |

## CustomerOrderListQueryDto

Source: Shared/Models/Dtos/Order/CustomerOrderDtos.cs

| JSON property | C# type |
|---|---|
| status | byte? |
| page | int |
| pageSize | int |
| newestFirst | bool |

## CustomerOrderListItemDto

Source: Shared/Models/Dtos/Order/CustomerOrderDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| orderNumber | string |
| storeId | int |
| orderSource | byte |
| status | byte |
| statusUpdatedAt | DateTime |
| paymentStatus | byte |
| grandTotal | decimal |
| createdAt | DateTime |

## CustomerOrderDetailsDto

Source: Shared/Models/Dtos/Order/CustomerOrderDtos.cs

| JSON property | C# type |
|---|---|
| paymentMethod | byte |
| subtotal | decimal |
| itemsDiscountTotal | decimal |
| deliveryFee | decimal |
| redeemedPoints | int |
| redeemedAmount | decimal |
| earnedPoints | int |
| prescriptionId | int? |
| deliveryTitle | string? |
| deliveryCity | string |
| deliveryRegion | string? |
| deliveryStreet | string |
| deliveryLatitude | double? |
| deliveryLongitude | double? |
| deliveryPhone | string? |
| notes | string? |
| items | List<OrderItemSnapshotDto> |

## AdminOrderDetailsDto

Source: Shared/Models/Dtos/Order/CustomerOrderDtos.cs

| JSON property | C# type |
|---|---|
| customerId | int |
| customerNameAr | string |
| customerPhone | string? |

## OrderItemSnapshotDto

Source: Shared/Models/Dtos/Order/CustomerOrderDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| productId | int |
| productNameAr | string |
| productNameEn | string? |
| unitLevel | byte |
| quantity | decimal |
| outerUnitIdSnapshot | int |
| innerUnitIdSnapshot | int? |
| innerPerOuterSnapshot | int? |
| unitPriceSnapshot | decimal |
| discountPercentSnapshot | decimal |
| finalUnitPriceSnapshot | decimal |
| lineTotal | decimal |
| pointsSnapshot | int |
| earnedPoints | int |
| appliedPromotionId | int? |

## AdminOrderListItemDto

Source: Shared/Models/Dtos/Order/order/AdminOrderListItemDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| orderNumber | string |
| customerId | int |
| storeId | int |
| status | byte |
| statusUpdatedAt | DateTime |
| paymentStatus | byte |
| grandTotal | decimal |
| prescriptionId | int? |
| createdAt | DateTime |

## AdminOrderListQueryDto

Source: Shared/Models/Dtos/Order/order/AdminOrderListQueryDto.cs

| JSON property | C# type |
|---|---|
| storeId | int? |
| status | byte? |
| paymentStatus | byte? |
| customerId | int? |
| orderNumber | string? |
| from | DateTime? |
| to | DateTime? |
| page | int |
| pageSize | int |

## OrderStatusUpdateDto

Source: Shared/Models/Dtos/Order/order/OrderStatusUpdateDto.cs

| JSON property | C# type |
|---|---|
| status | byte |
| notes | string? |

## EmployeeDto

Source: Shared/Models/Dtos/Organization/EmployeeDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| userId | int? |
| pharmacyId | int |
| pharmacyName | string |
| fullNameAr | string |
| fullNameEn | string? |
| phoneNumber | string? |
| email | string? |
| nationalId | string? |
| jobTitle | string? |
| employeeCode | string? |
| isActive | bool |
| createdAt | DateTime |
| deletedAt | DateTime? |

## EmployeeActiveDto

Source: Shared/Models/Dtos/Organization/EmployeeDtos.cs

| JSON property | C# type |
|---|---|
| isActive | bool |

## PharmacyDto

Source: Shared/Models/Dtos/Organization/PharmacyDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| name | string |
| nameEn | string |
| ownerName | string? |
| licenseNumber | string? |
| phoneNumber | string? |
| email | string? |
| address | string? |
| latitude | decimal? |
| longitude | decimal? |
| isActive | bool |

## PharmacyCreateDto

Source: Shared/Models/Dtos/Organization/PharmacyDtos.cs

| JSON property | C# type |
|---|---|
| latitude | decimal? |
| longitude | decimal? |

## PharmacyUpdateDto

Source: Shared/Models/Dtos/Organization/PharmacyDtos.cs

| JSON property | C# type |
|---|---|
| isActive | bool |

## StoreDto

Source: Shared/Models/Dtos/Organization/StoreDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| pharmacyId | int |
| pharmacyName | string |
| nameAr | string |
| nameEn | string |
| code | string? |
| address | string? |
| isDefault | bool |
| isActive | bool |
| rowVersion | string? |

## StoreCreateDto

Source: Shared/Models/Dtos/Organization/StoreDtos.cs

| JSON property | C# type |
|---|---|
| isDefault | bool |

## StoreUpdateDto

Source: Shared/Models/Dtos/Organization/StoreDtos.cs

| JSON property | C# type |
|---|---|
| isActive | bool |
| rowVersion | string? |

## StoreActiveDto

Source: Shared/Models/Dtos/Organization/StoreDtos.cs

| JSON property | C# type |
|---|---|
| isActive | bool |

## CreatePaymentAttemptDto

Source: Shared/Models/Dtos/Payments/PaymentDtos.cs

| JSON property | C# type |
|---|---|
| provider | string |

## UpdatePaymentAttemptDto

Source: Shared/Models/Dtos/Payments/PaymentDtos.cs

| JSON property | C# type |
|---|---|
| status | byte |
| externalReference | string? |
| failureReason | string? |

## CreatePaymentRefundDto

Source: Shared/Models/Dtos/Payments/PaymentDtos.cs

| JSON property | C# type |
|---|---|
| paymentAttemptId | int |
| amount | decimal |
| reason | string |

## UpdatePaymentRefundDto

Source: Shared/Models/Dtos/Payments/PaymentDtos.cs

| JSON property | C# type |
|---|---|
| status | byte |
| externalReference | string? |

## PaymentAttemptDto

Source: Shared/Models/Dtos/Payments/PaymentDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| orderId | int |
| provider | string |
| externalReference | string? |
| amount | decimal |
| status | byte |
| failureReason | string? |
| createdAt | DateTime |
| updatedAt | DateTime? |
| refunds | List<PaymentRefundDto> |

## PaymentRefundDto

Source: Shared/Models/Dtos/Payments/PaymentDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| paymentAttemptId | int |
| amount | decimal |
| status | byte |
| externalReference | string? |
| reason | string |
| requestedByUserId | int? |
| processedByUserId | int? |
| createdAt | DateTime |
| processedAt | DateTime? |

## PointsBalanceDto

Source: Shared/Models/Dtos/Points/PointsDtos.cs

| JSON property | C# type |
|---|---|
| availablePoints | int |
| pendingPoints | int |
| expiringWithin30Days | int |
| nextExpiryAt | DateTime? |
| availableValueEGP | decimal |

## PointsListQueryDto

Source: Shared/Models/Dtos/Points/PointsDtos.cs

| JSON property | C# type |
|---|---|
| status | byte? |
| transactionType | byte? |
| from | DateTime? |
| to | DateTime? |
| page | int |
| pageSize | int |

## CustomerPointLotDto

Source: Shared/Models/Dtos/Points/PointsDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| orderId | int? |
| pointsTotal | int |
| remainingPoints | int |
| status | byte |
| earnedAt | DateTime |
| availableAt | DateTime? |
| expiresAt | DateTime |

## CustomerPointTransactionDto

Source: Shared/Models/Dtos/Points/PointsDtos.cs

| JSON property | C# type |
|---|---|
| id | long |
| transactionType | byte |
| pointsDelta | int |
| balanceAfter | int? |
| orderId | int? |
| orderItemId | int? |
| lotId | int? |
| referenceType | byte? |
| referenceId | int? |
| expiresAt | DateTime? |
| notes | string? |
| createdAt | DateTime |

## RedeemablePointsProductDto

Source: Shared/Models/Dtos/Points/PointsDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| storeId | int |
| nameAr | string |
| nameEn | string? |
| outerUnitPrice | decimal |
| innerUnitPrice | decimal? |
| availableQuantity | decimal |
| outerUnitId | int |
| innerUnitId | int? |
| innerPerOuter | int? |
| allowSplitSale | bool |

## ManualPointsAdjustmentDto

Source: Shared/Models/Dtos/Points/PointsDtos.cs

| JSON property | C# type |
|---|---|
| pointsDelta | int |
| referenceId | int |
| reason | string |

## ManualPointsAdjustmentResultDto

Source: Shared/Models/Dtos/Points/PointsDtos.cs

| JSON property | C# type |
|---|---|
| customerId | int |
| pointsDelta | int |
| balanceAfter | int |
| transactionId | long |
| lotId | int? |

## AdminPrescriptionListItemDto

Source: Shared/Models/Dtos/Prescription/AdminPrescriptionListItemDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| customerId | int |
| storeId | int |
| status | byte |
| statusUpdatedAt | DateTime |
| reviewedBy | int? |
| readyForCheckoutAt | DateTime? |
| createdAt | DateTime |

## AdminPrescriptionListQueryDto

Source: Shared/Models/Dtos/Prescription/AdminPrescriptionListQueryDto.cs

| JSON property | C# type |
|---|---|
| storeId | int? |
| status | byte? |
| customerId | int? |
| reviewedBy | int? |
| from | DateTime? |
| to | DateTime? |
| sort | PrescriptionSortOption |
| page | int |
| pageSize | int |

## CustomerPrescriptionListQueryDto

Source: Shared/Models/Dtos/Prescription/CustomerPrescriptionDtos.cs

| JSON property | C# type |
|---|---|
| status | byte? |
| page | int |
| pageSize | int |
| newestFirst | bool |

## CustomerPrescriptionListItemDto

Source: Shared/Models/Dtos/Prescription/CustomerPrescriptionDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| storeId | int |
| status | byte |
| statusUpdatedAt | DateTime |
| createdAt | DateTime |
| readyForCheckoutAt | DateTime? |
| rejectReason | string? |
| primaryImageUrl | string? |

## CustomerPrescriptionDetailsDto

Source: Shared/Models/Dtos/Prescription/CustomerPrescriptionDtos.cs

| JSON property | C# type |
|---|---|
| notes | string? |
| images | List<PrescriptionCreatedImageDto> |
| items | List<PrescriptionItemListItemDto> |

## PrescriptionCreatedImageDto

Source: Shared/Models/Dtos/Prescription/PrescriptionCreatedImageDto.cs

| JSON property | C# type |
|---|---|
| id | long |
| imageUrl | string |
| thumbnailUrl | string? |
| isPrimary | bool |
| sortOrder | int |

## PrescriptionCreateRequestDto

Source: Shared/Models/Dtos/Prescription/PrescriptionCreateRequestDto.cs

| JSON property | C# type |
|---|---|
| storeId | int |
| notes | string? |
| primaryIndex | int? |

## PrescriptionCreateResultDto

Source: Shared/Models/Dtos/Prescription/PrescriptionCreateResultDto.cs

| JSON property | C# type |
|---|---|
| prescriptionId | int |
| status | byte |
| createdAt | DateTime |
| imagesCount | int |
| primaryImageId | long |
| images | List<PrescriptionCreatedImageDto> |

## PrescriptionItemCreatedDto

Source: Shared/Models/Dtos/Prescription/PrescriptionItemCreatedDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| prescriptionId | int |
| productId | int? |
| requestedName | string |
| requestedQuantity | decimal? |
| notes | string? |
| createdAt | DateTime |

## PrescriptionItemCreateDto

Source: Shared/Models/Dtos/Prescription/PrescriptionItemCreateDto.cs

| JSON property | C# type |
|---|---|
| productId | int? |
| requestedName | string |
| requestedQuantity | decimal? |
| notes | string? |

## PrescriptionItemListItemDto

Source: Shared/Models/Dtos/Prescription/PrescriptionItemListItemDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| prescriptionId | int |
| productId | int? |
| requestedName | string |
| requestedQuantity | decimal? |
| notes | string? |
| productNameAr | string? |
| productNameEn | string? |
| createdAt | DateTime |
| updatedAt | DateTime? |

## PrescriptionStatusUpdateDto

Source: Shared/Models/Dtos/Prescription/PrescriptionStatusUpdateDto.cs

| JSON property | C# type |
|---|---|
| status | byte |
| rejectReason | string? |
| notes | string? |

## ProductCreateDto

Source: Shared/Models/Dtos/Product/ProductCreateDto.cs

| JSON property | C# type |
|---|---|
| storeId | int |
| categoryId | int |
| companyId | int? |
| erpProductId | decimal? |
| erpStoreId | int? |
| internationalCode | string? |
| nameAr | string |
| nameEn | string? |
| descriptionAr | string? |
| descriptionEn | string? |
| searchKeywords | string? |
| outerUnitId | int |
| innerUnitId | int? |
| innerPerOuter | int? |
| outerUnitPrice | decimal |
| innerUnitPrice | decimal? |
| minOrderQty | int |
| maxOrderQty | int? |
| maxPerDayQty | int? |
| isReturnable | bool |
| isRedeemableByPoints | bool |
| allowSplitSale | bool |
| quantity | decimal |
| hasExpiry | bool |
| nearestExpiryDate | DateOnly? |
| lastStockSyncAt | DateTime? |
| hasPromotion | bool |
| promotionDiscountPercent | decimal |
| promotionStartsAt | DateTime? |
| promotionEndsAt | DateTime? |
| isFeatured | bool |
| points | int |
| requiresPrescription | bool |
| isAvailable | bool |
| isActive | bool |

## ProductDetailsDto

Source: Shared/Models/Dtos/Product/ProductDetailsDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| storeId | int |
| categoryId | int |
| companyId | int? |
| nameAr | string |
| nameEn | string? |
| descriptionAr | string? |
| descriptionEn | string? |
| internationalCode | string? |
| searchKeywords | string? |
| outerUnitId | int |
| innerUnitId | int? |
| innerPerOuter | int? |
| outerUnitPrice | decimal |
| innerUnitPrice | decimal? |
| quantity | decimal |
| isAvailable | bool |
| isActive | bool |
| requiresPrescription | bool |
| hasPromotion | bool |
| promotionDiscountPercent | decimal |
| promotionStartsAt | DateTime? |
| promotionEndsAt | DateTime? |
| isFeatured | bool |
| hasExpiry | bool |
| points | int |
| images | List<string> |
| tagIds | List<int> |
| createdAt | DateTime |
| updatedAt | DateTime? |

## ProductListItemDto

Source: Shared/Models/Dtos/Product/ProductListItemDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| nameAr | string |
| nameEn | string? |
| primaryImageUrl | string? |
| categoryNameAr | string? |
| companyNameAr | string? |
| outerUnitNameAr | string? |
| outerUnitPrice | decimal |
| hasPromotion | bool |
| promotionDiscountPercent | decimal |
| promotionStartsAt | DateTime? |
| promotionEndsAt | DateTime? |
| isFeatured | bool |
| points | int |
| requiresPrescription | bool |
| isAvailable | bool |
| isActive | bool |
| tagsCount | int |

## ProductStateChangeDto

Source: Shared/Models/Dtos/Product/ProductStateChangeDto.cs

| JSON property | C# type |
|---|---|
| rowVersion | byte[] |
| auditReason | string? |

## ProductSubDetailsDto

Source: Shared/Models/Dtos/Product/ProductSubDetailsDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| name | string |
| nameEn | string |
| description | string |
| descriptionEn | string |
| barcode | string |
| price | decimal |
| isAvailable | bool |
| points | decimal? |
| promoDisc | decimal? |
| promoEndDate | DateTime? |
| isGroupOffer | bool |
| imageName | string |

## ProductUpdateDto

Source: Shared/Models/Dtos/Product/ProductUpdateDto.cs

| JSON property | C# type |
|---|---|
| rowVersion | byte[] |
| categoryId | int |
| brandId | int? |
| barcode | string? |
| internationalCode | string? |
| stockProductCode | string? |
| name | string |
| nameEn | string |
| slug | string? |
| description | string? |
| descriptionEn | string? |
| auditReason | string? |
| searchKeywords | string? |
| dosageForm | string? |
| strength | string? |
| packSize | string? |
| unit | string? |
| requiresPrescription | bool |
| earnPoints | bool |
| hasExpiry | bool |
| ageRestricted | bool |
| minAge | int? |
| requiresColdChain | bool |
| controlledSubstance | bool |
| storageConditions | string? |
| isTaxable | bool |
| vatRate | decimal |
| taxCategoryCode | string? |
| minOrderQty | int |
| maxOrderQty | int? |
| maxPerDayQty | int? |
| isReturnable | bool |
| returnWindowDays | int? |
| allowSplitSale | bool |
| splitLevel | byte? |
| weightGrams | int? |
| lengthMm | int? |
| widthMm | int? |
| heightMm | int? |
| trackInventory | bool |
| isFeatured | bool |
| isActive | bool |

## ReceiveStockDto

Source: Shared/Models/Dtos/Product/ReceiveStockDto.cs

| JSON property | C# type |
|---|---|
| storeId | int |
| productUnitId | int |
| quantity | int |
| batchNumber | string |
| expirationDate | DateTime? |
| costPrice | decimal? |
| reason | string? |

## ReceiveStockResultDto

Source: Shared/Models/Dtos/Product/ReceiveStockResultDto.cs

| JSON property | C# type |
|---|---|
| productId | int |
| storeId | int |
| productUnitId | int |
| batchId | long |
| batchNumber | string |
| expirationDate | DateTime? |
| quantityReceived | int |
| batchQtyOnHandAfter | int |
| inventoryQtyOnHandAfter | int |

## StockAdjustmentDto

Source: Shared/Models/Dtos/Product/StockAdjustmentDto.cs

| JSON property | C# type |
|---|---|
| storeId | int |
| productUnitId | int |
| batchId | long? |
| batchNumber | string? |
| expirationDate | DateTime? |
| countedQty | int |
| costPrice | decimal? |
| reason | string? |

## StockAdjustmentResultDto

Source: Shared/Models/Dtos/Product/StockAdjustmentResultDto.cs

| JSON property | C# type |
|---|---|
| productId | int |
| storeId | int |
| productUnitId | int |
| batchId | long |
| batchNumber | string |
| expirationDate | DateTime? |
| oldBatchQty | int |
| newBatchQty | int |
| deltaQty | int |
| inventoryQtyOnHandAfter | int |

## PromotionActivateDto

Source: Shared/Models/Dtos/Promotion/PromotionActivateDto.cs

| JSON property | C# type |
|---|---|
| isActive | bool |

## PromotionAddProductsDto

Source: Shared/Models/Dtos/Promotion/PromotionAddProductsDto.cs

| JSON property | C# type |
|---|---|
| items | List<PromotionProductCreateItemDto> |

## PromotionProductCreateItemDto

Source: Shared/Models/Dtos/Promotion/PromotionAddProductsDto.cs

| JSON property | C# type |
|---|---|
| productId | int? |
| erpProductId | decimal |
| erpOfferId | decimal? |

## PromotionAddProductsResultDto

Source: Shared/Models/Dtos/Promotion/PromotionAddProductsResultDto.cs

| JSON property | C# type |
|---|---|
| promotionId | int |
| requestedCount | int |
| addedCount | int |
| restoredCount | int |
| skippedDuplicateCount | int |
| skippedErpProductIds | List<decimal> |

## PromotionCreateDto

Source: Shared/Models/Dtos/Promotion/PromotionCreateDto.cs

| JSON property | C# type |
|---|---|
| erpPgoId | decimal? |
| name | string? |
| notes | string? |
| startAt | DateTime? |
| endAt | DateTime? |
| totalAmount | int? |
| basicAmount | int? |
| offerAmount | int? |
| discountPercent | decimal? |
| isActive | bool? |

## PromotionDetailsDto

Source: Shared/Models/Dtos/Promotion/PromotionDetailsDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| erpPgoId | decimal? |
| name | string? |
| notes | string? |
| startAt | DateTime? |
| endAt | DateTime? |
| totalAmount | int? |
| basicAmount | int? |
| offerAmount | int? |
| discountPercent | decimal? |
| isActive | bool |
| productsCount | int |
| products | List<PromotionProductItemDto> |

## PromotionListItemDto

Source: Shared/Models/Dtos/Promotion/PromotionListItemDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| erpPgoId | decimal? |
| name | string? |
| isActive | bool |
| startAt | DateTime? |
| endAt | DateTime? |
| totalAmount | int? |
| basicAmount | int? |
| offerAmount | int? |
| discountPercent | decimal? |
| createdAt | DateTime |

## PromotionListQueryDto

Source: Shared/Models/Dtos/Promotion/PromotionListQueryDto.cs

| JSON property | C# type |
|---|---|
| q | string? |
| isActive | bool? |
| from | DateTime? |
| to | DateTime? |
| onlyRunningNow | bool? |
| sort | PromotionSortOption |
| page | int |
| pageSize | int |

## PromotionProductItemDto

Source: Shared/Models/Dtos/Promotion/PromotionProductItemDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| productId | int? |
| erpProductId | decimal |
| erpOfferId | decimal? |

## PromotionReplaceProductsDto

Source: Shared/Models/Dtos/Promotion/PromotionReplaceProductsDto.cs

| JSON property | C# type |
|---|---|
| items | List<PromotionProductReplaceItemDto> |

## PromotionProductReplaceItemDto

Source: Shared/Models/Dtos/Promotion/PromotionReplaceProductsDto.cs

| JSON property | C# type |
|---|---|
| productId | int? |
| erpProductId | decimal |
| erpOfferId | decimal? |

## PromotionReplaceProductsResultDto

Source: Shared/Models/Dtos/Promotion/PromotionReplaceProductsDto.cs

| JSON property | C# type |
|---|---|
| promotionId | int |
| requestedCount | int |
| finalActiveCount | int |
| insertedCount | int |
| restoredCount | int |
| softDeletedCount | int |
| unchangedCount | int |
| duplicatesInRequest | List<decimal> |

## PromotionProductRow

Source: Shared/Models/Dtos/Promotion/PromotionReplaceProductsDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| erpProductId | decimal |
| deletedAt | DateTime? |

## PromotionUpdateDto

Source: Shared/Models/Dtos/Promotion/PromotionUpdateDto.cs

| JSON property | C# type |
|---|---|
| erpPgoId | decimal? |
| name | string? |
| notes | string? |
| startAt | DateTime? |
| endAt | DateTime? |
| totalAmount | int? |
| basicAmount | int? |
| offerAmount | int? |
| discountPercent | decimal? |
| isActive | bool? |
| rowVersion | string? |

## PublicPromotionListItemDto

Source: Shared/Models/Dtos/Promotion/PublicPromotionDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| name | string? |
| startAt | DateTime? |
| endAt | DateTime? |
| totalAmount | int? |
| basicAmount | int? |
| offerAmount | int? |
| discountPercent | decimal? |

## PublicPromotionDetailsDto

Source: Shared/Models/Dtos/Promotion/PublicPromotionDtos.cs

| JSON property | C# type |
|---|---|
| productIds | List<int> |

## CancelOrderDto

Source: Shared/Models/Dtos/Returns/ReturnDtos.cs

| JSON property | C# type |
|---|---|
| reason | string |

## CreateReturnRequestDto

Source: Shared/Models/Dtos/Returns/ReturnDtos.cs

| JSON property | C# type |
|---|---|
| reason | string |
| items | List<CreateReturnRequestItemDto> |

## CreateReturnRequestItemDto

Source: Shared/Models/Dtos/Returns/ReturnDtos.cs

| JSON property | C# type |
|---|---|
| orderItemId | int |
| quantity | decimal |

## ReviewReturnRequestDto

Source: Shared/Models/Dtos/Returns/ReturnDtos.cs

| JSON property | C# type |
|---|---|
| approve | bool |
| notes | string? |

## ReturnRequestQueryDto

Source: Shared/Models/Dtos/Returns/ReturnDtos.cs

| JSON property | C# type |
|---|---|
| status | byte? |
| orderId | int? |
| customerId | int? |
| fromUtc | DateTime? |
| toUtc | DateTime? |
| page | int |
| pageSize | int |

## ReturnRequestItemDto

Source: Shared/Models/Dtos/Returns/ReturnDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| orderItemId | int |
| productId | int |
| productNameAr | string |
| unitLevel | byte |
| requestedQuantity | decimal |
| purchasedQuantity | decimal |
| finalUnitPrice | decimal |
| lineRefundValue | decimal |

## ReturnRequestDto

Source: Shared/Models/Dtos/Returns/ReturnDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| orderId | int |
| orderNumber | string |
| customerId | int |
| customerNameAr | string |
| status | byte |
| reason | string |
| requestedAt | DateTime |
| reviewedByEmployeeId | int? |
| reviewedByUserId | int? |
| reviewedAt | DateTime? |
| reviewNotes | string? |
| items | List<ReturnRequestItemDto> |

## ReturnRequestListItemDto

Source: Shared/Models/Dtos/Returns/ReturnDtos.cs

| JSON property | C# type |
|---|---|
| id | int |
| orderId | int |
| orderNumber | string |
| customerId | int |
| customerNameAr | string |
| status | byte |
| reason | string |
| requestedAt | DateTime |
| itemsCount | int |

## ReturnReviewResultDto

Source: Shared/Models/Dtos/Returns/ReturnDtos.cs

| JSON property | C# type |
|---|---|
| returnRequestId | int |
| status | byte |
| restockedOuterQuantity | decimal |
| redeemedPointsRefunded | int |
| earnedPointsReversed | int |

## TagCreateDto

Source: Shared/Models/Dtos/Tag/TagCreateDto.cs

| JSON property | C# type |
|---|---|
| nameAr | string? |
| nameEn | string? |
| isActive | bool? |

## TagListItemDto

Source: Shared/Models/Dtos/Tag/TagListItemDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| nameAr | string? |
| nameEn | string? |
| isActive | bool |
| createdAt | DateTime |

## TagQueryDto

Source: Shared/Models/Dtos/Tag/TagQueryDto.cs

| JSON property | C# type |
|---|---|
| q | string? |
| isActive | bool? |
| sort | string? |
| page | int |
| pageSize | int |

## ChangePasswordRequestDto

Source: Shared/Models/Auth/ChangePasswordRequestDto.cs

| JSON property | C# type |
|---|---|
| currentPassword | string |
| newPassword | string |

## ConfirmEmailRequestDto

Source: Shared/Models/Auth/ConfirmEmailRequestDto.cs

| JSON property | C# type |
|---|---|
| userId | int |
| token | string |

## ConfirmEmailResponseDto

Source: Shared/Models/Auth/ConfirmEmailResponseDto.cs

| JSON property | C# type |
|---|---|
| userId | int |
| emailConfirmed | bool |

## CurrentUserDto

Source: Shared/Models/Auth/CurrentUserDto.cs

| JSON property | C# type |
|---|---|
| userId | string |
| userName | string? |
| email | string? |
| pharmacyId | int? |
| roles | List<string> |
| permissions | List<string> |
| customer | CustomerProfileDto? |

## SetUserPermissionsRequestDto

Source: Shared/Models/Auth/CurrentUserDto.cs

| JSON property | C# type |
|---|---|
| permissions | List<string> |

## ForgotPasswordRequestDto

Source: Shared/Models/Auth/ForgotPasswordRequestDto.cs

| JSON property | C# type |
|---|---|
| email | string |

## LoginAuditEntry

Source: Shared/Models/Auth/LoginAuditEntry.cs

| JSON property | C# type |
|---|---|
| createdAtUtc | DateTime |
| outcome | string |
| failureReason | string? |
| userId | int? |
| identifierMasked | string? |
| identifierHash | byte[]? |
| ipAddress | string? |
| userAgent | string? |
| traceId | string? |
| pharmacyId | int? |
| latencyMs | int? |

## LoginRequestContext

Source: Shared/Models/Auth/LoginRequestContext.cs

| JSON property | C# type |
|---|---|
| ipAddress | string? |
| userAgent | string? |
| traceId | string? |
| pharmacyId | int |

## LoginRequestDto

Source: Shared/Models/Auth/LoginRequestDto.cs

| JSON property | C# type |
|---|---|
| identifier | string |
| password | string |
| rememberMe | bool |

## LoginResponseDto

Source: Shared/Models/Auth/LoginResponseDto.cs

| JSON property | C# type |
|---|---|
| userId | int |
| accessToken | string |
| expiresAtUtc | DateTime |

## ModifyUserRolesRequestDto

Source: Shared/Models/Auth/ModifyUserRolesRequestDto.cs

| JSON property | C# type |
|---|---|
| roles | List<string> |

## RegisterRequestDto

Source: Shared/Models/Auth/RegisterRequestDto.cs

| JSON property | C# type |
|---|---|
| email | string |
| userName | string? |
| password | string |

## RegisterResponseDto

Source: Shared/Models/Auth/RegisterResponseDto.cs

| JSON property | C# type |
|---|---|
| userId | int |
| email | string |
| emailConfirmationRequired | bool |

## ResendConfirmEmailRequestDto

Source: Shared/Models/Auth/ResendConfirmEmailRequestDto.cs

| JSON property | C# type |
|---|---|
| email | string |

## ResetPasswordRequestDto

Source: Shared/Models/Auth/ResetPasswordRequestDto.cs

| JSON property | C# type |
|---|---|
| userId | int |
| token | string |
| newPassword | string |

## RoleDto

Source: Shared/Models/Auth/RoleDto.cs

| JSON property | C# type |
|---|---|
| id | int |
| name | string |
| nameEn | string? |
| description | string? |

## SetUserRolesRequestDto

Source: Shared/Models/Auth/SetUserRolesRequestDto.cs

| JSON property | C# type |
|---|---|
| roles | List<string> |

## UserRolesResponseDto

Source: Shared/Models/Auth/UserRolesResponseDto.cs

| JSON property | C# type |
|---|---|
| userId | int |
| roles | List<string> |

## PrescriptionCreateFormData

Source: WebAPI/SpecificDtos/PrescriptionCreateFormData.cs

| JSON property | C# type |
|---|---|
| storeId | int |
| notes | string? |
| primaryIndex | int? |
| files | List<IFormFile> |

## ProductCreateWithImageDto

Source: WebAPI/SpecificDtos/ProductCreateWithImageDto.cs

| JSON property | C# type |
|---|---|
| image | IFormFile? |
| images | List<IFormFile>? |

