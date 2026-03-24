# My Network Time Implementation Plan

## Goal

Build a `.NET 10` cross-platform application inspired by NetTime using `.NET MAUI + Blazor Hybrid` for the UI.

Target platforms:

- Windows
- Android
- iOS

Primary user scenarios:

- View current local time and last successful sync
- Query several time servers and compare offset/lag
- Run manual sync
- Configure update interval, retry interval, server list, logging, and time adjustment rules
- Review sync history and errors

## Important platform constraints

To keep the roadmap realistic, platform behavior will differ where the operating system imposes limits:

- Windows: full support for querying time servers and adjusting system time, but changing system time requires elevated privileges.
- Android: a normal store app can query time servers and calculate drift, but changing system time is generally reserved for system/privileged apps.
- iOS: the app can query time servers and present drift/status, but direct system clock changes are not expected to be possible through public APIs.

Because of that, the shared product shape is:

- Windows: monitor + sync + optional system time adjustment
- Android: monitor + manual checks + user guidance to system settings
- iOS: monitor + manual checks + user guidance to system settings

## Technical direction

Recommended template:

- `dotnet new maui-blazor`

Planned solution structure:

- `src/MyNetworkTime.App`
  - MAUI Blazor host, platform startup, native capabilities
- `src/MyNetworkTime.Core`
  - Domain models, protocol clients, sync orchestration, persistence abstractions
- `tests/MyNetworkTime.Core.Tests`
  - Protocol parsing, interval rules, failover and selection logic

Core modules:

- `Protocols`
  - `SntpClient`
  - `Rfc868TcpClient`
  - `Rfc868UdpClient`
- `Sync`
  - `SyncCoordinator`
  - `ServerSelectionPolicy`
  - `TimeAdjustmentPolicy`
- `Storage`
  - `SettingsRepository`
  - `LogRepository`
- `Platform`
  - `ITimeAdjustmentService`
  - `IAppLifecycleSyncService`
  - `IPermissionGuidanceService`

Blazor UI areas:

- Dashboard
- Settings
- Log Viewer
- About

## Delivery stages

### Stage 0

Branch:

- `codex/stage-00-plan`

Deliverables:

- Git repository initialization
- Base `.gitignore`
- This implementation plan

Exit criteria:

- Plan approved by the user

### Stage 1

Branch:

- `codex/stage-01-bootstrap`

Deliverables:

- Create solution and projects on `.NET 10`
- Scaffold `MAUI Blazor Hybrid` app
- Add `Core` and test projects
- Register dependency injection, navigation shell, and base layout
- Add initial theme tokens and responsive shell for desktop/mobile

Exit criteria:

- App builds successfully on Windows
- Android and iOS targets restore successfully
- Main screens open with placeholder content

### Stage 2

Branch:

- `codex/stage-02-time-core`

Deliverables:

- Implement `SNTP`, `RFC868 (TCP)`, and `RFC868 (UDP)` clients
- Add models for server entries, sync result, app status, log record
- Add server prioritization and demotion-after-failures logic
- Add local persistence for settings and logs
- Cover protocol and selection logic with tests

Exit criteria:

- Manual integration flow can query configured servers
- Settings persist between app launches
- Tests for core logic pass

### Stage 3

Branch:

- `codex/stage-03-ui`

Deliverables:

- Build dashboard matching the provided reference screens
- Build settings editor with protocol and interval pickers
- Build log viewer
- Add validation, loading states, error presentation, and mobile layout polish

Exit criteria:

- The main flows are usable from the UI on Windows
- Layout is responsive for phone and desktop widths

### Stage 4

Branch:

- `codex/stage-04-platform-sync`

Deliverables:

- Windows time adjustment integration with safe privilege handling
- Background or scheduled refresh strategy per platform
- Platform-specific capability banners and fallback actions
- Open system date/time settings where direct adjustment is not available

Exit criteria:

- Windows supports real system time sync
- Android and iOS provide a stable monitor mode with clear user guidance

### Stage 5

Branch:

- `codex/stage-05-hardening`

Deliverables:

- Interactive dashboard refresh for `Current Status` and `Individual Time Servers` without requiring manual page reload
- Final cleanup and documentation
- App icons and basic branding
- Build verification checklist for Windows, Android, and iOS
- Release notes for supported versus unsupported platform features

Exit criteria:

- Solution builds cleanly
- Dashboard status blocks refresh interactively after sync and scheduled updates
- README and test/run instructions are complete

## Branching and merge workflow

For every stage:

1. Create a new branch from `main`.
2. Complete the stage only inside that branch.
3. Validate the result.
4. Share the result with the user.
5. Merge into `main` only after the user confirms the stage is successful.

Branch naming convention:

- `codex/stage-00-plan`
- `codex/stage-01-bootstrap`
- `codex/stage-02-time-core`
- `codex/stage-03-ui`
- `codex/stage-04-platform-sync`
- `codex/stage-05-hardening`

## Notes before implementation

- The UI will be styled after the provided screenshots, but adapted for touch and smaller screens.
- The first production milestone should target Windows parity for actual time synchronization.
- Android and iOS will share as much UI and domain logic as possible, while platform-specific behavior will be isolated behind interfaces.
