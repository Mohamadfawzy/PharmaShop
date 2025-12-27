using Shared.Models.Auth;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices;

public interface IUserRoleService
{
    Task<AppResponse<UserRolesResponseDto>> SetRolesAsync(int userId, List<string> roles, CancellationToken ct);
    Task<AppResponse<UserRolesResponseDto>> AddRolesAsync(int userId, List<string> roles, CancellationToken ct);
    Task<AppResponse<UserRolesResponseDto>> RemoveRolesAsync(int userId, List<string> roles, CancellationToken ct);

    Task<AppResponse<List<RoleDto>>> GetAllRolesAsync(CancellationToken ct);
    Task<AppResponse<UserRolesResponseDto>> GetUserRolesAsync(int userId, CancellationToken ct);



}