using System.Security.Cryptography;
using System.Text;

namespace Sentinel.Api.Security;

public static class WebhookSecurity
{
    public static bool FixedTimeEquals(string? provided, string? expected)
    {
        if (string.IsNullOrWhiteSpace(provided) || string.IsNullOrWhiteSpace(expected))
            return false;

        var left = Encoding.UTF8.GetBytes(provided);
        var right = Encoding.UTF8.GetBytes(expected);
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    public static bool IsAuthorizedBearer(string? header, string? expectedToken)
    {
        const string prefix = "Bearer ";
        return header is not null &&
            header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            FixedTimeEquals(header[prefix.Length..].Trim(), expectedToken);
    }

    public static bool VerifyMetaSignature(ReadOnlySpan<byte> body, string? header, string? appSecret)
    {
        if (string.IsNullOrWhiteSpace(appSecret) || header is null ||
            !header.StartsWith("sha256=", StringComparison.Ordinal))
            return false;

        var hex = header["sha256=".Length..];
        Span<byte> supplied = stackalloc byte[32];
        if (!Convert.TryFromHexString(hex, supplied, out var count) || count != 32)
            return false;

        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(appSecret), body);
        return CryptographicOperations.FixedTimeEquals(supplied, expected);
    }
}
