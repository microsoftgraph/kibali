# Contributing to Kibali

Thank you for your interest in contributing to Kibali! This document describes how to set up your environment and land changes.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Git

## Getting Started

```shell
git clone https://github.com/chegeapollo11/kibali.git
cd kibali
dotnet build
```

## Build

From the repository root:

```shell
dotnet build
```

## Test

```shell
dotnet test
```

Tests use xunit and are located in `test/kibaliTests/`.

## Build Topology

The solution (`kibali.sln`) contains three projects with the following dependency graph:

```
kibali.sln
  |-- src/kibali/Kibali.csproj          (Library)  - Core permissions model and logic
  |-- src/kibaliTool/KibaliTool.csproj  (Exe/CLI)  - CLI tool, depends on Kibali
  |-- test/kibaliTests/KibaliTests.csproj (Tests)  - Unit tests (xunit), depends on Kibali
```

| Project | Path | Output Type | Dependencies |
|---------|------|-------------|-------------|
| **Kibali** | `src/kibali/Kibali.csproj` | Library (NuGet: `Microsoft.Graph.Kibali`) | `Microsoft.OpenApi` |
| **KibaliTool** | `src/kibaliTool/KibaliTool.csproj` | Exe (dotnet tool: `kibali`) | Kibali, `System.CommandLine`, `Microsoft.OpenApi.Readers` |
| **KibaliTests** | `test/kibaliTests/KibaliTests.csproj` | Test (xunit) | Kibali |

<!-- Source: kibali.sln, ProjectReference links in each .csproj -->

Running `dotnet build` at the repo root builds all three projects via the solution file. NuGet packages are output to `./artifacts`.

## Project Structure

See [repo-context.md](repo-context.md) for full architecture and module details.

## Contribution Workflow

1. **Fork and branch** - Create a feature branch from `main`.
2. **Make changes** - Implement your fix or feature.
3. **Build and test** - Run `dotnet build` and `dotnet test` to verify no regressions.
4. **Submit a PR** - Open a pull request against `main`. The CI pipeline (GitHub Actions) will run build and test automatically.
5. **Code review** - PRs are reviewed by `@microsoftgraph/msgraph-devx-api-write`.

## CI/CD

The repository uses GitHub Actions (`.github/workflows/ci-cd.yml`). On every push and pull request:

- Projects are built in Release configuration.
- Unit tests are executed.

## Common Pitfalls

- The CLI tool project references the core library via `<ProjectReference>`. Ensure the solution builds successfully before testing the tool in isolation.
- `System.CommandLine` is at a beta version (2.0.0-beta4); its API may differ from stable documentation.

## Code of Conduct

This project follows the [Microsoft Open Source Code of Conduct](CODE_OF_CONDUCT.md).
