# PharmaShop API — دليل المطورين

هذه الوثائق موجهة لمطوري تطبيق الهاتف ولوحة الإدارة، سواء كانت WPF أو Web Dashboard أو عميلًا آخر. تصف المسارات الموجودة حاليًا في المشروع.

## ملفات الدليل

- [المصادقة والأدوار وشكل الاستجابة](common.md)
- [مرجع حقول DTOs والـ Query models](request-models.md)
- [تطبيق الهاتف والعميل والموظف ومندوب التوصيل](mobile-client.md)
- [لوحة الإدارة والتشغيل](admin-dashboard.md)
- [Postman Collection الكاملة](../../postman/PharmaShop-Complete-API.postman_collection.json)
- [بيئة Postman المحلية](../../postman/PharmaShop-Local.postman_environment.json)

## كيف تقرأ المسارات؟

استبدل {id} وأشباهه بمعرّف فعلي، مثل /api/v1/products/12. اختر عنوان الخدمة من إعداد البيئة، مثل https://localhost:7001 في Postman المحلي. أرسل JSON بترميز UTF-8، واستخدم المعرفات التي ترجعها الاستجابات في الطلبات التالية.

## النطاق

تشمل هذه النسخة 191 Endpoint فعليًا استخرجت من Controllers الحالية. تكامل ERP مؤجل إلى حين تحديد بنية قاعدة بيانات ERP وعقد الوسيط. لا يوجد مزود دفع خارجي محدد حاليًا؛ واجهات محاولات الدفع والاسترداد تحفظ سجلات تشغيلية فقط.
