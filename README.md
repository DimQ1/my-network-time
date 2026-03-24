# My Network Time

`My Network Time` is a `.NET 10` `MAUI + Blazor Hybrid` application inspired by NetTime. It combines a shared sync core with platform-aware behavior for querying network time servers, showing drift, and applying or guiding time updates where the platform allows it.

## Stage 5 status

Stage 5 hardening is complete in `codex/stage-05-hardening` with:

- interactive dashboard refresh for `Current Status` and `Individual Time Servers`
- drag-and-drop reordering for `Time Servers` in `Settings`
- basic app branding assets for the icon and splash screen
- release-facing documentation and verification checklists

## Solution layout

- `src/MyNetworkTime.App` - MAUI host, Blazor UI, platform services, branded assets
- `src/MyNetworkTime.Core` - protocols, sync orchestration, dashboard models, storage
- `tests/MyNetworkTime.Core.Tests` - xUnit coverage for core protocol and policy behavior
- `docs/implementation-plan.md` - staged delivery roadmap
- `docs/build-verification-checklist.md` - build and smoke-test checklist
- `docs/release-notes-stage-05.md` - release notes and support boundaries

## Supported platform behavior

| Platform | Query time servers | Live dashboard refresh | Open date/time settings | Direct clock adjustment |
| --- | --- | --- | --- | --- |
| Windows | Yes | Yes | Yes | Yes, when elevated |
| Android | Yes | Yes while app is active | Yes | No |
| iOS | Yes | Yes while app is active | Manual guidance only | No |

## Settings behavior

The `Settings` page supports up to five configured servers.

- host, protocol, and port are editable per row
- rows can be reordered by dragging the `Drag` handle
- the saved order becomes the sync priority order used by the coordinator

## Prerequisites

- `.NET 10 SDK`
- Visual Studio 2026 with `.NET MAUI` workload
- Android SDK for Android builds
- a Mac build host for iOS builds

## Build

From the repository root:

- `dotnet build`
- `dotnet test tests/MyNetworkTime.Core.Tests/MyNetworkTime.Core.Tests.csproj`

Platform-specific examples:

- `dotnet build src/MyNetworkTime.App/MyNetworkTime.App.csproj -f net10.0-android`
- `dotnet build src/MyNetworkTime.App/MyNetworkTime.App.csproj -f net10.0-windows10.0.19041.0`

For `iOS`, build from Visual Studio paired to a Mac host.

## Run

Open the solution in Visual Studio and choose a target platform:

- `Windows Machine` for desktop validation
- `Android Emulator` or a connected Android device
- `iOS Simulator` through a paired Mac host

At startup the app initializes local storage, starts the lifecycle sync loop, and populates the dashboard from persisted state.

## Tests

Current automated coverage is focused on the shared core:

- protocol parsing
- server ordering and demotion rules
- interval conversions
- time adjustment policy
- JSON repositories

Run the suite with:

- `dotnet test tests/MyNetworkTime.Core.Tests/MyNetworkTime.Core.Tests.csproj`

## Notes

- Direct system time changes are intentionally limited by platform capabilities.
- Dashboard status cards update live while the app is active.
- Background refresh behavior is foreground-app scoped on mobile platforms.
