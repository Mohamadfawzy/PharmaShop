using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Auth.Extensions;

public static class IdentityResultExtensions
{
    public static Dictionary<string, string[]> ToFieldErrors(this IdentityResult result)
    {
        var dict = new Dictionary<string, List<string>>();

        foreach (var error in result.Errors)
        {
            var field = MapIdentityErrorToField(error.Code);

            if (!dict.ContainsKey(field))
                dict[field] = new List<string>();

            dict[field].Add(error.Description);
        }

        return dict.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToArray()
        );
    }

    private static string MapIdentityErrorToField(string errorCode)
    {
        // Password-related
        if (errorCode.Contains("Password", StringComparison.OrdinalIgnoreCase))
            return "Password";

        // Email-related
        if (errorCode.Contains("Email", StringComparison.OrdinalIgnoreCase))
            return "Email";

        // Username-related
        if (errorCode.Contains("UserName", StringComparison.OrdinalIgnoreCase))
            return "UserName";

        // Fallback
        return "General";
    }
}