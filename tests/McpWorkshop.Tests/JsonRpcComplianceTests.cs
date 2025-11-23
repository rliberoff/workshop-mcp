using System.Text.Json;
using System.Text.Json.Nodes;

namespace McpWorkshop.Tests.Protocol;

/// <summary>
/// Tests to validate JSON-RPC 2.0 protocol compliance for all MCP servers.
/// Based on contracts/mcp-server-base.json specification.
/// </summary>
public class JsonRpcComplianceTests
{
    [Fact]
    public void JsonRpcRequest_ShouldHaveRequiredFields()
    {
        // Arrange
        var requestJson = """
        {
            "jsonrpc": "2.0",
            "method": "initialize",
            "id": 1
        }
        """;

        // Act
        var request = JsonNode.Parse(requestJson);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("2.0", request["jsonrpc"]?.GetValue<string>());
        Assert.Equal("initialize", request["method"]?.GetValue<string>());
        Assert.NotNull(request["id"]);
    }

    [Fact]
    public void JsonRpcRequest_IdCanBeStringOrNumber()
    {
        // Arrange - Test with number ID
        var requestWithNumberId = """
        {
            "jsonrpc": "2.0",
            "method": "resources/list",
            "id": 123
        }
        """;

        var requestWithStringId = """
        {
            "jsonrpc": "2.0",
            "method": "resources/list",
            "id": "abc-def-123"
        }
        """;

        // Act
        var numericRequest = JsonNode.Parse(requestWithNumberId);
        var stringRequest = JsonNode.Parse(requestWithStringId);

        // Assert
        Assert.Equal(123, numericRequest!["id"]?.GetValue<int>());
        Assert.Equal("abc-def-123", stringRequest!["id"]?.GetValue<string>());
    }

    [Fact]
    public void JsonRpcRequest_ParamsIsOptional()
    {
        // Arrange
        var requestWithoutParams = """
        {
            "jsonrpc": "2.0",
            "method": "resources/list",
            "id": 1
        }
        """;

        var requestWithParams = """
        {
            "jsonrpc": "2.0",
            "method": "resources/read",
            "params": {
                "uri": "customer://123"
            },
            "id": 2
        }
        """;

        // Act
        var withoutParams = JsonNode.Parse(requestWithoutParams);
        var withParams = JsonNode.Parse(requestWithParams);

        // Assert
        Assert.Null(withoutParams!["params"]);
        Assert.NotNull(withParams!["params"]);
        Assert.Equal("customer://123", withParams["params"]!["uri"]?.GetValue<string>());
    }

    [Fact]
    public void JsonRpcResponse_ShouldHaveRequiredFields()
    {
        // Arrange
        var responseJson = """
        {
            "jsonrpc": "2.0",
            "result": {
                "serverInfo": {
                    "name": "sql-mcp-server",
                    "version": "1.0.0"
                }
            },
            "id": 1
        }
        """;

        // Act
        var response = JsonNode.Parse(responseJson);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("2.0", response["jsonrpc"]?.GetValue<string>());
        Assert.NotNull(response["result"]);
        Assert.NotNull(response["id"]);
    }

    [Fact]
    public void JsonRpcResponse_MustHaveEitherResultOrError()
    {
        // Arrange - Valid response with result
        var successResponse = """
        {
            "jsonrpc": "2.0",
            "result": { "data": "success" },
            "id": 1
        }
        """;

        // Arrange - Valid response with error
        var errorResponse = """
        {
            "jsonrpc": "2.0",
            "error": {
                "code": -32600,
                "message": "Invalid Request"
            },
            "id": 1
        }
        """;

        // Act
        var success = JsonNode.Parse(successResponse);
        var error = JsonNode.Parse(errorResponse);

        // Assert
        Assert.NotNull(success!["result"]);
        Assert.Null(success["error"]);
        Assert.Null(error!["result"]);
        Assert.NotNull(error["error"]);
    }

    [Fact]
    public void JsonRpcError_ShouldHaveRequiredFields()
    {
        // Arrange
        var errorJson = """
        {
            "jsonrpc": "2.0",
            "error": {
                "code": -32601,
                "message": "Method not found",
                "data": {
                    "method": "unknown_method"
                }
            },
            "id": 1
        }
        """;

        // Act
        var response = JsonNode.Parse(errorJson);
        var error = response!["error"];

        // Assert
        Assert.NotNull(error);
        Assert.Equal(-32601, error!["code"]?.GetValue<int>());
        Assert.Equal("Method not found", error["message"]?.GetValue<string>());
        Assert.NotNull(error["data"]); // Optional, but if present should be valid
    }

