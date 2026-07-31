# Repo Context - Kibali

## Repo at a Glance

| Attribute | Value |
|-----------|-------|
| Primary Language | C# |
| Framework | .NET 8.0 |
| Build System | dotnet CLI / MSBuild |
| Test Framework | xunit 2.9 |
| Package Manager | NuGet |
| Source Control | Git (GitHub) |
| CI/CD | GitHub Actions (`ci-cd.yml`) |
| License | MIT |
| NuGet Package ID | `Microsoft.Graph.Kibali` / `Microsoft.Graph.KibaliTool` |
| Current Version | 0.27.0 |

## Top-Level Directory Map

```
kibali/
|-- src/
|   |-- kibali/          # Core library (NuGet package)
|   `-- kibaliTool/      # CLI tool (dotnet tool)
|-- test/
|   `-- kibaliTests/     # Unit tests (xunit)
|-- specs/               # Permission schema specs (JSON Schema, CDDL)
|-- .github/             # CI workflows, CODEOWNERS, Dependabot
|-- .azure-pipelines/    # Azure Pipelines config
|-- kibali.sln           # Solution file
`-- permissions.csv      # Legacy permissions CSV
```

## Source Code Structure

### `src/kibali` - Core Library

The Kibali library provides an in-memory model and operations for authorization (AuthZ) permission documents used by Microsoft Graph.

| File | Purpose |
|------|---------|
| `PermissionsDocument.cs` | Root document model - load/write JSON permission files |
| `Permission.cs` | Single permission entry model |
| `ProtectedResource.cs` | Maps URL paths to required permissions |
| `AuthZChecker.cs` | Resolves which permissions apply to a given URL/method using an OpenAPI URL tree |
| `PathSet.cs` | Represents sets of URL path patterns |
| `AcceptableClaim.cs` | Models a claim that satisfies a permission requirement |
| `Scheme.cs` | Authentication scheme model |
| `PermissionsDeployment.cs` | Deployment-specific permission configuration |
| `ProvisioningInfo.cs` | Provisioning metadata for permissions |
| `OwnerInfo.cs` | Ownership metadata |
| `CsdlExporter.cs` | Exports permissions as CSDL annotations |
| `PermissionsStubGenerator.cs` | Generates permission stub files |
| `TableGenerator.cs` | Generates tabular permission documentation |
| `MarkdownBuilder.cs` | Utility for constructing markdown output |
| `PermissionsError.cs` | Validation error model |
| `ParsingHelpers.cs` | JSON parsing utilities |
| `StringConstants.cs` | Shared string literals |

### `src/kibaliTool` - CLI Tool

A .NET global tool (`kibali`) with subcommands for managing permission files:

| Command | Purpose |
|---------|---------|
| `import` | Imports permissions from Graph Explorer metadata into Kibali format |
| `query` | Queries a permissions file for URL/method combinations |
| `validate` | Validates permission files for structural correctness |
| `document` | Generates markdown documentation from permission files |
| `export` | Exports permissions (placeholder) |

### `test/kibaliTests` - Unit Tests

Tests covering permission resolution, validation, documentation generation, least-privilege lookup, and access-check logic.

### `specs/` - Permission Specifications

Formal schema definitions for the permission document format:

| File | Purpose |
|------|---------|
| `permissions.md` | Human-readable spec for permission documents |
| `permissions-schema.json` | JSON Schema for permission files |
| `permissions.cddl` | CDDL schema for permission files |
| `permissions-deployment.cddl` | CDDL schema for deployment files |
| `permissions-deployment-schema.json` | JSON Schema for deployment files |
| `permissions-deployments.md` | Deployment spec documentation |

## Key Entry Points

| Entry Point | Path | Description |
|-------------|------|-------------|
| CLI Main | `src/kibaliTool/Program.cs` | `System.CommandLine`-based CLI root |
| Library Root | `src/kibali/PermissionsDocument.cs` | Load/write permission documents |
| AuthZ Resolution | `src/kibali/AuthZChecker.cs` | Resolve permissions for a URL path |

## Dependencies and Integrations

### Internal Project References

| Project | References |
|---------|-----------|
| KibaliTool | Kibali (core library) |
| KibaliTests | Kibali (core library) |

### External NuGet Packages

| Package | Version | Used By |
|---------|---------|---------|
| `Microsoft.OpenApi` | 1.6.24 | Kibali (core) - OpenAPI model and URL tree |
| `Microsoft.OpenApi.Readers` | 1.6.24 | KibaliTool - OpenAPI document parsing |
| `System.CommandLine` | 2.0.0-beta4 | KibaliTool - CLI argument parsing |
| `xunit` | 2.9.3 | KibaliTests |
| `Microsoft.NET.Test.Sdk` | 17.13.0 | KibaliTests |

### External Service Integrations

| Integration | Purpose |
|-------------|---------|
| Microsoft Graph DevX Content (GitHub raw) | Source for Graph Explorer permission metadata during import |

## Team Ownership

| Scope | Owner |
|-------|-------|
| All files (`*`) | `@microsoftgraph/msgraph-devx-api-write` |

## Key Conventions for Agents

- The solution targets **.NET 8.0** exclusively.
- Build with `dotnet build` from the repo root; test with `dotnet test`.
- The CLI tool is packaged as a .NET global tool (`<PackAsTool>true</PackAsTool>`).
- Permission document JSON uses `System.Text.Json` with `PropertyNameCaseInsensitive` deserialization.
- URL matching uses `Microsoft.OpenApi`'s `OpenApiUrlTreeNode` for path resolution.
- The `Kibali` namespace is used for the core library; `KibaliTool` namespace for the CLI.
