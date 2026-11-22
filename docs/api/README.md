# PharmaShop API — دليل المطورين

هذه الوثائق موجهة لمطوري تطبيق الهاتف ولوحة الإدارة، سواء كانت WPF أو Web Dashboard أو عميلًا آخر. تصف المسارات الموجودة حاليًا في المشروع.

## ملفات الدليل

- [وحدة البنرات الترويجية وتكامل واجهة الهاتف](promotional-banners.md)

- [جرد الوحدات وجميع المسارات (199 عملية)](API_ENDPOINTS_OVERVIEW.md)
- [Postman Collection الخاصة بالبنرات](../../postman/promotional-banners/PharmaShop-Promotional-Banners.postman_collection.json)
- [تحليل التنفيذ وControllers/Services](API_ENDPOINTS_DETAILS.md)
- [أمثلة Requests وResponses](API_REQUEST_RESPONSE_EXAMPLES.md)
- [مرجع خصائص DTOs والأنواع المتداخلة](API_RESPONSE_MODELS.md)
- [المصادقة والأدوار وJWT](API_AUTHENTICATION.md)
- [جميع Enums والقيم الرقمية](API_ENUMS.md)
- [استجابات الأخطاء وStatus Codes](API_ERRORS.md)
- [مشكلات التكامل ومراجعة التنفيذ](API_INTEGRATION_ISSUES.md)
- [العقد المختصر لتطبيق .NET MAUI](mobile-api-contract.md)
- [المصادقة والأدوار وشكل الاستجابة](common.md)
- [مرجع حقول DTOs والـ Query models](request-models.md)
- [تطبيق الهاتف والعميل والموظف ومندوب التوصيل](mobile-client.md)
- [لوحة الإدارة والتشغيل](admin-dashboard.md)
- [Postman Collection الكاملة](../../postman/PharmaShop-Complete-API.postman_collection.json)
- [بيئة Postman المحلية](../../postman/PharmaShop-Local.postman_environment.json)

## كيف تقرأ المسارات؟

استبدل {id} وأشباهه بمعرّف فعلي، مثل /api/v1/products/12. اختر عنوان الخدمة من إعداد البيئة، مثل https://localhost:7001 في Postman المحلي. أرسل JSON بترميز UTF-8، واستخدم المعرفات التي ترجعها الاستجابات في الطلبات التالية.

## النطاق

تشمل هذه النسخة 199 Action فعالة ذات HTTP attributes في Controllers الحالية. خمس عمليات صور تطويرية مستبعدة من ApiExplorer عمدًا، بينما توجد مقتطفات قديمة داخل التعليقات لا تُعد Routes. تفصيل الفروق والملاحظات في API_ENDPOINTS_OVERVIEW.md وAPI_INTEGRATION_ISSUES.md. تكامل ERP مؤجل إلى حين تحديد بنية قاعدة بيانات ERP وعقد الوسيط. لا يوجد مزود دفع خارجي محدد حاليًا؛ واجهات محاولات الدفع والاسترداد تحفظ سجلات تشغيلية فقط.
