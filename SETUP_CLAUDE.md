# MCP .NET DLL Reflector Setup Guide

This guide explains how to configure the MCP .NET DLL Reflector with Claude Desktop, Claude Code CLI, and OpenAI Codex CLI.

## Prerequisites

- .NET 9.0 SDK installed
- Claude Desktop, Claude Code CLI, or OpenAI Codex CLI installed
- DLL files you want to analyze

## Building the Project

### For Development
```bash
cd McpNetDll
dotnet build
```

### For Production (Published Executable)
```bash
cd McpNetDll
dotnet publish -c Release
```

The published executable will be at: `McpNetDll/bin/Release/net9.0/publish/McpNetDll.exe`

## Configuration for Claude Desktop

### Windows
1. Open Claude Desktop
2. Navigate to Settings → Developer → Edit Config
3. Or manually edit: `%APPDATA%\Claude\claude_desktop_config.json`

### macOS
1. Open Claude Desktop
2. Navigate to Settings → Developer → Edit Config
3. Or manually edit: `~/Library/Application Support/Claude/claude_desktop_config.json`

### Configuration Example

Add this to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "dotnet-dll-reflector": {
      "type": "stdio",
      "command": "C:/path/to/McpNetDll.exe",
      "args": [
        "C:/path/to/your.dll",
        "C:/path/to/another.dll"
      ]
    }
  }
}
```

Or for development mode using `dotnet run`:

```json
{
  "mcpServers": {
    "dotnet-dll-reflector-dev": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:/path/to/McpNetDll/McpNetDll.csproj",
        "--",
        "C:/path/to/your.dll"
      ]
    }
  }
}
```

## Configuration for Claude Code CLI

For Claude Code CLI, you can add the server using:

```bash
# Add the MCP server
claude mcp add dotnet-dll-reflector --scope user

# When prompted, provide:
# Command: C:/path/to/McpNetDll.exe
# Arguments: C:/path/to/your.dll
```

Or manually edit the configuration:

```bash
# List current servers
claude mcp list

# Test server
claude mcp get dotnet-dll-reflector
```

## Using the AI-Optimized Format

By default, the server uses the AI-optimized concise format. To use JSON format instead:

### Option 1: Command Line Flag
Add `--json-format` to the args array:

```json
{
  "mcpServers": {
    "dotnet-dll-reflector": {
      "type": "stdio",
      "command": "C:/path/to/McpNetDll.exe",
      "args": [
        "C:/path/to/your.dll",
        "--json-format"
      ]
    }
  }
}
```

### Option 2: Environment Variable
Set the environment variable:

```json
{
  "mcpServers": {
    "dotnet-dll-reflector": {
      "type": "stdio",
      "command": "C:/path/to/McpNetDll.exe",
      "args": ["C:/path/to/your.dll"],
      "env": {
        "MCP_JSON_FORMAT": "true"
      }
    }
  }
}
```

## Available Tools

Once configured, the following tools will be available in Claude:

### 1. `ListNamespaces`
Lists all public namespaces and their types from loaded .NET assemblies.

Parameters:
- `namespaces` (optional): Specific namespaces to inspect
- `limit` (optional): Maximum number to return (default: 50)
- `offset` (optional): Number to skip (default: 0)

### 2. `GetTypeDetails`
Gets detailed public API information for specific .NET types.

Parameters:
- `typeNames` (required): Array of type names to analyze

### 3. `SearchElements`
Searches across all elements matching a regex pattern.

Parameters:
- `pattern` (required): Regex pattern to search
- `searchScope` (optional): 'all', 'types', 'methods', 'properties', 'fields', 'enums'
- `limit` (optional): Maximum results (default: 100)
- `offset` (optional): Results to skip (default: 0)

## Multiple DLL Configuration

You can load multiple DLLs:

```json
{
  "mcpServers": {
    "system-dlls": {
      "type": "stdio",
      "command": "C:/path/to/McpNetDll.exe",
      "args": [
        "C:/Windows/Microsoft.NET/Framework64/v4.0.30319/System.dll",
        "C:/Windows/Microsoft.NET/Framework64/v4.0.30319/System.Core.dll",
        "C:/Windows/Microsoft.NET/Framework64/v4.0.30319/System.Data.dll"
      ]
    },
    "custom-dlls": {
      "type": "stdio",
      "command": "C:/path/to/McpNetDll.exe",
      "args": [
        "C:/myproject/bin/MyLibrary.dll",
        "C:/myproject/bin/MyOtherLibrary.dll"
      ]
    }
  }
}
```

## Configuration for OpenAI Codex CLI

### Windows/macOS/Linux
1. Configuration file location: `~/.codex/config.toml`
2. Add the following to your config.toml:

```toml
[mcp_servers.dotnet-dll-reflector]
command = "C:/path/to/McpNetDll.exe C:/path/to/your.dll"
description = ".NET DLL Reflector with AI-optimized output"
startup_timeout_ms = 15000
```

### Using CLI Commands
You can also add the server using Codex CLI commands:

```bash
# Add the MCP server
codex mcp add dotnet-dll-reflector --startup-timeout 15000 -- "C:/path/to/McpNetDll.exe C:/path/to/your.dll"

# List configured servers
codex mcp list

# Show server configuration
codex mcp show dotnet-dll-reflector

# Remove server if needed
codex mcp delete dotnet-dll-reflector
```

### Multiple DLL Configuration for Codex
```toml
[mcp_servers.dotnet-dll-reflector]
command = "C:/path/to/McpNetDll.exe C:/dll1.dll C:/dll2.dll C:/dll3.dll"
description = "Multiple DLLs analyzer"
startup_timeout_ms = 15000
```

## Troubleshooting

### Server Not Starting
1. Check the paths are correct and use forward slashes or escaped backslashes
2. Ensure .NET 9.0 runtime is installed: `dotnet --version`
3. Check Claude Desktop logs for errors

### DLL Not Loading
1. Ensure the DLL path is correct
2. Check that the DLL is a valid .NET assembly
3. Dependencies of the DLL should be in the same directory or GAC

### Restart Claude Desktop
After modifying `claude_desktop_config.json`, restart Claude Desktop for changes to take effect.

## Example Usage in Claude

Once configured, you can ask Claude:

- "List all namespaces in the loaded DLLs"
- "Show me the details of the String class"
- "Search for all methods that start with 'Get'"
- "Find all classes that implement IDisposable"
- "Show me all enum types in the System namespace"

The AI-optimized formatter will provide concise, readable C# code representations perfect for AI analysis.