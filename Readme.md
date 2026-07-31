# Kibali Permissions Tooling

This repository contains a library and a commandline tool for managing AuthZ permissions.  This was built to 
help manage permissions for Microsoft Graph.


See [CONTRIBUTING.md](CONTRIBUTING.md) for setup, build, test, and usage instructions.

From the repo root you can build the kibali tool using the following command:

```shell
dotnet build
```

You can create the Kibali permissions file from the Graph Explorer permissions metadata data using the following command:

```shell
.\kibaliTool\bin\Debug\net8.0\KibaliTool.exe import
```

This command will output a file called GraphPermissions.json in the `.\output` folder. Once you have this file you
can query the file for permissions using the following command:

```shell
.\kibaliTool\bin\Debug\net8.0\KibaliTool.exe query --pf .\output\GraphPermissions.json --url "/me/messages"
```

## Documentation

| Area | Link | Description |
|------|------|-------------|
| Contributing | [CONTRIBUTING.md](CONTRIBUTING.md) | Setup, build, test, and contribution guidelines |
| AI Agents | [AGENTS.md](AGENTS.md) | Copilot and AI agent usage guidance |
| Repository Context | [repo-context.md](repo-context.md) | Architecture overview and codebase map |
| Copilot Instructions | [.github/copilot-instructions.md](.github/copilot-instructions.md) | Repository-specific Copilot configuration |
| PR Template | [.github/pull_request_template.md](.github/pull_request_template.md) | Pull request submission template |
| Code of Conduct | [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) | Community code of conduct |
| Permissions Spec | [specs/permissions.md](specs/permissions.md) | Permissions data model specification |
| Deployment Spec | [specs/permissions-deployments.md](specs/permissions-deployments.md) | Permissions deployment specification |
