
use pharma_shope_db;
GO

USE [pharma_shope_db]
GO

INSERT INTO [dbo].[Companies] ([NameAr],[NameEn],[IsActive])
     VALUES('باراك فيل','bark fell',1)
GO

INSERT INTO Pharmacies(Name,NameEn,OwnerName,LicenseNumber,PhoneNumber,Email,Address,Latitude,Longitude)
VALUES
(
    N'صيدلية الشفاء', N'Al Shifa Pharmacy',
	N'محمد أحمد',N'PH-2024-001',
	'01012345678','info@alshifa.com',
	N'القاهرة - مدينة نصر - شارع مصطفى النحاس',
    30.0566100,31.3304300
);
