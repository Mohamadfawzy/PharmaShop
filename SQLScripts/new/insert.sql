
use pharma_shope_db;
go

INSERT INTO dbo.Companies (PharmacyId, NameAr, NameEn)
VALUES (1, N'جلَكسو سميث كلاين', N'GlaxoSmithKline');
GO


go
INSERT INTO Pharmacies(Name,NameEn,OwnerName,LicenseNumber,PhoneNumber,Email,Address,Latitude,Longitude)
VALUES
(
    N'صيدلية الشفاء', N'Al Shifa Pharmacy',
	N'محمد أحمد',N'PH-2024-001',
	'01012345678','info@alshifa.com',
	N'القاهرة - مدينة نصر - شارع مصطفى النحاس',
    30.0566100,31.3304300
);
