# AGENTS.md

## Build & Test

See `CONTRIBUTING.md` for build, test, and verify commands.

### Verify

Run all tests from the repository root:

```shell
dotnet test
```

<!-- Source: CONTRIBUTING.md, kibali.sln -->

### Scoped Verify

To build and test only the unit-test project:

```shell
dotnet test test/kibaliTests/KibaliTests.csproj
```

<!-- Source: test/kibaliTests/KibaliTests.csproj -->

### Run a Single Test

The test project uses **xunit**. To run a single test by name:

```shell
dotnet test test/kibaliTests/KibaliTests.csproj --filter "FullyQualifiedName~TestName"
```

Replace `TestName` with the method name (or a substring) of the test to run. Examples:

```shell
# Run all tests in a specific class
dotnet test test/kibaliTests/KibaliTests.csproj --filter "FullyQualifiedName~CanAccessTests"

# Run a single test method
dotnet test test/kibaliTests/KibaliTests.csproj --filter "FullyQualifiedName~PositivePermissionMatch"
```

<!-- Source: xunit PackageReference in test/kibaliTests/KibaliTests.csproj -->

## Environment Constraints

See `CONTRIBUTING.md` for toolchain and runtime requirements.

## Operational Context

Before answering questions about this codebase, read these files for context:
- `repo-context.md` - System structure, modules, dependencies, key conventions
- `CONTRIBUTING.md` - Setup, contribution workflow, common pitfalls

## Safety Guardrails

### Rules
- **NEVER** delete or overwrite production data, databases, or deployed resources
- **NEVER** force push, reset --hard, or rewrite shared branch history
- **NEVER** deploy or publish without explicit user approval
- **NEVER** modify CI/CD pipeline security gates or SDL suppressions
- **NEVER** inline secrets, tokens, passwords, or connection strings in code
- **NEVER** commit `.env` files, certificates, or key files

## AI Governance

### Attribution
- Use `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` for AI-assisted commits

### Autonomy Boundaries
- [REPLACE: Define what actions AI may take autonomously vs. what requires human approval. Example: AI may fix build errors and add tests autonomously; AI must get human approval for dependency upgrades, public API changes, and CI/CD modifications.]

### Human Review Requirements
- [REPLACE: Define sensitive paths requiring human review. Example: .github/workflows/, .azure-pipelines/, src/kibali/AuthZChecker.cs]

## PR Workflow

When creating pull requests, read and follow the PR template at `.github/pull_request_template.md`. Fill in all applicable sections before submitting. If a section does not apply, mark it N/A rather than removing it.
