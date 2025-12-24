using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices;


public interface ICurrentUserService
{
    /// <summary>
    /// User Id from JWT (sub / nameidentifier)
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Username or email (if available)
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// User email (optional)
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// All role names assigned to the user
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// True if the request is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Check if user has a specific role
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// PharmacyId (for future multi-pharmacy support)
    /// </summary>
    int? PharmacyId { get; }
}
