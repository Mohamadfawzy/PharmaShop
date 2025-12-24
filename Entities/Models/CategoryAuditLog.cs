using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("CategoryAuditLog", Schema = "audit")]
public class CategoryAuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ChangeType { get; set; } = null!; // Insert, Update, Activate, Deactivate, Delete

    [MaxLength(100)]
    public string? FieldName { get; set; } // Name, NameEn, DescriptionEn, IsActive, ParentCategoryId ...

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    [MaxLength(100)]
    public string? ChangedBy { get; set; } // UserId أو Username

    public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Reason { get; set; }

    // Navigation Property
    public Category Category { get; set; } = null!;
}
