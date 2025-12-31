# McpNetDll - .NET DLL Reflector

A .NET 9.0 tool for analyzing and extracting metadata from .NET assemblies. Provides an interactive Web UI for browsing assemblies, an MCP (Model Context Protocol) server for AI assistants, and a REST API for programmatic access.

## Features

- **Interactive Web UI** - Browser-based assembly explorer with namespace tree, type details, and search
- **Assembly Introspection** - Extract types, methods, properties, fields, and enums from .NET DLLs
- **MCP Server** - Model Context Protocol tools for AI assistants (Claude, etc.)
- **REST API** - RESTful endpoints for programmatic access
- **Full-text Search** - Lucene.NET-powered keyword search with OR/AND logic
- **Regex Search** - Pattern matching across all assembly elements
- **Multiple DLL Support** - Load and analyze multiple assemblies simultaneously
- **Dynamic Loading** - Add assemblies at runtime via UI or API
- **AI-Optimized Output** - Compact formatting designed for LLM consumption

## Quick Start

### Build

```bash
dotnet build
```

### Publish for Distribution

```bash
# MCP Server
dotnet publish McpNetDll -c Release -o ./publish/mcp

# Web API
dotnet publish McpNetDll.Web -c Release -o ./publish/web
```

## Running the Web Server

The Web API provides REST endpoints and an interactive Web UI for accessing assembly metadata.

### Web UI Features

Access the UI at `http://localhost:5000` (default) after starting the server:

- **Namespace Browser** - Hierarchical tree view with expand/collapse all
- **Type Details Panel** - View methods, properties, fields, enum values (grouped by static/instance)
- **Dual Search** - Keyword search (Lucene full-text) and regex pattern search
- **Type Reference Links** - Clickable type names navigate to type details
- **Dynamic DLL Loading** - Load additional assemblies via the UI
- **Resizable Sidebar** - Width persisted across sessions
- **Keyboard Shortcuts** - `Ctrl+K` to focus search, `Escape` to clear
- **Deep Linking** - URL hash routing (e.g., `#type=Namespace.ClassName`)

### CLI Usage

```bash
McpNetDll.Web.exe [--urls <url>] [--DllPaths:0 <path>] [--DllPaths:1 <path>] ...
```

### Configuration Methods

DLL paths can be configured in three ways (in order of precedence):

#### 1. Command-Line Arguments

```bash
# Single DLL
./publish/web/McpNetDll.Web.exe --DllPaths:0 "C:/Libraries/MyLibrary.dll"

# Multiple DLLs
./publish/web/McpNetDll.Web.exe --DllPaths:0 "C:/Libs/Core.dll" --DllPaths:1 "C:/Libs/Data.dll"

# With custom port
./publish/web/McpNetDll.Web.exe --urls "http://localhost:8080" --DllPaths:0 "C:/Libs/MyLib.dll"
```

#### 2. Environment Variables

```bash
# Windows (cmd)
set DllPaths__0=C:\Libraries\First.dll
set DllPaths__1=C:\Libraries\Second.dll
McpNetDll.Web.exe

# Windows (PowerShell)
$env:DllPaths__0 = "C:\Libraries\First.dll"
$env:DllPaths__1 = "C:\Libraries\Second.dll"
.\McpNetDll.Web.exe

# Linux/macOS
export DllPaths__0="/path/to/first.dll"
export DllPaths__1="/path/to/second.dll"
./McpNetDll.Web
```

#### 3. Configuration File (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DllPaths": [
    "C:/path/to/first.dll",
    "C:/path/to/second.dll"
  ]
}
```

### Examples

```bash
# Run with default settings (reads from appsettings.json)
./publish/web/McpNetDll.Web.exe

# Run on a specific port
./publish/web/McpNetDll.Web.exe --urls "http://localhost:5000"

# Run on multiple URLs (HTTP + HTTPS)
./publish/web/McpNetDll.Web.exe --urls "http://localhost:5000;https://localhost:5001"

# Full example: custom port + DLLs from CLI
./publish/web/McpNetDll.Web.exe --urls "http://0.0.0.0:8080" --DllPaths:0 "C:/MyApp/bin/MyApp.dll"

# Using environment variables for URL
set ASPNETCORE_URLS=http://localhost:8080
./publish/web/McpNetDll.Web.exe
```

### Dynamic DLL Loading

Load additional DLLs at runtime via the API:

```bash
curl -X POST "http://localhost:5000/api/load?path=C:/path/to/new.dll"
```

### REST API Endpoints

#### Info
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/info` | Get assembly statistics |

