using Shared.Enums;

namespace Shared.Constants;

public static class ResponseDefaults
{
    public const int SuccessStatusCode = 200;
    public const int CreatedStatusCode = 201;
    public const int NoContentStatusCode = 204;
    public const int BadRequestStatusCode = 400;
    public const int UnauthorizedStatusCode = 401;
    public const int ForbiddenStatusCode = 403;
    public const int NotFoundStatusCode = 404;
    public const int ConflictStatusCode = 409;
    public const int InternalServerErrorStatusCode = 500;

    public const string DefaultSuccessTitle = "Operation Successful";
    public const string DefaultFailureTitle = "Operation Failed";
    public const string ValidationFailureTitle = "Validation Failed";
    public const string NotFoundTitle = "Resource Not Found";
    public const string UnauthorizedTitle = "Unauthorized Access";
    public const string ForbiddenTitle = "Access Forbidden";
    public const string ConflictTitle = "Conflict Detected";
    public const string InternalErrorTitle = "Internal Server Error";



    // ===== UTILITY METHODS =====

    public static int GetDefaultStatusCode(AppErrorCode errorCode)
    {
        return errorCode switch
        {
            AppErrorCode.ValidationError => ResponseDefaults.BadRequestStatusCode,
            AppErrorCode.BusinessRuleViolation => ResponseDefaults.BadRequestStatusCode,
            AppErrorCode.NotFound => ResponseDefaults.NotFoundStatusCode,
            AppErrorCode.Unauthorized => ResponseDefaults.UnauthorizedStatusCode,
            AppErrorCode.Forbidden => ResponseDefaults.ForbiddenStatusCode,
            AppErrorCode.Conflict => ResponseDefaults.ConflictStatusCode,
            AppErrorCode.InternalServerError => ResponseDefaults.InternalServerErrorStatusCode,
            AppErrorCode.ExternalServiceError => ResponseDefaults.InternalServerErrorStatusCode,
            AppErrorCode.DatabaseError => ResponseDefaults.InternalServerErrorStatusCode,
            _ => ResponseDefaults.BadRequestStatusCode
        };
    }

    public static string GetDefaultTitle(AppErrorCode errorCode)
    {
        return errorCode switch
        {
            AppErrorCode.ValidationError => ResponseDefaults.ValidationFailureTitle,
            AppErrorCode.NotFound => ResponseDefaults.NotFoundTitle,
            AppErrorCode.Unauthorized => ResponseDefaults.UnauthorizedTitle,
            AppErrorCode.Forbidden => ResponseDefaults.ForbiddenTitle,
            AppErrorCode.Conflict => ResponseDefaults.ConflictTitle,
            AppErrorCode.InternalServerError => ResponseDefaults.InternalErrorTitle,
            AppErrorCode.ExternalServiceError => ResponseDefaults.InternalErrorTitle,
            AppErrorCode.DatabaseError => ResponseDefaults.InternalErrorTitle,
            _ => ResponseDefaults.DefaultFailureTitle
        };
    }

    //private static readonly Dictionary<AppErrorCode, int> ErrorStatusCodes = new()
    //{
    //    { AppErrorCode.None, BadRequestStatusCode },
    //    { AppErrorCode.ValidationError, BadRequestStatusCode },
    //    { AppErrorCode.Unauthorized, UnauthorizedStatusCode },
    //    { AppErrorCode.Forbidden, ForbiddenStatusCode },
    //    { AppErrorCode.NotFound, NotFoundStatusCode },
    //    { AppErrorCode.Conflict, ConflictStatusCode },
    //    { AppErrorCode.BusinessRuleViolation, BadRequestStatusCode },
    //    { AppErrorCode.InternalServerError, InternalServerErrorStatusCode }
    //};

    //public static int GetStatusCode(AppErrorCode errorCode)
    //{
    //    return ErrorStatusCodes.TryGetValue(errorCode, out var code)
    //        ? code
    //        : InternalServerErrorStatusCode; // fallback
    //}
}
