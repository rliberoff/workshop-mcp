using System.Text.Encodings.Web;

namespace McpWorkshop.Shared.Security;

/// <summary>
/// Utility class for input sanitization
/// </summary>
public static class InputSanitizer
{
    private static readonly HtmlEncoder _htmlEncoder = HtmlEncoder.Default;

    /// <summary>
    /// Sanitize HTML input to prevent XSS attacks
    /// </summary>
    public static string SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return _htmlEncoder.Encode(input);
    }

    /// <summary>
    /// Validate and sanitize string input (remove control characters)
    /// </summary>
    public static string SanitizeString(string? input, int maxLength = 1000)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Remove control characters except tab, line feed, carriage return
        var sanitized = new string(input
            .Where(c => !char.IsControl(c) || c == '\t' || c == '\n' || c == '\r')
            .ToArray());

        // Trim to max length
        if (sanitized.Length > maxLength)
            sanitized = sanitized[..maxLength];

        return sanitized.Trim();
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate URI format
    /// </summary>
    public static bool IsValidUri(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            return false;

        return Uri.TryCreate(uri, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
