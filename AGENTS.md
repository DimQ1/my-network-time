# AGENTS.md — My Network Time

This file provides guidance to AI coding agents working in this repository.
See [.github/copilot-instructions.md](.github/copilot-instructions.md) for the canonical instructions file.
Both files are kept in sync; the `.github/copilot-instructions.md` file is the authoritative source.

## Quick reference for agents

- **Build**: `dotnet build` (or `Ctrl+Shift+B` in VS Code)
- **Test**: `dotnet test tests/MyNetworkTime.Core.Tests/MyNetworkTime.Core.Tests.csproj`
- **Target frameworks**: `net10.0`, `net10.0-android`, `net10.0-windows10.0.19041.0`, `net10.0-ios`
- **Service entry point**: `INetworkTimeWorkspaceService` in `src/MyNetworkTime.Core/Services/`
- **DI registration**: `src/MyNetworkTime.App/AppServiceCollectionExtensions.cs`
- **Test conventions**: xUnit `[Fact]`/`[Theory]`, fakes in `tests/.../Support/`, behavioral naming
- **Platform code**: behind interfaces in `MyNetworkTime.Core.Platforms`, implementations in `MyNetworkTime.App/Services/`
- **Coding style**: file-scoped namespaces, record-based snapshots, `ValueTask` for async, `CancellationToken` flowing through boundaries
