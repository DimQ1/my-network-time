# Stage 5 Release Notes

## Highlights

Stage 5 finalizes release hardening for `My Network Time`.

### New in this stage

- live dashboard refresh for `Current Status`
- live dashboard refresh for `Individual Time Servers`
- drag-and-drop reordering for `Time Servers` in `Settings`
- branded app icon and splash screen assets
- release documentation and verification checklist

## Supported features by platform

### Windows

Supported:

- querying configured time servers
- scheduled refresh while the app is active
- interactive dashboard updates after sync activity
- opening Windows date and time settings
- direct system clock adjustment when the app is running with the required privileges

Not supported or constrained:

- direct system clock adjustment without elevation

### Android

Supported:

- querying configured time servers
- scheduled refresh while the app is active
- interactive dashboard updates after sync activity
- opening Android date and time settings
- settings persistence and server-priority reordering

Not supported or constrained:

- direct system clock adjustment for a normal app
- unrestricted background execution outside platform limits

### iOS

Supported:

- querying configured time servers
- scheduled refresh while the app is active
- interactive dashboard updates after sync activity
- settings persistence and server-priority reordering
- in-app guidance for manual date and time changes

Not supported or constrained:

- direct system clock adjustment through public APIs
- guaranteed shortcut into the system date and time page
- unrestricted background execution outside platform limits

## Upgrade and migration notes

- Existing settings continue to load from JSON storage.
- Saved server order now directly defines the sync priority order.
- No storage migration is required for Stage 5.

## Validation summary

Validated during implementation:

- workspace build succeeded
- `MyNetworkTime.Core.Tests` passed

Manual verification still required:

- device and simulator smoke tests for Windows, Android, and iOS
- UX confirmation for drag reorder inside each target platform WebView host
