# .NET DLL Metadata Extractor for MCP

This tool provides a powerful and accurate way to perform **.NET DLL metadata extraction**, designed to be used with AI agents and tools that implement the **Model Context Protocol (MCP)**. It allows for deep **reverse engineering** of .NET assemblies (`.dll` files) by extracting detailed information about namespaces, types, methods, and properties, without relying on source code.

This capability is crucial for AI models that need to understand and interact with compiled .NET libraries, enabling them to analyze public APIs, check for breaking changes, or generate compatible code.

## Key Features

-   **Comprehensive Metadata Extraction**: Extracts a wide range of .NET types, including `class`, `struct`, `enum`, `interface`, `delegate`, `static class`, `abstract class`, and more.
-   **Public API Analysis**: Focuses on the public surface of an assembly, just as a developer would see it.
-   **Detailed Type Information**: Provides details on methods, properties, fields, and their signatures.
-   **Struct Layout Inspection**: Retrieves memory layout information for structs, including packing and field offsets.
-   **MCP Ready**: Designed to be integrated as a tool in an MCP ecosystem, allowing AI agents to query DLLs directly.

## How to Set Up

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd McpNetDll
    ```

2.  **Build the project:**
    You will need the .NET SDK installed. Run the following command from the root directory to build the solution:
    ```bash
    dotnet build
    ```
    This will compile the `McpNetDll` tool and the associated test library.

## How to Use (Exposed Methods)

The core logic is contained within the `Extractor` class in `McpNetDll/MetadataExtractor.cs`. It exposes two primary methods for metadata extraction.

### 1. `ListNamespaces(string assemblyPath, string[]? namespaces = null)`

This method lists all namespaces within a given DLL or, if specified, provides details on the types within a filtered set of namespaces.

**Parameters:**
-   `assemblyPath` (string): The absolute file path to the .NET assembly (`.dll`).
-   `namespaces` (string[]?, optional): An array of namespace names to filter by. If null or empty, it returns a list of all namespaces in the assembly.

**Example Usage:**
```csharp
using McpNetDll;

var extractor = new Extractor();
var dllPath = "path/to/your/library.dll";

// List all namespaces
string allNamespacesJson = extractor.ListNamespaces(dllPath);
Console.WriteLine(allNamespacesJson);

// Get types within a specific namespace
string[] namespaceFilter = { "MyLibrary.Features" };
string filteredTypesJson = extractor.ListNamespaces(dllPath, namespaceFilter);
Console.WriteLine(filteredTypesJson);
```

### 2. `GetTypeDetails(string assemblyPath, string[] typeNames)`

This method retrieves detailed information for one or more specified types, including their methods, properties, fields, and enum values.

**Parameters:**
-   `assemblyPath` (string): The absolute file path to the .NET assembly (`.dll`).
-   `typeNames` (string[]): An array of full or simple type names (e.g., `MyNamespace.MyClass` or `MyClass`).

**Example Usage:**
```csharp
using McpNetDll;

var extractor = new Extractor();
var dllPath = "path/to/your/library.dll";
string[] typesToInspect = { "MyLibrary.MyPublicClass", "MyEnum" };

string typeDetailsJson = extractor.GetTypeDetails(dllPath, typesToInspect);
Console.WriteLine(typeDetailsJson);
```

This project empowers developers and AI agents to programmatically understand and interact with the vast ecosystem of .NET libraries through powerful **.NET DLL metadata extraction** and **reverse engineering** techniques, all seamlessly integrated via the **Model Context Protocol (MCP)**.