using Shared.Models.Dtos.Tag;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices;

public interface ITagService
{
    Task<AppResponse<int>> CreateTagAsync(TagCreateDto dto, CancellationToken ct);
    Task<AppResponse<List<TagListItemDto>>> GetTagsAsync(TagQueryDto query, CancellationToken ct);
}
