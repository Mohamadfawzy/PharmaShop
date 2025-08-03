namespace Shared.Enums;

public enum AppErrorCode
{
    None = 0,

    // 1xx - Validation & Input
    ValidationFailed = 100,
    RequiredFieldMissing = 101,
    InvalidFormat = 102,
    ExceedsMaxLength = 103,
    BelowMinLength = 104,
    InvalidRange = 105,
    InvalidEnumValue = 106,

    // 2xx - Authentication & Authorization
    Unauthorized = 200,
    Forbidden = 201,
    TokenExpired = 202,
    InvalidCredentials = 203,
    AccountLocked = 204,
    AccountNotConfirmed = 205,
    AccessDenied = 206,

    // 3xx - Not Found / Existence
    NotFound = 300,
    EntityNotFound = 301,
    ResourceNotFound = 302,
    UserNotFound = 303,
    PageNotFound = 304,

    // 4xx - Conflict / Already Exists
    Conflict = 400,
    DuplicateRecord = 401,
    AlreadyExists = 402,
    EmailAlreadyExists = 403,
    UsernameAlreadyExists = 404,
    PhoneAlreadyExists = 405,
    OperationConflict = 406,

    // 5xx - Business Logic
    OperationNotAllowed = 500,
    InvalidOperation = 501,
    PrerequisiteNotMet = 502,
    BusinessRuleViolated = 503,
    OrderAlreadyCompleted = 504,
    SubscriptionExpired = 505,

    // 6xx - Database & Storage
    DatabaseError = 600,
    DbUpdateFailed = 601,
    ConstraintViolation = 602,
    RecordLocked = 603,

    // 7xx - External Services
    ExternalServiceUnavailable = 700,
    ExternalServiceTimeout = 701,
    ExternalServiceError = 702,
    PaymentGatewayError = 703,
    SmsProviderError = 704,
    EmailProviderError = 705,

    // 8xx - Network & System
    NetworkError = 800,
    Timeout = 801,
    ServiceUnavailable = 802,
    TooManyRequests = 803,

    // 9xx - Unexpected / Internal
    InternalServerError = 900,
    UnknownError = 901,
    UnhandledException = 902,
    NotImplemented = 903,
    SerializationError = 904
}
