using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;



public sealed class OpenBoxDto
{
    public int StoreId { get; init; }

    // تحويل من وحدة أب إلى وحدة ابن (مثال: Carton -> Box, Box -> Strip)
    public int FromProductUnitId { get; init; }   // Parent product-unit (ProductUnits.Id)
    public int ToProductUnitId { get; init; }     // Child product-unit  (ProductUnits.Id)

    // عدد وحدات الأب التي سيتم فتحها/تحويلها
    public int FromQtyToConvert { get; init; } = 1;

    // اختيار batch محدد للتحويل منه (اختياري). لو null => FEFO
    public long? FromBatchId { get; init; }

    // سبب (اختياري) - سنستخدمه لاحقاً في الـ Audit/Ledger
    public string? Reason { get; init; }
}