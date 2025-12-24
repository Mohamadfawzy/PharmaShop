using Contracts.IServices;
using Contracts.Notifications;
using Microsoft.AspNetCore.Identity;
using Repository.Identity;
using Service.Auth.Extensions;
using Shared.Models.Auth;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Auth;

public sealed class AuthService : IAuthService
{
    private const string DefaultCustomerRole = "Customer";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEmailSender _emailSender;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
    }

    public async Task<AppResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto, CancellationToken ct)
    {
        // Normalize
        var email = dto.Email.Trim().ToLowerInvariant();
        var userName = string.IsNullOrWhiteSpace(dto.UserName)
            ? email
            : dto.UserName.Trim();

        // 1) Check if email already exists (friendly error)
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            return AppResponse<RegisterResponseDto>.Conflict("Email is already registered");

        // 2) Ensure role exists (defensive)
        if (!await _roleManager.RoleExistsAsync(DefaultCustomerRole))
            return AppResponse<RegisterResponseDto>.InternalError("Default role not found. Seed roles first.");

        // 3) Create user
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

        // 4) Add to Customer role
        var roleResult = await _userManager.AddToRoleAsync(user, DefaultCustomerRole);
        if (!roleResult.Succeeded)
        {
            // rollback user creation (optional but clean)
            await _userManager.DeleteAsync(user);

            var errors = roleResult.Errors.Select(e => e.Description).ToList();
            return AppResponse<RegisterResponseDto>.InternalError("Failed to assign role");
        }

        // 5) Generate email confirmation token + send email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // NOTE: token must be URL-encoded when sent as querystring
        var encodedToken = Uri.EscapeDataString(token);

        // في البداية: نرسل رابط API مباشر (لاحقًا سيكون رابط Frontend)
        var confirmUrl = $"/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        await _emailSender.SendEmailAsync(
            toEmail: user.Email!,
            subject: "Confirm your email",
            htmlBody: $"Click to confirm: {confirmUrl}",
            ct: ct
        );

        // 6) Return
        var data = new RegisterResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            EmailConfirmationRequired = true
        };

        // الأفضل: Created
        return AppResponse<RegisterResponseDto>.Created(data);
    }
}