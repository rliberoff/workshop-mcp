# MCP Protocol Contracts

This directory contains the API specifications for MCP servers built during the workshop exercises.

## Overview

All MCP servers implement the [Model Context Protocol](https://modelcontextprotocol.io/) specification based on JSON-RPC 2.0. These contracts define the expected request/response formats for exercises.

## Contract Files

- **[mcp-server-base.json](mcp-server-base.json)**: Base protocol messages (initialize, resources/list, tools/list)
- **[exercise-1-static-resource.json](exercise-1-static-resource.json)**: Static resource server contract
- **[exercise-2-parametric-query.json](exercise-2-parametric-query.json)**: Parametric query tool contract
- **[exercise-3-secure-server.json](exercise-3-secure-server.json)**: Authenticated server with rate limiting contract
- **[exercise-4-virtual-analyst.json](exercise-4-virtual-analyst.json)**: Multi-source orchestration contract

## Protocol Version

All contracts use MCP protocol version: **2024-11-05**

## Transport

Servers support both:

- **HTTP**: POST to `/mcp` endpoint with JSON-RPC 2.0 payload
- **stdio**: Standard input/output with line-delimited JSON

## JSON Schema Validation

All request/response payloads are validated against JSON Schema definitions included in each contract file.

## Testing

Use the verification scripts in each exercise directory to validate protocol compliance:

```powershell
# Test Exercise 1 server
.\docs\modules\03-exercise-static-resource\verify.ps1

# Test Exercise 2 server
.\docs\modules\04-exercise-parametric-queries\verify.ps1
```

## References

- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [JSON-RPC 2.0](https://www.jsonrpc.org/specification)
- [JSON Schema](https://json-schema.org/)
