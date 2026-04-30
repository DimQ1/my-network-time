---
name: testing
description: 'Write and run tests for My Network Time — xUnit, fakes, behavioral naming, and project-specific patterns'
---

# Testing — My Network Time

Your goal is to help me write, run, and maintain tests that follow the conventions of this project.

## Quick commands

```bash
# Build everything
dotnet build

# Run all tests
dotnet test tests/MyNetworkTime.Core.Tests/MyNetworkTime.Core.Tests.csproj

# Run tests without rebuilding (faster when code hasn't changed)
dotnet test tests/MyNetworkTime.Core.Tests/MyNetworkTime.Core.Tests.csproj --no-build
```

In VS Code, use `Ctrl+Shift+B` for the default build task, or pick from the task list: `build-tests`, `test-core`, etc.

## Test project layout

```
tests/MyNetworkTime.Core.Tests/
├── MyNetworkTime.Core.Tests.csproj   # net10.0, xUnit 2.9.x, coverlet
├── ServerSelectionPolicyTests.cs     # [Fact] only, behavioral names
├── SntpClientTests.cs                # async [Fact], ManualTimeProvider + FakeUdpTransport
├── Rfc868ClientTests.cs
├── TimeAdjustmentPolicyTests.cs      # [Fact], TimeAdjustmentAvailability fakes
├── TimeIntervalTests.cs
├── JsonRepositoryTests.cs            # IDisposable for temp-dir cleanup
└── Support/
    ├── ManualTimeProvider.cs         # TimeProvider subclass for deterministic time
    └── FakeTransports.cs             # FakeUdpTransport, FakeTcpTransport
```

## Test conventions (project-specific)

### Naming
- **Behavioral naming**: `MethodName_Scenario_ExpectedBehavior`
  - ✅ `OrderServers_PushesDemotedServersToTheEnd`
  - ✅ `Evaluate_ReturnsAutoAdjust_WhenModeIsAutomatic_OffsetExceedsThreshold_AndAdjustmentIsAvailable`
  - ✅ `QueryAsync_ParsesOffsetAndRoundTripFromSntpPacket`
- Use `[Fact]` for unit tests, `[Theory]` with `[InlineData]` for data-driven tests.
- No `[Trait]` attributes — not used in this codebase.

### Structure
- **Arrange/Act/Assert** pattern in every test.
- Test classes are `public sealed class`.
- Use file-scoped namespaces (`namespace MyNetworkTime.Core.Tests;`).
- No base test class — use `IDisposable` for cleanup when needed (see `JsonRepositoryTests`).
- No `IClassFixture` or `ICollectionFixture` — not used here.

### Fakes (in `tests/MyNetworkTime.Core.Tests/Support/`)
- **`ManualTimeProvider(DateTimeOffset utcNow)`**: Subclass of `System.TimeProvider`. Provides deterministic UTC time. Call `Advance(TimeSpan)` to move time forward.
- **`FakeUdpTransport(Func<...> handler)`**: Implements `IUdpTransport`. The handler receives `(host, port, payload, timeout)` and returns a `UdpTransportResponse`. Exposes `LastPayload` for assertion.
- **`FakeTcpTransport(Func<...> handler)`**: Same pattern as `FakeUdpTransport` but for `ITcpTransport`.
- When testing protocol clients (SNTP, RFC 868), use `ManualTimeProvider` + the appropriate fake transport. Do NOT use real network calls.

### Assertions
- Use `Assert.Equal` for value comparisons.
- Use `Assert.InRange` for time-range assertions (offsets, delays).
- Use `Assert.Collection` for ordered collection assertions.
- Use `Assert.True(File.Exists(...))` for file-system side effects.
- No fluent assertion library — stick to xUnit built-in assertions.

### Async tests
- Return `async Task` (not `async void`).
- Use `await` for all async calls — never `.Result` or `.Wait()`.
- Fake transports return `ValueTask.FromResult(...)` for synchronous completion.

### Data-driven tests
- Use `[Theory]` + `[InlineData]` when testing multiple input variations.
- Use `AppSettingsDefaults.Create() with { ... }` to create settings snapshots with specific overrides — this is the project's pattern for test data.

### Cleanup
- If a test creates temp files/directories, implement `IDisposable` and clean up in `Dispose()`.
- Use `Path.Combine(Path.GetTempPath(), $"mynetworktime-tests-{Guid.NewGuid():N}")` for temp directories.

## When adding a new test

1. **Identify the class under test** in `src/MyNetworkTime.Core/`.
2. **Create a test class** named `{ClassName}Tests` in `tests/MyNetworkTime.Core.Tests/`.
3. **Determine dependencies**: Does it need time? → `ManualTimeProvider`. Network? → `FakeUdpTransport`/`FakeTcpTransport`. File system? → temp directory + `IDisposable`. Settings? → `AppSettingsDefaults.Create() with { ... }`.
4. **Write the test** following Arrange/Act/Assert with behavioral naming.
5. **Run**: `dotnet test tests/MyNetworkTime.Core.Tests/MyNetworkTime.Core.Tests.csproj`
6. **Verify no regressions**: run the full suite before committing.

## When changing production code

- If you change behavior in `src/MyNetworkTime.Core/`, add or update tests in `tests/MyNetworkTime.Core.Tests/` in the same commit.
- Run the full test suite after changes — never skip this step.
- If a test fails, fix the test OR the production code — don't leave failing tests.

## Anti-patterns (do NOT do these)

- ❌ Real network calls in tests (use fakes)
- ❌ `DateTime.Now` / `DateTimeOffset.Now` in tests (use `ManualTimeProvider`)
- ❌ `Task.Delay` or `Thread.Sleep` in tests (use `ManualTimeProvider.Advance`)
- ❌ Multiple behaviors in one test method
- ❌ Test interdependencies (tests must run in any order)
- ❌ `async void` test methods (use `async Task`)
- ❌ Skipping test runs before committing
