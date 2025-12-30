# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **.NET 9.0 DLL Reflector** - a tool for analyzing and extracting metadata from .NET assemblies. It provides:
- **MCP Server** - Model Context Protocol tools for AI assistants to introspect .NET assemblies
- **Web API** - ASP.NET Core REST API for programmatic access
- **Full-text Search** - Lucene.NET-powered keyword search across all assembly elements

## Build Commands

```bash
dotnet build                           # Build all projects
dotnet test McpNetDll.Tests            # Run all unit tests
dotnet test McpNetDll.Tests --filter "FullyQualifiedName~TestName"  # Run single test
dotnet run --project McpNetDll.Web     # Run web API server
dotnet run --project McpNetDll         # Run MCP server
dotnet publish McpNetDll -c Release    # Publish MCP server for distribution
```

## Architecture

```
MCP Tools / Web Endpoints
        ↓
Response Formatters (IMcpResponseFormatter)
        ↓
Query Layer (IMetadataRepository)
        ↓
Indexing Service (IIndexingService) ←→ Lucene.NET
        ↓
Registry Layer (ITypeRegistry)
        ↓
dnlib (IL reflection)
```

### Project Structure

| Project | Purpose |
|---------|---------|
| `McpNetDll/` | MCP Server console app - entry point and tool definitions |
| `McpNetDll.Core/` | Core library - all business logic, models, services |
| `McpNetDll.Web/` | ASP.NET Core Web API with REST endpoints |
| `McpNetDll.Tests/` | xUnit tests |
| `MyTestLibrary/` | Test fixture DLL for unit tests |

### Core Components (McpNetDll.Core/)

- **Registry/** - `TypeRegistry` loads DLLs via dnlib, indexes types by namespace/name
- **Repository/** - `MetadataRepository` provides query abstraction with pagination
- **Indexing/** - `LuceneIndexingService` full-text search with OR/AND keyword logic
- **Helpers/** - Formatters (`AiConsumptionFormatter` for AI-optimized output, `McpResponseFormatter` for JSON)

### Key Interfaces

- `ITypeRegistry` - Type metadata collection and lookup
- `IMetadataRepository` - Query abstraction (namespaces, type details, search)
- `IIndexingService` - Full-text keyword search
- `IMcpResponseFormatter` - Output formatting (AI-compact vs JSON)

## Code Conventions

- All metadata uses **C# records** (immutable)
- **Nullable reference types** enabled project-wide
- Nested types use `+` notation: `ParentType+NestedType`
- DI registration in `ServiceCollectionExtensions.cs`
- MCP server logs to stderr (stdout reserved for protocol)

## MCP Tools

The MCP server exposes four tools:
1. `ListNamespaces` - Browse loaded assemblies and namespaces
2. `GetTypeDetails` - Get complete type information (methods, properties, fields)
3. `SearchElements` - Regex pattern search across types
4. `SearchByKeywords` - Full-text keyword search with `keyword_or`/`keyword_and` parameters

## Configuration Examples

- `claude_desktop_config_example.json` - Claude Desktop MCP server registration
- `codex_config_example.toml` - OpenAI Codex CLI configuration
