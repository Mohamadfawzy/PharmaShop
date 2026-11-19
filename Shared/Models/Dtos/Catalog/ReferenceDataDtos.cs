namespace Shared.Models.Dtos.Catalog;

public sealed record CompanyDto(int Id, string NameAr, string? NameEn, bool IsActive);
public sealed record CompanyWriteDto(string NameAr, string? NameEn, bool? IsActive = null);
public sealed record UnitDto(int Id, string Code, string NameAr, string NameEn, bool IsActive);
public sealed record UnitWriteDto(string Code, string NameAr, string NameEn, bool? IsActive = null);
