using Contracts.IServices;
using Contracts.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Identity;
using Service.Auth.Extensions;
using Shared.Constants;
using Shared.Models.Auth;
using Shared.Responses;

namespace Service.Auth;
public sealed class AuthService : IAuthService
{
    private const string DefaultCustomerRole = "Customer";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILoginAuditService _loginAuditService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtTokenService,
        ILoginAuditService loginAuditService,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _loginAuditService = loginAuditService;
        _emailSender = emailSender;
        this.configuration = configuration;
    }

    // -----------------------------------------------
    //  1 RegisterAsync
    // -----------------------------------------------
    public async Task<AppResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto, CancellationToken ct)
    {
        // Normalize
        var email = dto.Email.Trim().ToLowerInvariant();
        var userName = string.IsNullOrWhiteSpace(dto.UserName)
            ? email
            : dto.UserName.Trim();

        // 1 Check if email already exists (friendly error)
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            return AppResponse<RegisterResponseDto>.Conflict("Email is already registered");

        // 2 Ensure role exists (defensive)
        if (!await _roleManager.RoleExistsAsync(DefaultCustomerRole))
            return AppResponse<RegisterResponseDto>.InternalError("Default role not found. Seed roles first.");

        // 3 Create user
        var user = new ApplicationUser
        {
            Email = email,
            UserName = userName,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            //var errors = createResult.Errors.Select(e => e.Description).ToList();
            //return AppResponse<RegisterResponseDto>.ValidationErrors(errors);

            var fieldErrors = createResult.ToFieldErrors();

            return AppResponse<RegisterResponseDto>.ValidationErrors(
                fieldErrors,
                detail: "User registration validation failed"
            );
        }

        // 4 Add to Customer role
        var roleResult = await _userManager.AddToRoleAsync(user, DefaultCustomerRole);
        if (!roleResult.Succeeded)
        {
            // rollback user creation (optional but clean)
            await _userManager.DeleteAsync(user);

            var errors = roleResult.Errors.Select(e => e.Description).ToList();
            return AppResponse<RegisterResponseDto>.InternalError("Failed to assign role");
        }

        // 5 Generate email confirmation token + send email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // NOTE: token must be URL-encoded when sent as querystring
        var encodedToken = Uri.EscapeDataString(token);

        // في البداية: نرسل رابط API مباشر (لاحقًا سيكون رابط Frontend)
        //var confirmUrl = $"/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        var baseUrl = configuration["App:BaseUrl"];
        var confirmUrl = $"{baseUrl}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        await _emailSender.SendEmailAsync(
            toEmail: user.Email!,
            subject: "Confirm your email",
            htmlBody: $"Click to confirm: {confirmUrl}",
            ct: ct
        );

        // 6 Return
        var data = new RegisterResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            EmailConfirmationRequired = true
        };

        // Created
        return AppResponse<RegisterResponseDto>.Created(data);
    }

    // -----------------------------------------------
    //  2 ConfirmEmailAsync
    // -----------------------------------------------
    public async Task<AppResponse<ConfirmEmailResponseDto>> ConfirmEmailAsync(ConfirmEmailRequestDto dto, CancellationToken ct)
    {
        // 1) Find user
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is null)
            return AppResponse<ConfirmEmailResponseDto>.NotFound("User not found");

        // 2) Already confirmed? (idempotent)
        if (user.EmailConfirmed)
        {
            return AppResponse<ConfirmEmailResponseDto>.Ok(new ConfirmEmailResponseDto
            {
                UserId = user.Id,
                EmailConfirmed = true
            }, "Email already confirmed");
        }

        // 3) Token decode (important!)
        // لو التوكن جاء من querystring أو رابط ايميل سيكون URL-encoded غالباً
        // إذا جاء في body كما هو من client، Decode لن يضر
        var token = dto.Token?.Trim() ?? string.Empty;
        token = Uri.UnescapeDataString(token);

        if (string.IsNullOrWhiteSpace(token))
        {
            // نرجع ValidationErrors بنفس شكل AppResponse
            var fe = new Dictionary<string, string[]>
            {
                ["Token"] = new[] { "Token is required" }
            };
            return AppResponse<ConfirmEmailResponseDto>.ValidationErrors(fe, "Validation failed");
        }

        // 4) Confirm
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            // تحويل IdentityResult إلى FieldErrors
            var fieldErrors = result.ToFieldErrors();

            // ملاحظة: أخطاء تأكيد الإيميل غالباً تكون Token invalid/expired
            return AppResponse<ConfirmEmailResponseDto>.ValidationErrors(
                fieldErrors,
                detail: "Email confirmation failed"
            );
        }

        // 5) Success
        return AppResponse<ConfirmEmailResponseDto>.Ok(new ConfirmEmailResponseDto
        {
            UserId = user.Id,
            EmailConfirmed = true
        }, "Email confirmed successfully");
    }

    // -----------------------------------------------
    //  3 LoginAsync
    // -----------------------------------------------
    public async Task<AppResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto,LoginRequestContext ctx,CancellationToken ct)
    {
        // Normalize
        var identifier = dto.Identifier?.Trim() ?? string.Empty;

        // ===== [AUDIT - NEW] Tracking variables =====
        var auditUserId = (int?)null;
        var auditSuccess = false;
        string? auditReason = null;
        // ===== [/AUDIT - NEW] ======================

        if (string.IsNullOrWhiteSpace(identifier))
        {
            // ===== [AUDIT - NEW] =====
            auditReason = LoginFailReasons.ValidationError;
            await AuditLoginAsync(identifier, auditUserId, auditSuccess, auditReason, ctx, ct);
            // ===== [/AUDIT - NEW] ====

            return AppResponse<LoginResponseDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Identifier"] = new[] { "Identifier is required" }
                },
                detail: "Validation failed"
            );
        }

        // 1) Find user by Email OR Username (بدون user enumeration في الرسالة النهائية)
        // ApplicationUser? user = await FindUserByIdentifierAsync(identifier, ct);

        ApplicationUser? user = null;

        if (identifier.Contains('@'))
        {
            var normalizedEmail = identifier.ToLowerInvariant();
            user = await _userManager.FindByEmailAsync(normalizedEmail);
        }
        else
        {
            user = await _userManager.FindByNameAsync(identifier);
        }

        // رسالة موحدة لتفادي كشف هل المستخدم موجود أم لا (Security best practice)
        static AppResponse<LoginResponseDto> InvalidCredentials() =>
            AppResponse<LoginResponseDto>.Unauthorized("Invalid credentials");

        if (user is null)
        {
            // ===== [AUDIT - NEW] =====
            auditReason = LoginFailReasons.InvalidCredentials;
            await AuditLoginAsync(identifier, auditUserId, auditSuccess, auditReason, ctx, ct);
            // ===== [/AUDIT - NEW] ====

            return InvalidCredentials();
        }

        // ===== [AUDIT - NEW] capture userId once we know the user =====
        auditUserId = user.Id;
        // ===== [/AUDIT - NEW] =======================================

        // 2) Business flags (مثال: IsActive)
        // IMPORTANT: لو عندك IsActive في ApplicationUser
        if (user is { IsActive: false })
        {
            // ===== [AUDIT - NEW] =====
            auditReason = LoginFailReasons.UserDisabled;
            await AuditLoginAsync(identifier, auditUserId, auditSuccess, auditReason, ctx, ct);
            // ===== [/AUDIT - NEW] ====

            return AppResponse<LoginResponseDto>.Forbidden("User account is disabled");
        }

        // 3) Email confirmation policy
        // بما أنك فعلت options.SignIn.RequireConfirmedEmail = true
        // ممكن تعتمد على SignInManager ويفشل تلقائياً، لكن الأفضل رسالة واضحة:
        if (!user.EmailConfirmed)
        {
            // ===== [AUDIT - NEW] =====
            auditReason = LoginFailReasons.EmailNotConfirmed;
            await AuditLoginAsync(identifier, auditUserId, auditSuccess, auditReason, ctx, ct);
            // ===== [/AUDIT - NEW] ====

            return AppResponse<LoginResponseDto>.Forbidden("Email is not confirmed");
        }

        // 4) Lockout check (Real-world)
        if (await _userManager.IsLockedOutAsync(user))
        {
            // ===== [AUDIT - NEW] =====
            auditReason = LoginFailReasons.LockedOut;
            await AuditLoginAsync(identifier, auditUserId, auditSuccess, auditReason, ctx, ct);
            // ===== [/AUDIT - NEW] ====

            return AppResponse<LoginResponseDto>.Forbidden("Account is locked. Try again later.");
        }

        // 5) Verify password (مع دعم lockoutOnFailure)
        // SignInManager أفضل من CheckPasswordAsync لأنه يطبّق lockout counters.
        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            dto.Password,
            lockoutOnFailure: true
        );

        if (signInResult.IsLockedOut)
        {
            // ===== [AUDIT - NEW] =====
            auditReason = LoginFailReasons.LockedOut;
            await AuditLoginAsync(identifier, auditUserId, auditSuccess, auditReason, ctx, ct);
            // ===== [/AUDIT - NEW] ====

            return AppResponse<LoginResponseDto>.Forbidden("Account is locked. Try again later.");
        }

        if (!signInResult.Succeeded)
        {
            // ===== [AUDIT - NEW] =====
            auditReason = LoginFailReasons.InvalidCredentials;
            await AuditLoginAsync(identifier, auditUserId, auditSuccess, auditReason, ctx, ct);
            // ===== [/AUDIT - NEW] ====

            return InvalidCredentials();
        }

        // 6) (اختياري) Update last login (لو عندك أعمدة أو جدول Audit)
        // Future: سجل LoginEvent + IP + UserAgent + DeviceId

        // 7) Issue JWT
        // pharmacy_id: الآن ثابت 1، لاحقاً تُستخرج من Admin/Customer profile أو Tenant middleware
        var accessToken = await _jwtTokenService.CreateAccessTokenAsync(user, pharmacyId: 1);

        // JWT expiry (نحسبها من config بدل ما نفك JWT)
        var minutes = int.Parse(configuration.GetSection("Jwt")["AccessTokenMinutes"]!);
        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        // ===== [AUDIT - NEW] success audit just before returning =====
        auditSuccess = true;
        await AuditLoginAsync(identifier, auditUserId, auditSuccess, failureReason: null, ctx, ct);
        // ===== [/AUDIT - NEW] =======================================

        // 8) Return
        return AppResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            UserId = user.Id,
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAt
        }, "Login successful");
    }


    public async Task<AppResponse> ResendConfirmEmailAsync(ResendConfirmEmailRequestDto dto, CancellationToken ct)
    {
        // Normalize
        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();

        // رسالة موحدة لمنع user enumeration
        static AppResponse GenericOk() =>
            AppResponse.Ok("If the email exists, a confirmation message has been sent.");

        if (string.IsNullOrWhiteSpace(email))
        {
            return AppResponse.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Email"] = new[] { "Email is required" }
                },
                detail: "Validation failed"
            );
        }

        // 1) Find user by email
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return GenericOk();

        // 2) Already confirmed? return generic ok (idempotent + no enumeration)
        if (user.EmailConfirmed)
            return GenericOk();

        // 3) Generate token + send
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);

        var baseUrl = configuration["App:BaseUrl"];
        var confirmUrl = $"{baseUrl}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        await _emailSender.SendEmailAsync(
            toEmail: user.Email!,
            subject: "Confirm your email",
            htmlBody: $"Click to confirm: {confirmUrl}",
            ct: ct
        );

        return GenericOk();
    }

    public async Task<AppResponse> ForgotPasswordAsync(ForgotPasswordRequestDto dto, CancellationToken ct)
    {
        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();

        static AppResponse GenericOk() =>
            AppResponse.Ok("If the email exists, a password reset message has been sent.");

        if (string.IsNullOrWhiteSpace(email))
        {
            return AppResponse.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Email"] = new[] { "Email is required" }
                },
                detail: "Validation failed"
            );
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return GenericOk();

        // اختياري: لو تحب تمنع reset قبل تأكيد الإيميل (سياسة شائعة)
        if (!user.EmailConfirmed)
            return GenericOk();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);

        var baseUrl = configuration["App:BaseUrl"];
        // في البداية: رابط API أو رابط Frontend لاحقاً
        // مثال Frontend: /reset-password?userId=...&token=...
        var resetUrl = $"{baseUrl}/auth/reset-password?userId={user.Id}&token={encodedToken}";

        await _emailSender.SendEmailAsync(
            toEmail: user.Email!,
            subject: "Reset your password",
            htmlBody: $"Click to reset your password: {resetUrl}",
            ct: ct
        );

        return GenericOk();
    }


    public async Task<AppResponse> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct)
    {
        // Basic validation (AppResponse style)
        var fieldErrors = new Dictionary<string, string[]>();

        if (dto.UserId <= 0)
            fieldErrors["UserId"] = new[] { "UserId is required" };

        var token = (dto.Token ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(token))
            fieldErrors["Token"] = new[] { "Token is required" };

        var newPassword = dto.NewPassword ?? string.Empty;
        if (string.IsNullOrWhiteSpace(newPassword))
            fieldErrors["NewPassword"] = new[] { "NewPassword is required" };

        if (fieldErrors.Count > 0)
            return AppResponse.ValidationErrors(fieldErrors, detail: "Validation failed");

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is null)
            return AppResponse.NotFound("User not found");

        // decode token (important when passed via querystring/email)
        token = Uri.UnescapeDataString(token);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            // نفس أسلوبك الحالي: IdentityResult -> FieldErrors
            var identityErrors = result.ToFieldErrors();

            // غالباً errors ستكون "Invalid token" أو password policy
            return AppResponse.ValidationErrors(identityErrors, detail: "Password reset failed");
        }

        // (اختياري مستقبلاً)
        // - revoke refresh tokens (logout all sessions)
        // - add security audit "PasswordReset"
        // - update SecurityStamp (Identity يفعلها غالباً عند تغيير كلمة المرور)

        return AppResponse.Ok("Password has been reset successfully");
    }


    public async Task<AppResponse> ChangePasswordAsync(string userId, ChangePasswordRequestDto dto, CancellationToken ct)
    {
        // Basic validation (AppResponse style)
        var fieldErrors = new Dictionary<string, string[]>();

        var current = (dto.CurrentPassword ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(current))
            fieldErrors["CurrentPassword"] = new[] { "CurrentPassword is required" };

        var next = (dto.NewPassword ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(next))
            fieldErrors["NewPassword"] = new[] { "NewPassword is required" };

        if (fieldErrors.Count > 0)
            return AppResponse.ValidationErrors(fieldErrors, detail: "Validation failed");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return AppResponse.Unauthorized("Invalid user");

        if (user is { IsActive: false })
            return AppResponse.Forbidden("User account is disabled");

        // Important: ChangePassword applies current password verification + updates password hash + updates security stamp
        var result = await _userManager.ChangePasswordAsync(user, current, next);

        if (!result.Succeeded)
        {
            // تحويل IdentityResult إلى FieldErrors بنفس أسلوبك
            var identityErrors = result.ToFieldErrors();

            // غالبًا: current password wrong OR password policy errors
            return AppResponse.ValidationErrors(identityErrors, detail: "Change password failed");
        }

        // (Real-world) Optional future:
        // - revoke refresh tokens (logout other sessions)
        // - add security audit "PasswordChanged"

        return AppResponse.Ok("Password changed successfully");
    }


    // ====================
    private Task AuditLoginAsync(string identifier,int? userId,bool success,string? failureReason,LoginRequestContext ctx,CancellationToken ct)
    {
        // مهم: لا نسمح أن الـaudit يكسر الـlogin
        try
        {
            return _loginAuditService.WriteAsync(new LoginAuditEntry
            {
                Outcome = success ? "Success" : "Failure",
                FailureReason = success ? null : failureReason,

                UserId = userId,
                IdentifierMasked = LoginAuditHelpers.MaskIdentifier(identifier),
                IdentifierHash = LoginAuditHelpers.Sha256(identifier),

                IpAddress = ctx.IpAddress,
                UserAgent = ctx.UserAgent,
                TraceId = ctx.TraceId,
                PharmacyId = ctx.PharmacyId
            }, ct);
        }
        catch
        {
            return Task.CompletedTask;
        }
    }

    private async Task<ApplicationUser?> FindUserByIdentifierAsync(string identifier, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return null;

        identifier = identifier.Trim();

        // Email
        if (identifier.Contains('@'))
            return await _userManager.FindByEmailAsync(identifier.ToLowerInvariant());

        // Phone (simple heuristic)
        if (LooksLikePhone(identifier))
        {
            var phone = NormalizePhone(identifier);
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phone, ct);
        }

        // Username
        return await _userManager.FindByNameAsync(identifier);
    }

    private static bool LooksLikePhone(string s)
    {
        // بسيط ومبدئي: أرقام مع + (ممكن تحسنها لاحقًا بـ lib)
        s = s.Replace(" ", "").Replace("-", "");
        return s.All(ch => char.IsDigit(ch) || ch == '+') && s.Count(char.IsDigit) >= 8;
    }

    private static string NormalizePhone(string s)
    {
        // مبدئي: remove spaces/dashes، لاحقًا اعتمد E.164
        return s.Replace(" ", "").Replace("-", "");
    }

}