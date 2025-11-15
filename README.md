# MCP .NET DLL Reflector

A Model Context Protocol (MCP) server that provides AI-friendly access to .NET assembly metadata. This server allows Claude Desktop and other MCP-compatible clients to inspect and analyze .NET DLLs with a focus on token-efficient, readable output.

## Features

- **AI-Optimized Output**: Default formatter renders C# code in a concise, token-efficient format perfect for AI consumption
- **Comprehensive Type Inspection**: Explore namespaces, classes, interfaces, enums, structs, and their members
- **Smart Search**: Use regex patterns to search across types, methods, properties, fields, and enum values
- **Multiple DLL Support**: Load and analyze multiple assemblies simultaneously
- **Cross-Platform**: Works on Windows, macOS, and Linux with .NET 9.0

## Key Improvements

### AI Consumption Formatter
The new `AiConsumptionFormatter` (default) provides:
- Concise C# code representation instead of verbose JSON
- Preserves all essential information (documentation, modifiers, parameters)
- Significantly reduces token usage for AI models
- Shows actual C# syntax as it would appear in code

Example output difference:

**JSON Format** (old):
```json
{
  "Types": [{
    "Name": "String",
    "Namespace": "System",
    "TypeKind": "Class",
    "Methods": [...]
  }]
}
```

**AI Format** (new):
```csharp
namespace System;

public class String
{
    /// Gets the length of the string
    public int Length { get; set; }

    public static string Format(string format, object arg0);
}
```

## Installation

### Prerequisites
- .NET 9.0 SDK or Runtime
- Claude Desktop or Claude Code CLI

### Build from Source
```bash
git clone https://github.com/yourusername/mcp-dotnet-dll-reflector.git
cd mcp-dotnet-dll-reflector/McpNetDll
dotnet publish -c Release
```

The executable will be at: `McpNetDll/bin/Release/net9.0/publish/McpNetDll.exe`

## Configuration

### Claude Desktop (Windows)

Add to `%APPDATA%\Claude\claude_desktop_config.json`:

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

### Claude Desktop (macOS)

Add to `~/Library/Application Support/Claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "dotnet-dll-reflector": {
      "type": "stdio",
      "command": "/path/to/McpNetDll",
      "args": [
        "/path/to/your.dll"
      ]
    }
  }
}
```

### Using JSON Format (Optional)

To use the JSON formatter instead of the default AI-optimized format:

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

Or via environment variable:

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

### ListNamespaces
Lists all public namespaces and their types from loaded assemblies.

**Parameters:**
- `namespaces` (optional): Specific namespaces to inspect
- `limit` (optional): Maximum results (default: 50)
- `offset` (optional): Skip results (default: 0)

**Example:** "List all namespaces in the loaded DLLs"

### GetTypeDetails
Gets detailed public API information for specific .NET types.

**Parameters:**
- `typeNames` (required): Array of type names to analyze

**Example:** "Show me the details of System.String and System.DateTime"

### SearchElements
Searches across all elements using regex patterns.

**Parameters:**
- `pattern` (required): Regex pattern to search
- `searchScope` (optional): 'all', 'types', 'methods', 'properties', 'fields', 'enums'
- `limit` (optional): Maximum results (default: 100)
- `offset` (optional): Skip results (default: 0)

**Example:** "Search for all methods that start with 'Get'"

## Usage Examples

After configuration, ask Claude:

- "List all namespaces in the loaded assemblies"
- "Show me the public API of the String class"
- "Find all classes that implement IDisposable"
- "Search for methods containing 'Async' in their name"
- "Show all enum types in the System namespace"
- "Find all static methods that return Task"

## Architecture

The project uses a clean architecture with:

- **McpNetDll**: Main executable and MCP server host
- **McpNetDll.Core**: Core business logic, repositories, and formatters
- **McpNetDll.Tests**: Unit tests
- **McpNetDll.Web**: Web API variant (optional)

Key components:

- `ITypeRegistry`: Manages loaded assemblies and type discovery
- `IMetadataRepository`: Queries and retrieves type metadata
- `IMcpResponseFormatter`: Formats responses (AI or JSON format)
- `DllMetadataTool`: MCP tool implementations

## Development

### Running Tests
```bash
dotnet test
```

### Development Mode
```bash
cd McpNetDll
dotnet run -- path/to/test.dll
```

### Adding New Formatters
Implement the `IMcpResponseFormatter` interface:

```csharp
public class CustomFormatter : IMcpResponseFormatter
{
    public string FormatNamespaceResponse(NamespaceQueryResult result, ITypeRegistry registry);
    public string FormatTypeDetailsResponse(TypeDetailsQueryResult result, ITypeRegistry registry);
    public string FormatSearchResponse(SearchQueryResult result, ITypeRegistry registry);
}
```

## Troubleshooting

### Server Not Starting
- Verify .NET 9.0 is installed: `dotnet --version`
- Check file paths use forward slashes or escaped backslashes
- Ensure DLL paths are valid .NET assemblies

### DLL Not Loading
- Verify the DLL is a valid .NET assembly
- Check that dependencies are in the same directory or GAC
- Try with a simple test DLL first

### No Tools Available in Claude
- Restart Claude Desktop after configuration changes
- Check Claude Desktop logs for errors
- Verify the configuration JSON is valid

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome! Please submit pull requests or issues on GitHub.

## Acknowledgments

Built with the Model Context Protocol SDK for .NET by Anthropic.