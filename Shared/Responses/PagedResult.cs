using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Responses;

public sealed class PagedResult<T>
{
    public required List<T> Items { get; init; }
    public required int TotalCount { get; init; }
}