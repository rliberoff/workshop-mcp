using System;

namespace Exercise3SecureServer.Models;

public class TokenRequest
{
    public string UserId { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public string Tier { get; set; } = "basic";
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

public class AuthenticatedUser
{
    public string UserId { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public string Tier { get; set; } = "basic";
}

public class RateLimitInfo
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime ResetTime { get; set; }
}