#### Namespaces
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/namespaces/list` | List all namespace names |
| GET | `/api/namespaces?namespaces=&limit=50&offset=0` | Query namespaces with details |

#### Types
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/types/list` | List all fully qualified type names |
| GET | `/api/types?typeNames=Namespace.TypeName` | Get type details |

#### Search
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/search?pattern=<regex>&scope=all&limit=100` | Regex pattern search |
| GET | `/api/search/keywords?keywords=<terms>&scope=all` | Full-text keyword search |
| GET | `/api/search/index/stats` | Get search index statistics |
| POST | `/api/search/index/rebuild` | Rebuild search index |

#### Load
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/load?path=<dll-path>` | Load a DLL at runtime |

### Search Scopes

Both search endpoints support filtering by scope:
- `all` (default) - Search everything
- `types` - Types only
- `methods` - Methods only
- `properties` - Properties only
- `fields` - Fields only
- `enums` - Enum values only

### API Examples

```bash
# Get assembly info
curl http://localhost:5000/api/info

# List all namespaces
curl http://localhost:5000/api/namespaces/list

# Get type details
curl "http://localhost:5000/api/types?typeNames=MyNamespace.MyClass"

# Search for types matching a pattern
curl "http://localhost:5000/api/search?pattern=.*Service.*&scope=types"

# Keyword search
curl "http://localhost:5000/api/search/keywords?keywords=user+auth&scope=methods"
```

## Running the MCP Server

The MCP server communicates via stdio and is designed for integration with AI assistants like Claude.

### CLI Usage

```bash
McpNetDll.exe <dll-path> [additional-dlls...] [--json-format]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `<dll-path>` | Yes | Path to the .NET DLL to analyze |
| `[additional-dlls]` | No | Additional DLL paths (space-separated) |
| `--json-format` | No | Use JSON output instead of AI-optimized format |

### Examples

```bash
# Analyze a single DLL
./publish/mcp/McpNetDll.exe "C:\Libraries\MyLibrary.dll"

# Analyze multiple DLLs
./publish/mcp/McpNetDll.exe "C:\Libs\Core.dll" "C:\Libs\Data.dll" "C:\Libs\Api.dll"

# Use JSON format output
./publish/mcp/McpNetDll.exe "C:\Libraries\MyLibrary.dll" --json-format

# Using environment variable for JSON format
set MCP_JSON_FORMAT=true
./publish/mcp/McpNetDll.exe "C:\Libraries\MyLibrary.dll"
```

### Configure with Claude Desktop

Add to your Claude Desktop configuration (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "dotnet-reflector": {
      "type": "stdio",
      "command": "C:/path/to/publish/mcp/McpNetDll.exe",
      "args": ["C:/path/to/your/library.dll"]
    }
  }
}
```

For multiple DLLs:

```json
{
  "mcpServers": {
    "dotnet-reflector": {
      "type": "stdio",
      "command": "C:/path/to/publish/mcp/McpNetDll.exe",
      "args": [
        "C:/path/to/first.dll",
        "C:/path/to/second.dll",
        "C:/path/to/third.dll"
      ]
    }
  }
}
```

### MCP Tools Available

| Tool | Description |
|------|-------------|
| `ListNamespaces` | Browse loaded assemblies and namespaces |
| `GetTypeDetails` | Get complete type information (methods, properties, fields) |
| `SearchElements` | Regex pattern search across types |
| `SearchByKeywords` | Full-text keyword search with OR/AND logic |

## Development

### Build Commands

```bash
dotnet build                           # Build all projects
dotnet test McpNetDll.Tests            # Run all tests
dotnet test McpNetDll.Tests --filter "FullyQualifiedName~TestName"  # Run specific test
dotnet run --project McpNetDll.Web     # Run web API (dev mode)
dotnet run --project McpNetDll -- "path/to/dll"  # Run MCP server (dev mode)
```

### Project Structure

```
McpNetDll/
├── McpNetDll/           # MCP Server console app
├── McpNetDll.Core/      # Core library (business logic)
├── McpNetDll.Web/       # ASP.NET Core Web API
├── McpNetDll.Tests/     # xUnit tests
└── MyTestLibrary/       # Test fixture DLL
```

### Architecture

```
MCP Tools / Web Endpoints
        │
        ▼
Response Formatters (IMcpResponseFormatter)
        │
        ▼
Query Layer (IMetadataRepository)
        │
        ▼
Indexing Service (IIndexingService) ◄──► Lucene.NET
        │
        ▼
Registry Layer (ITypeRegistry)
        │
        ▼
dnlib (IL reflection)
```

## Requirements

- .NET 9.0 SDK
- Windows, Linux, or macOS

## License

See LICENSE file for details.
