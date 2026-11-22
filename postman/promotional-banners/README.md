# Postman — Promotional Banners

## الإعداد

1. افتح `PharmaShop-Promotional-Banners.postman_collection.json` في Postman.
2. عدّل متغير المجموعة `baseUrl` إلى عنوان API لديك.
3. سجّل دخول Admin في collection الرئيسية أو استخدم `/api/auth/login`، ثم ضع access token في `adminToken`.
4. في request إنشاء البنر اختر صورة من جهازك في حقل form-data `image`.
5. غيّر Target IDs إلى سجلات موجودة ونشطة. يتم حفظ `bannerId` تلقائيًا من استجابة الإنشاء.

## الطلبات

- Public: بنرات Home وجميع المواضع.
- Admin: البحث والتفاصيل والإنشاء والتعديل والتفعيل والتعطيل وإعادة الترتيب والحذف المنطقي.
- كل admin request تستخدم `Authorization: Bearer {{adminToken}}`.
- ملف الصورة لا يمكن أن يشير إلى مسار ثابت عبر collection؛ اختره من جهاز Postman عند تشغيل إنشاء/تحديث البنر.
- `image` في تحديث البنر معطّل افتراضيًا؛ فعّله واختر ملفًا عند استبدال الصورة.

انظر [عقد واجهة البنرات](../../docs/api/promotional-banners.md) لحقول form-data وقواعد ActionType ونماذج الردود.
