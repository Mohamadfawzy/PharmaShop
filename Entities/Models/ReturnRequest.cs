namespace Entities.Models;

public class ReturnRequest
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public byte Status { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public int? ReviewedByEmployeeId { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
    public virtual Employee? ReviewedByEmployee { get; set; }
    public virtual ICollection<ReturnRequestItem> Items { get; set; } = new List<ReturnRequestItem>();
}
