using System;
using System.Collections.Generic;

namespace Exercise4VirtualAnalyst.Models;

public class ParsedQuery
{
    public string Intent { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public List<string> RequiredServers { get; set; } = new();
    public string OriginalQuery { get; set; } = string.Empty;
}

public class QueryRequest
{
    public string Query { get; set; } = string.Empty;
}

public class QueryResponse
{
    public string Query { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public List<string> ServersUsed { get; set; } = new();
    public bool FromCache { get; set; }
    public int DurationMs { get; set; }
}
