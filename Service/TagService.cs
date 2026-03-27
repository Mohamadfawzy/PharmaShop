namespace Service;
using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Models.Dtos.Tag;
using Shared.Responses;
using System.Linq.Expressions;

public class TagService : ITagService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IValidator<TagCreateDto> _validator;
    private readonly ILogger<TagService> logger;

    public TagService(IUnitOfWork unitOfWork, IValidator<TagCreateDto> validator, ILogger<TagService> logger)
    {
        this.unitOfWork = unitOfWork;
        _validator = validator;
        this.logger = logger;
    }

    public async Task<AppResponse<int>> CreateTagAsync(TagCreateDto dto, CancellationToken ct)
    {
        // 1) Validate DTO
        //var vr = await _validator.ValidateAsync(dto, ct);
        //if (!vr.IsValid)
        //{
        //    var fieldErrors = vr.Errors
        //        .GroupBy(e => e.PropertyName)
        //        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        //    return AppResponse<int>.ValidationErrors(fieldErrors, detail: "Validation failed");
        //}

        // Normalize
        var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();
        var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();

        // 2) Uniqueness check
        var term = nameAr?.Trim();

        //var exists = await unitOfWork.Tags.AnyAsync(t =>
        //    t.NameAr == term
        //    || (t.NameEn != null && t.NameEn == term),
        //    ct);
        
        //if (exists)
        //{
        //    return AppResponse<int>.ValidationErrors(
        //        new Dictionary<string, string[]>
        //        {
        //            ["Name"] = new[] { "Tag name already exists" }
        //        },
        //        detail: "Validation failed"
        //    );
        //}

        // 3) Create entity
        var tag = new Tag
        {
            NameAr = nameAr,
            NameEn = nameEn,
            IsActive = dto.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,

            // ⚠️ إن كانت DB ما زالت تحتوي PharmacyId NOT NULL ولم تُعدّل:
            // PharmacyId = 1  // مؤقت فقط لحين عمل Migration (غير مُفضل)
        };

        // 4) Save
        await unitOfWork.Tags.AddAsync(tag, ct);
        await unitOfWork.CompleteAsync(ct);

        // 5) Return Created
        var location = $"/api/v1/tags/{tag.Id}";
        return AppResponse<int>.Created(tag.Id, location, title: "Tag created successfully");
    }

    public async Task<AppResponse<List<TagListItemDto>>> GetTagsAsync(TagQueryDto query, CancellationToken ct)
    {
        // 1) Normalize pagination
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 50 : query.PageSize;

        // حماية من PageSize الكبير (اختياري)
        if (pageSize > 200) pageSize = 200;

        var skip = (page - 1) * pageSize;

        // 2) Normalize search term
        var term = string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim();

        // 3) Build criteria
        Expression<Func<Tag, bool>>? criteria = null;

        // (A) Filter IsActive
        if (query.IsActive.HasValue)
        {
            criteria = t => t.IsActive == query.IsActive.Value;
        }

        // (B) Search in Name + NameEn (Arabic or English)
        if (!string.IsNullOrWhiteSpace(term))
        {
            Expression<Func<Tag, bool>> searchExpr =
                t => t.NameAr.Contains(term) || (t.NameEn != null && t.NameEn.Contains(term));

            if (criteria is null)
                criteria = searchExpr;
            else
                criteria = CombineAnd(criteria, searchExpr);
        }

        // 4) Sorting
        Func<IQueryable<Tag>, IOrderedQueryable<Tag>>? orderBy = query.Sort?.ToLowerInvariant() switch
        {
            "name:desc" => q => q.OrderByDescending(x => x.NameAr),
            "newest" => q => q.OrderByDescending(x => x.CreatedAt),
            "oldest" => q => q.OrderBy(x => x.CreatedAt),
            _ => q => q.OrderBy(x => x.NameAr) // default: name:asc
        };

        // 5) Total count
        var totalCount = await unitOfWork.Tags.CountAsync(criteria, ct);

        // 6) Get paged list (Projection)
        var items = await unitOfWork.Tags.GetAllAsync(
            selector: t => new TagListItemDto
            {
                Id = t.Id,
                NameAr = t.NameAr,
                NameEn = t.NameEn,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            },
            criteria: criteria,
            orderBy: orderBy,
            skip: skip,
            take: pageSize,
            asNoTracking: true,
            ct: ct
        );

        // 7) PaginationInfo (حسب كلاس PaginationInfo عندك)
        var pagination = PaginationInfo.Create(page, pageSize, totalCount);
        return AppResponse<List<TagListItemDto>>.Ok(items, pagination, title: "Tags retrieved successfully");
    }


    // Helper لدمج expressions بـ AND (لأن Expression<Func<T,bool>> لا يتجمع بسهولة)
    private static Expression<Func<T, bool>> CombineAnd<T>(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        var param = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(first, param),
            Expression.Invoke(second, param)
        );
        return Expression.Lambda<Func<T, bool>>(body, param);
    }





}