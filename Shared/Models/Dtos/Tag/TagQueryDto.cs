using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Tag;

public sealed class TagQueryDto
{
    public string? Q { get; set; }           
    public bool? IsActive { get; set; }     // null = all ، true = active ، false = not acitve

    public string? Sort { get; set; }       // "name:asc" | "name:desc" | "newest" | "oldest"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}