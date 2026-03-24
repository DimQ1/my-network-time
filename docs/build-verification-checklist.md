# Build Verification Checklist

## Automated verification completed in `codex/stage-05-hardening`

- [x] `dotnet build`
- [x] `MyNetworkTime.Core.Tests` test suite
- [x] Dashboard live-refresh changes compile cleanly
- [x] Settings drag-and-drop reorder changes compile cleanly

## Windows checklist

- [ ] Launch the app on `net10.0-windows10.0.19041.0`
- [ ] Confirm `Dashboard` updates `Time` and `Next Attempt` without manual reload
- [ ] Confirm `Update Now` refreshes `Current Status` and `Individual Time Servers`
- [ ] Confirm `Settings` lets you drag rows to change server priority
- [ ] Confirm saved server order persists after app restart
- [ ] Confirm `Adjust System Time` works when the app is elevated
- [ ] Confirm `Open system time settings` opens the OS settings page

## Android checklist

- [ ] Launch the app on `net10.0-android`
- [ ] Confirm dashboard refresh works while the app stays active
- [ ] Confirm `Update Now` refreshes server rows without page navigation
- [ ] Confirm `Settings` reorder UX behaves correctly in the Blazor WebView
- [ ] Confirm saved server order persists after app restart
- [ ] Confirm platform guidance opens Android date and time settings
- [ ] Confirm the app remains in monitor mode without direct clock adjustment

## iOS checklist

- [ ] Launch the app on `net10.0-ios` from a paired Mac host
- [ ] Confirm dashboard refresh works while the app stays active
- [ ] Confirm `Update Now` refreshes server rows without page navigation
- [ ] Confirm `Settings` reorder UX behaves correctly in the Blazor WebView
- [ ] Confirm saved server order persists after app restart
- [ ] Confirm platform guidance is clear when direct system settings shortcuts are unavailable
- [ ] Confirm the app remains in monitor mode without direct clock adjustment

## Release readiness sign-off

- [ ] README reviewed
- [ ] Release notes reviewed
- [ ] App icon and splash branding reviewed
- [ ] Windows smoke test passed
- [ ] Android smoke test passed
- [ ] iOS smoke test passed
