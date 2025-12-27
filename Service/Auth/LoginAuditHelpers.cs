using System.Security.Cryptography;
using System.Text;

namespace Service.Auth;


internal static class LoginAuditHelpers
{
    public static string MaskIdentifier(string identifier)
    {
        identifier = identifier.Trim();

        // Email masking: a***@domain.com
        var at = identifier.IndexOf('@');
        if (at > 0)
        {
            var name = identifier[..at];
            var domain = identifier[(at + 1)..];

            var maskedName = name.Length <= 1
                ? "*"
                : $"{name[0]}***";

            // mask domain lightly
            var dot = domain.IndexOf('.');
            var domainHead = dot > 0 ? domain[..dot] : domain;
            var domainTail = dot > 0 ? domain[dot..] : "";

            var maskedDomain = domainHead.Length <= 1
                ? "*"
                : $"{domainHead[0]}***";

            return $"{maskedName}@{maskedDomain}{domainTail}";
        }

        // Username masking: first char + ***
        if (identifier.Length <= 1) return "*";
        return $"{identifier[0]}***";
    }

    public static byte[] Sha256(string input)
    {
        var normalized = input.Trim().ToLowerInvariant();
        var bytes = Encoding.UTF8.GetBytes(normalized);
        return SHA256.HashData(bytes); // 32 bytes
    }
}