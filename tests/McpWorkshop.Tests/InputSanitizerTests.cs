using McpWorkshop.Shared.Security;
using Xunit;

namespace McpWorkshop.Tests.Unit.Security;

/// <summary>
/// Unit tests for InputSanitizer
/// Tests HTML encoding, string sanitization, email validation, and URI validation
/// </summary>
public class InputSanitizerTests
{
    #region SanitizeHtml Tests

    [Fact]
    public void SanitizeHtml_WithNull_ReturnsEmptyString()
    {
        // Act
        var result = InputSanitizer.SanitizeHtml(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeHtml_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var result = InputSanitizer.SanitizeHtml(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>", "&lt;script&gt;alert(&#x27;xss&#x27;)&lt;/script&gt;")]
    [InlineData("<img src=x onerror=alert(1)>", "&lt;img src=x onerror=alert(1)&gt;")]
    [InlineData("<div onclick=\"evil()\">test</div>", "&lt;div onclick=&quot;evil()&quot;&gt;test&lt;/div&gt;")]
    public void SanitizeHtml_WithMaliciousInput_EncodesCorrectly(string malicious, string expected)
    {
        // Act
        var result = InputSanitizer.SanitizeHtml(malicious);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizeHtml_WithNormalText_PreservesContent()
    {
        // Arrange
        var input = "Hello World 123";

        // Act
        var result = InputSanitizer.SanitizeHtml(input);

        // Assert
        Assert.Equal(input, result);
    }

    #endregion

    #region SanitizeString Tests

    [Fact]
    public void SanitizeString_WithNull_ReturnsEmptyString()
    {
        // Act
        var result = InputSanitizer.SanitizeString(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeString_WithControlCharacters_RemovesInvalidChars()
    {
        // Arrange
        var input = "Hello\0World\x01Test\x1F";

        // Act
        var result = InputSanitizer.SanitizeString(input);

        // Assert
        Assert.Equal("HelloWorldTest", result);
        // Verify length matches expected output (control characters removed)
        Assert.Equal(14, result.Length);
    }

    [Fact]
    public void SanitizeString_WithTabNewLineCarriageReturn_PreservesWhitespace()
    {
        // Arrange
        var input = "Line1\t\nLine2\r\nLine3";

        // Act
        var result = InputSanitizer.SanitizeString(input);

        // Assert
        Assert.Contains("\t", result);
        Assert.Contains("\n", result);
        Assert.Contains("\r", result);
    }

    [Fact]
    public void SanitizeString_ExceedsMaxLength_TruncatesCorrectly()
    {
        // Arrange
        var input = new string('A', 2000);

        // Act
        var result = InputSanitizer.SanitizeString(input, maxLength: 1000);

        // Assert
        Assert.Equal(1000, result.Length);
    }

    [Fact]
    public void SanitizeString_WithLeadingTrailingSpaces_TrimsCorrectly()
    {
        // Arrange
        var input = "   text with spaces   ";

        // Act
        var result = InputSanitizer.SanitizeString(input);

        // Assert
        Assert.Equal("text with spaces", result);
    }

    #endregion

    #region IsValidEmail Tests

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co.uk")]
    [InlineData("valid+email@test.org")]
    public void IsValidEmail_WithValidEmail_ReturnsTrue(string email)
    {
        // Act
        var result = InputSanitizer.IsValidEmail(email);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@missing-local.com")]
    [InlineData("missing-at-sign.com")]
    [InlineData("double@@example.com")]
    [InlineData("spaces in@email.com")]
    public void IsValidEmail_WithInvalidEmail_ReturnsFalse(string? email)
    {
        // Act
        var result = InputSanitizer.IsValidEmail(email);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region IsValidUri Tests

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    [InlineData("http://localhost:5000")]
    [InlineData("https://sub.domain.example.com/path?query=value")]
    public void IsValidUri_WithValidHttpUri_ReturnsTrue(string uri)
    {
        // Act
        var result = InputSanitizer.IsValidUri(uri);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData("file:///path/to/file")]
    [InlineData("javascript:alert(1)")]
    [InlineData("//example.com")]
    public void IsValidUri_WithInvalidUri_ReturnsFalse(string? uri)
    {
        // Act
        var result = InputSanitizer.IsValidUri(uri);

        // Assert
        Assert.False(result);
    }

    #endregion
}
