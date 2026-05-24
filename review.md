# JukeBox Code Review

## Critical Priority

- [x] **Resource leak in `SongPlayer` — `Mp3FileReader` and `WaveOutEvent` are never disposed**
  `SongPlayer.cs` creates new `Mp3FileReader` and `WaveOutEvent` instances on every call to `PlaySong()` but never disposes them. The `CheckForPlayingSong()` method calls `Stop()` but has a TODO acknowledging disposal is missing. Over time this leaks unmanaged audio handles and file locks, eventually crashing the app or locking mp3 files. `SongPlayer` itself should implement `IDisposable` and dispose both objects in `CheckForPlayingSong()` and on shutdown.

- [x] **Hardcoded file paths in `SongSources` make the app non-portable**
  `SongSources.Init()` contains machine-specific paths (`E:\iTunes Music\`, `C:\Users\rober\Music\`). On any other machine these won't exist, causing silent failures at startup. The TODO in the code acknowledges this. Move these to a configuration file (`appsettings.json` or similar) so the app works without code changes on different machines.

- [x] **`FileSystemParser.ExtensionsOk()` uses `Contains` instead of `EndsWith`**
  A file named `something.mp3.bak` or a folder path containing `.mp3` in a directory name would incorrectly pass the extension check. Use `Path.GetExtension()` or `EndsWith` with `StringComparison.OrdinalIgnoreCase` for correctness. Note that `SongSources.CountSongsInSource()` already does this correctly with `EndsWith`, making the inconsistency more confusing.

- [x] **Extension mismatch between `FileSystemParser` and `SongSources`**
  `SongSources.CountSongsInSource()` counts both `.mp3` and `.m4a` files, but `FileSystemParser.ExtensionsOk()` only accepts `.mp3`. This means the startup count reports songs that can never actually be found or played, misleading the user.

## High Priority

- [x] **`while (true)` loop in `JukeboxEngine.LetTheMusicPlay()` has no exit path**
  There is no way to gracefully quit the application. The user must kill the process. Add a quit command (e.g. typing "q" or pressing Escape) that breaks the loop and disposes resources cleanly.

- [x] **`Song.FileName` uses hardcoded `\\` path separator**
  `Song.cs` splits on `'\\'` which won't work on non-Windows platforms. Use `Path.GetFileName()` instead for cross-platform correctness, especially since .NET 10 supports multiple OS targets.

- [x] **`Display.Write()` calls `BrightColours.ToList().Count` on every character iteration**
  Inside the inner `foreach` loop over each character, `BrightColours.ToList().Count` allocates a new list every iteration just to get the count. Use `BrightColours.Count` directly since it's already a `List<ConsoleColor>`.

- [x] **`currentColour` in `Display` is `static` — shared across all instances**
  If multiple `Display` instances were ever created (unlikely today but a DI misconfiguration away), they'd share colour state. More importantly, being static means the colour position persists across calls in unexpected ways. Make it an instance field.

- [x] **`SelectVersion` shows "Nothing selected!" even when user explicitly cancelled**
  When the user presses a key other than Y/N (returning `null`), the state jumps to `RequestSong` but then falls through to the `"Nothing selected!"` error message. The error should only display when the user said "no" to every song, not when they intentionally cancelled.

## Medium Priority

- [x] **Dead project: `JukeboxHelpers` contains only commented-out code**
  The `JukeboxHelpers` project has all code commented out — it was clearly superseded by `JukeboxDomain.Helpers`. It's still on disk (though not in the solution file). Remove the folder entirely to avoid confusion.

- [x] **`JukeboxDomain.Test` project references `JukeBoxLibrary` but doesn't test it**
  The test csproj has a `ProjectReference` to `JukeboxLibrary` that appears unnecessary. The tests in this project only cover domain classes. Remove the unused reference.

- [x] **Duplicate `DisplayTest` class exists in both test projects**
  `JukeboxLibrary.Test\Helpers\DisplayTest.cs` and `JukeboxDomain.Test\Helpers\DisplayTest.cs` contain identical tests. Remove the one in `JukeboxLibrary.Test` since `Display` lives in `JukeboxDomain`.

- [x] **No test coverage for `SongPlayer` or `SongList`**
  `SongPlayer` has audio playback logic and error handling that is completely untested. `SongList` is trivial but also untested. At minimum, verify that `PlaySong` handles missing files gracefully and that `SongList.Build` delegates correctly.

- [x] **GitHub Actions workflow uses deprecated `actions/checkout@v1`**
  `dotnet.yml` uses `actions/checkout@v1` which is outdated and slower (uses git REST API instead of git commands). Update to `actions/checkout@v4`.

- [x] **GitHub Actions workflow uses `$Env:` PowerShell syntax in `env:` block**
  The `SOLUTION_FILE` env var is set as `$Env:GITHUB_WORKSPACE\JukeBox.sln` which uses PowerShell syntax. In the `env:` context of a workflow file this should be `${{ github.workspace }}\JukeBox.sln` or the steps should reference it differently. This may cause build failures.

## Low Priority

- [x] **`ISongSources.Sources` exposes a mutable `List<string>` via the interface**
  The interface allows consumers to replace or mutate the sources list directly. Use `IReadOnlyList<string>` on the interface and expose mutation only through methods if needed.

- [x] **`ISongList.SongCollection` exposes a mutable `List<ISong>` via the interface**
  Same issue — the interface exposes a settable `List<ISong>`. Prefer `IReadOnlyList<ISong>` on the interface contract.

- [x] **`FileSystemParser` is a static class making it hard to test in isolation**
  The file system access is baked into a static helper with no abstraction. This makes unit testing require real directories. Consider injecting a file system abstraction or making it non-static and interface-backed.

- [x] **`JukeboxEngine` exposes all dependencies as public properties**
  `SongSources`, `SongList`, `SongPlayer`, and `DisplayEngine` are all public on the engine class. These are implementation details and should be private (or at most internal). The tests currently assert on them directly, which couples tests to internals.

- [x] **`ConsoleEngineTest.ReadAKey` test doesn't actually test anything**
  The test just asserts `consoleEngine` is not null. Either remove it or use a proper approach to verify `ReadKey` behaviour (e.g. redirect stdin with a `ConsoleKeyInfo` mock).

- [x] **Collection initialisers use `new()` target-typed syntax inconsistently**
  Some places use `new List<string>()` and others use `new()`. Pick one style for consistency across the codebase.
