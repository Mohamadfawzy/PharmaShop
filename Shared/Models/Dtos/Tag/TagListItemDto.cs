using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Tag;

public sealed class TagListItemDto
{
    public int Id { get; set; }
    public string? NameAr { get; set; } 
    public string? NameEn { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}