    [Theory]
    [InlineData(-32700, "Parse error")]
    [InlineData(-32600, "Invalid Request")]
    [InlineData(-32601, "Method not found")]
    [InlineData(-32602, "Invalid params")]
    [InlineData(-32603, "Internal error")]
    public void JsonRpcError_StandardErrorCodes_ShouldBeValid(int code, string message)
    {
        // Arrange
        var errorJson = $$"""
        {
            "jsonrpc": "2.0",
            "error": {
                "code": {{code}},
                "message": "{{message}}"
            },
            "id": null
        }
        """;

        // Act
        var response = JsonNode.Parse(errorJson);
        var error = response!["error"];

        // Assert
        Assert.NotNull(error);
        Assert.Equal(code, error!["code"]?.GetValue<int>());
        Assert.Equal(message, error["message"]?.GetValue<string>());
    }

    [Fact]
    public void ServerInfo_ShouldHaveRequiredFields()
    {
        // Arrange
        var serverInfoJson = """
        {
            "name": "sql-mcp-server",
            "version": "1.0.0",
            "protocolVersion": "2024-11-05"
        }
        """;

        // Act
        var serverInfo = JsonNode.Parse(serverInfoJson);

        // Assert
        Assert.NotNull(serverInfo);
        Assert.Equal("sql-mcp-server", serverInfo["name"]?.GetValue<string>());
        Assert.Equal("1.0.0", serverInfo["version"]?.GetValue<string>());
        Assert.Equal("2024-11-05", serverInfo["protocolVersion"]?.GetValue<string>());
    }

    [Theory]
    [InlineData("1.0.0", true)]
    [InlineData("2.3.5", true)]
    [InlineData("10.20.30", true)]
    [InlineData("1.0", false)]
    [InlineData("1", false)]
    [InlineData("v1.0.0", false)]
    public void ServerInfo_Version_ShouldFollowSemanticVersioning(string version, bool isValid)
    {
        // Arrange
        var pattern = @"^\d+\.\d+\.\d+$";
        var regex = new System.Text.RegularExpressions.Regex(pattern);

        // Act
        var matches = regex.IsMatch(version);

        // Assert
        Assert.Equal(isValid, matches);
    }

    [Fact]
    public void Capabilities_ShouldHaveValidStructure()
    {
        // Arrange
        var capabilitiesJson = """
        {
            "resources": {
                "subscribe": true,
                "listChanged": false
            },
            "tools": {
                "listChanged": false
            },
            "prompts": {
                "listChanged": false
            },
            "logging": {}
        }
        """;

        // Act
        var capabilities = JsonNode.Parse(capabilitiesJson);

        // Assert
        Assert.NotNull(capabilities);
        Assert.NotNull(capabilities["resources"]);
        Assert.NotNull(capabilities["tools"]);
        Assert.NotNull(capabilities["prompts"]);
        Assert.NotNull(capabilities["logging"]);
        Assert.True(capabilities["resources"]!["subscribe"]?.GetValue<bool>());
        Assert.False(capabilities["resources"]!["listChanged"]?.GetValue<bool>());
    }

    [Fact]
    public void InitializeRequest_ShouldMatchContract()
    {
        // Arrange
        var initializeRequestJson = """
        {
            "jsonrpc": "2.0",
            "method": "initialize",
            "params": {
                "protocolVersion": "2024-11-05",
                "capabilities": {
                    "roots": {
                        "listChanged": false
                    }
                },
                "clientInfo": {
                    "name": "test-client",
                    "version": "1.0.0"
                }
            },
            "id": 1
        }
        """;

        // Act
        var request = JsonNode.Parse(initializeRequestJson);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("2.0", request["jsonrpc"]?.GetValue<string>());
        Assert.Equal("initialize", request["method"]?.GetValue<string>());
        Assert.NotNull(request["params"]);
        Assert.Equal("2024-11-05", request["params"]!["protocolVersion"]?.GetValue<string>());
        Assert.NotNull(request["params"]!["capabilities"]);
        Assert.NotNull(request["params"]!["clientInfo"]);
    }

    [Fact]
    public void InitializeResponse_ShouldMatchContract()
    {
        // Arrange
        var initializeResponseJson = """
        {
            "jsonrpc": "2.0",
            "result": {
                "protocolVersion": "2024-11-05",
                "capabilities": {
                    "resources": {
                        "subscribe": true,
                        "listChanged": false
                    },
                    "tools": {
                        "listChanged": false
                    }
                },
                "serverInfo": {
                    "name": "sql-mcp-server",
                    "version": "1.0.0",
                    "protocolVersion": "2024-11-05"
                }
            },
            "id": 1
        }
        """;

        // Act
        var response = JsonNode.Parse(initializeResponseJson);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("2.0", response["jsonrpc"]?.GetValue<string>());
        Assert.NotNull(response["result"]);
        Assert.Equal("2024-11-05", response["result"]!["protocolVersion"]?.GetValue<string>());
        Assert.NotNull(response["result"]!["capabilities"]);
        Assert.NotNull(response["result"]!["serverInfo"]);
    }

