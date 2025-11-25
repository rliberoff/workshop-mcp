using System.Text.Encodings.Web;

namespace McpWorkshop.Shared.Security;

/// <summary>
/// Utility class for input sanitization.
/// </summary>
public static class InputSanitizer
{
    private static readonly HtmlEncoder HtmlEncoder = HtmlEncoder.Default;

    /// <summary>
    /// Sanitize HTML input to prevent XSS attacks.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>The sanitized HTML string.</returns>
    public static string SanitizeHtml(string? input)
    {
        return string.IsNullOrEmpty(input) ? string.Empty : HtmlEncoder.Encode(input);
    }

    /// <summary>
    /// Validate and sanitize string input (remove control characters).
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <returns>The sanitized string.</returns>
    public static string SanitizeString(string? input, int maxLength = 1000)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // Remove control characters except tab, line feed, carriage return
        var sanitized = new string([.. input.Where(c => !char.IsControl(c) || c == '\t' || c == '\n' || c == '\r')]);

        // Trim to max length
        if (sanitized.Length > maxLength)
        {
            sanitized = sanitized[..maxLength];
        }

        return sanitized.Trim();
    }

    /// <summary>
    /// Validate email format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email is valid; otherwise, false.</returns>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

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
    /// Validate URI format.
    /// </summary>
    /// <param name="uri">The URI to validate.</param>
    /// <returns>True if the URI is valid; otherwise, false.</returns>
    public static bool IsValidUri(string? uri)
    {
        return !string.IsNullOrWhiteSpace(uri) && Uri.TryCreate(uri, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
