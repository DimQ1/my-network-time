# Copilot Instructions for `My Network Time`

## Big picture architecture
- This solution is a `.NET 10` MAUI Blazor Hybrid app split into:
  - `src/MyNetworkTime.App`: UI, MAUI host, platform services, app storage wiring
  - `src/MyNetworkTime.Core`: protocols, sync orchestration, domain models, persistence abstractions
  - `tests/MyNetworkTime.Core.Tests`: xUnit tests for core behavior
- Service registration is centralized in `src/MyNetworkTime.App/AppServiceCollectionExtensions.cs`.
- UI pages should call `INetworkTimeWorkspaceService` (`src/MyNetworkTime.Core/Services/INetworkTimeWorkspaceService.cs`) instead of calling repositories/coordinator directly.

## Runtime data flow you should preserve
- App startup (`src/MyNetworkTime.App/App.xaml.cs`) starts `IAppLifecycleSyncService` immediately.
- Scheduled sync loop is in `src/MyNetworkTime.App/Services/AppLifecycleSyncService.cs` (30s ticker + `SyncTrigger` selection + single-flight gate).
- Core refresh logic lives in `src/MyNetworkTime.Core/Sync/SyncCoordinator.cs`:
  - orders servers via `ServerSelectionPolicy`
  - queries protocol clients via `NetworkTimeProtocolClientResolver`
  - persists state via `ISyncStateRepository`
  - appends logs via `ILogRepository`
- Dashboard UI updates are event-driven via `DashboardRefreshNotifier` and local ticking in `Dashboard.razor`.

## Settings + server ordering conventions
- Settings editor remains bounded to exactly 5 slots (see `SettingsEditorModel.ServerCount`).
- Reordering servers must use `SettingsEditorModel.MoveServer(...)` so persistence order maps to sync priority order.
- In MAUI WebView, prefer pointer-based reorder bridge (`src/MyNetworkTime.App/wwwroot/settings-drag.js`) over HTML5 drag/drop.
- Keep all settings notifications/validation grouped under the `Time Servers` section in `Settings.razor`.

## Persistence and platform boundaries
- Local files are under `FileSystem.AppDataDirectory/MyNetworkTime` (`AppStoragePaths.cs`).
- JSON persistence is guarded by `SemaphoreSlim` in `JsonFileStore<T>`; keep this thread-safety behavior.
- Keep platform-specific behavior behind interfaces in `MyNetworkTime.Core.Platforms` (`ITimeAdjustmentService`, `IPermissionGuidanceService`, `IAppLifecycleSyncService`).

## Build/test workflow
- Restore/build from repo root: `dotnet build`
- Run tests: `dotnet test tests/MyNetworkTime.Core.Tests/MyNetworkTime.Core.Tests.csproj`
- Common app targets (`MyNetworkTime.App.csproj`):
  - `net10.0-android`
  - `net10.0-windows10.0.19041.0`
  - `net10.0-ios` (requires paired Mac host)

## Project-specific coding patterns
- Use file-scoped namespaces and record-based snapshots (e.g., `DashboardSnapshot`, `SyncStateSnapshot`).
- Keep async/cancellation flowing through service boundaries (`ValueTask` is used heavily in this codebase).
- Prefer extending existing services/models over adding new abstraction layers.
- When changing sync or selection behavior, update/add xUnit tests in `tests/MyNetworkTime.Core.Tests` first-class (behavioral naming like `OrderServers_PushesDemotedServersToTheEnd`).