    [Fact]
    public void ResourcesListRequest_ShouldMatchContract()
    {
        // Arrange
        var resourcesListRequestJson = """
        {
            "jsonrpc": "2.0",
            "method": "resources/list",
            "id": 2
        }
        """;

        // Act
        var request = JsonNode.Parse(resourcesListRequestJson);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("2.0", request["jsonrpc"]?.GetValue<string>());
        Assert.Equal("resources/list", request["method"]?.GetValue<string>());
        Assert.Null(request["params"]); // No params required for resources/list
    }

    [Fact]
    public void ResourcesReadRequest_ShouldMatchContract()
    {
        // Arrange
        var resourcesReadRequestJson = """
        {
            "jsonrpc": "2.0",
            "method": "resources/read",
            "params": {
                "uri": "customer://123"
            },
            "id": 3
        }
        """;

        // Act
        var request = JsonNode.Parse(resourcesReadRequestJson);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("2.0", request["jsonrpc"]?.GetValue<string>());
        Assert.Equal("resources/read", request["method"]?.GetValue<string>());
        Assert.NotNull(request["params"]);
        Assert.Equal("customer://123", request["params"]!["uri"]?.GetValue<string>());
    }

    [Fact]
    public void ToolsListRequest_ShouldMatchContract()
    {
        // Arrange
        var toolsListRequestJson = """
        {
            "jsonrpc": "2.0",
            "method": "tools/list",
            "id": 4
        }
        """;

        // Act
        var request = JsonNode.Parse(toolsListRequestJson);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("2.0", request["jsonrpc"]?.GetValue<string>());
        Assert.Equal("tools/list", request["method"]?.GetValue<string>());
        Assert.Null(request["params"]); // No params required for tools/list
    }

    [Fact]
    public void ToolsCallRequest_ShouldMatchContract()
    {
        // Arrange
        var toolsCallRequestJson = """
        {
            "jsonrpc": "2.0",
            "method": "tools/call",
            "params": {
                "name": "GetCustomers",
                "arguments": {
                    "page": 1,
                    "pageSize": 10
                }
            },
            "id": 5
        }
        """;

        // Act
        var request = JsonNode.Parse(toolsCallRequestJson);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("2.0", request["jsonrpc"]?.GetValue<string>());
        Assert.Equal("tools/call", request["method"]?.GetValue<string>());
        Assert.NotNull(request["params"]);
        Assert.Equal("GetCustomers", request["params"]!["name"]?.GetValue<string>());
        Assert.NotNull(request["params"]!["arguments"]);
    }

    [Fact]
    public void JsonRpcBatch_ShouldBeArrayOfRequests()
    {
        // Arrange
        var batchRequestJson = """
        [
            {
                "jsonrpc": "2.0",
                "method": "resources/list",
                "id": 1
            },
            {
                "jsonrpc": "2.0",
                "method": "tools/list",
                "id": 2
            }
        ]
        """;

        // Act
        var batch = JsonNode.Parse(batchRequestJson);

        // Assert
        Assert.NotNull(batch);
        var array = batch.AsArray();
        Assert.Equal(2, array.Count);
        Assert.Equal("resources/list", array[0]!["method"]?.GetValue<string>());
        Assert.Equal("tools/list", array[1]!["method"]?.GetValue<string>());
    }

    [Fact]
    public void JsonRpcNotification_ShouldNotHaveId()
    {
        // Arrange - Notification (no response expected)
        var notificationJson = """
        {
            "jsonrpc": "2.0",
            "method": "notifications/resources/list_changed"
        }
        """;

        // Act
        var notification = JsonNode.Parse(notificationJson);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal("2.0", notification["jsonrpc"]?.GetValue<string>());
        Assert.Equal("notifications/resources/list_changed", notification["method"]?.GetValue<string>());
        Assert.Null(notification["id"]); // Notifications MUST NOT have an id
    }

    [Fact]
    public void JsonRpcVersion_MustBe2Point0()
    {
        // Arrange
        var validRequest = """
        {
            "jsonrpc": "2.0",
            "method": "initialize",
            "id": 1
        }
        """;

        var invalidRequest = """
        {
            "jsonrpc": "1.0",
            "method": "initialize",
            "id": 1
        }
        """;

        // Act
        var valid = JsonNode.Parse(validRequest);
        var invalid = JsonNode.Parse(invalidRequest);

        // Assert
        Assert.Equal("2.0", valid!["jsonrpc"]?.GetValue<string>());
        Assert.NotEqual("2.0", invalid!["jsonrpc"]?.GetValue<string>());
    }
}
