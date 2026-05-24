```# JukeBox Code Review

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

---

## Second Pass

### High Priority

- [x] **`ServiceProvider` is never disposed in `Program.cs`**
  The `ServiceProvider` is built but never disposed. Since `SongPlayer` now implements `IDisposable`, the DI container should dispose it on shutdown. Wrap the provider in a `using` statement or call `serviceProvider.Dispose()` after `engine.Start()` returns.

- [x] **`HaveASongByThisArtist` uses hardcoded `\\` path separator**
  `FileSystemParser.HaveASongByThisArtist()` calls `LastIndexOf("\\")` which won't work on non-Windows platforms. Use `Path.GetDirectoryName()` or `Path.DirectorySeparatorChar` instead.

- [x] **`SongSources.CountSongsInSource` only catches `DirectoryNotFoundException` at the top level**
  If a source path exists but is inaccessible (e.g. network drive timeout), `IOException` or `UnauthorizedAccessException` from `GetDirectories` will crash the app at startup. The inner loop catches `UnauthorizedAccessException` but the outer `GetDirectories` call does not.

- [x] **`JukeboxLibrary` has a direct dependency on NAudio but doesn't use it**
  `JukeboxLibrary.csproj` references the `naudio` package, but `JukeboxEngine` doesn't use NAudio directly — that's in `JukeboxDomain`. This is a redundant dependency that inflates the library project.

### Medium Priority

- [x] **`ISongPlayer` doesn't extend `IDisposable` — disposal depends on knowing the concrete type**
  `SongPlayer` implements `IDisposable` but the interface `ISongPlayer` doesn't declare it. Consumers working through the interface (including the DI container) won't know to dispose it unless the container tracks it. Add `IDisposable` to the interface contract.

- [x] **`FileSystemParser.ParseFileSystem` uses `Guid.NewGuid()` for shuffle — not truly random**
  `OrderBy(_ => Guid.NewGuid())` produces a non-uniform shuffle because GUIDs aren't designed for randomness. Use `Random.Shared.Next()` or the .NET 8+ `Random.Shared.Shuffle()` for a proper Fisher-Yates shuffle.

- [x] **`FindSong` can skip the `Build` call if `selectedPattern` is null, then checks stale `SongCollection`**
  If `selectedPattern` is somehow null, the `Build` call is skipped but `SongCollection.Count` is still checked. This would use results from a previous search, which is confusing. The parameter is typed as `string?` but the caller always passes a non-null value — tighten the type to `string` and remove the null check.

- [x] **`JukeboxLibrary.Test` is missing `<IsTestProject>true</IsTestProject>`**
  The `JukeboxDomain.Test` csproj has this property but `JukeboxLibrary.Test` does not. This can affect how the project is treated by tooling (e.g. NuGet pack, code coverage).

- [x] **`SongListTest` has an unused `using JukeboxDomain.Helpers`**
  Minor, but the import is no longer needed since `FileSystemParser` is now injected via interface.

### Low Priority

- [x] **`ISong.FullPath` and `ShortenedPath` have public setters on the interface**
  The interface exposes mutable setters for `FullPath` and `ShortenedPath`. Only `FileSystemParser` sets these during construction. Make them `{ get; }` on the interface and use `init` or constructor parameters on the `Song` class.

- [x] **`MachineState.cs` filename doesn't match its content**
  The file is named `MachineState.cs` but contains the enum `JukeboxStateType`. Rename the file to `JukeboxStateType.cs` for discoverability.

- [x] **`JukeboxStateType.Unknown` is used as a sentinel for "quit" — semantically misleading**
  The `Unknown` state now means "exit the application" rather than an actual unknown/error state. Consider renaming it to `Exit` or adding a dedicated `Exit` value to make the intent clear.

- [x] **No `appsettings.json` validation — app crashes with unhelpful error if file is malformed**
  If `appsettings.json` is missing or contains invalid JSON, the app throws a raw `FileNotFoundException` or `JsonException`. Add a friendlier startup check or make the file optional with a warning.

- [x] **`Display.IsThisTheRightSong` mixes UI prompting with input parsing**
  The method writes output, reads a key, and interprets the result — three responsibilities. Splitting the prompt from the input interpretation would make it easier to test and reuse.

- [x] **NAudio `2.2.1` is referenced in two projects (`JukeboxDomain` and `JukeboxLibrary`)**
  Both csproj files reference NAudio but only `JukeboxDomain` actually uses it. If the version drifts between the two, it could cause binding conflicts. Remove the one in `JukeboxLibrary` (noted above) and consider using a `Directory.Packages.props` for central package management.

---

## Final Pass

The codebase is in solid shape after the previous rounds of fixes. The remaining items below are minor polish and architectural considerations rather than bugs or correctness issues.

### Medium Priority

- [x] **`SongPlayer` tests don't dispose the player — potential test resource leaks**
  `SongPlayerTest` creates `SongPlayer` instances (which implement `IDisposable`) but never wraps them in `using` statements. While unlikely to cause issues in a test runner, it's inconsistent with the disposal pattern we've established.

- [x] **`ConsoleEngineTest` mutates global `Console.SetOut`/`Console.SetIn` without restoring**
  Tests redirect `Console.Out` and `Console.In` but never restore the originals. If tests run in parallel or a later test depends on standard console streams, this could cause flaky failures. Capture and restore the original streams in a `try/finally` or use `IDisposable` test fixtures.

- [x] **`FileSystemParser.ParseFileSystem` only catches `DirectoryNotFoundException` from `GetDirectories`**
  Similar to the fix applied in `SongSources`, the outer `GetDirectories` call in `ParseFileSystem` should also catch `UnauthorizedAccessException` and `IOException` for resilience against network drives or permission issues.

- [x] **`IFileSystemParser.ParseFileSystem` returns `List<ISong>` — should return `IReadOnlyList<ISong>`**
  The interface returns a mutable `List<ISong>` which is inconsistent with the `IReadOnlyList` pattern used elsewhere (`ISongList.SongCollection`, `ISongSources.Sources`). Return `IReadOnlyList<ISong>` for consistency.

### Low Priority

- [x] **`JukeboxEngine` tests rely on `OperationCanceledException` to break the loop — fragile pattern**
  Every engine test throws `OperationCanceledException` from a mock to exit the `while` loop. Now that the engine supports a proper `Exit` state via "q", tests could use `consoleMock.Setup(x => x.ReadLine()).Returns("q")` for a cleaner exit on the second call instead of throwing.

- [x] **No `Usings.cs` file in `JukeboxDomain.Test` for global usings**
  `JukeboxLibrary.Test` has a `<Using Include="Xunit" />` in its csproj and a `Usings.cs` file, but `JukeboxDomain.Test` has neither. Adding a global `using Xunit;` would remove the need for implicit `[Fact]` resolution and keep the projects consistent.

- [x] **`SongSources` constructor performs I/O (directory scanning) — slows DI resolution**
  The constructor calls `DisplaySongCounts()` which scans the file system. This means DI container resolution triggers potentially slow I/O. Consider making the count display lazy (called on first use or via an explicit `Initialize()` method) so the app starts faster.

- [x] **`JukeboxEngine` could benefit from a `CancellationToken` for cooperative shutdown**
  The engine currently relies on the "q" command to exit. If the app needs to be stopped programmatically (e.g. from a signal handler or test), there's no mechanism. Adding a `CancellationToken` parameter to `Start()` would enable graceful external shutdown.

- [x] **Solution file still references old-style project GUIDs (`FAE04EC0-...`) for test projects**
  The test projects use the legacy project type GUID (`FAE04EC0-301F-11D3-BF4B-00C04F79EFBC`) while the main projects use the SDK-style GUID (`9A19103F-16F7-4668-BE54-9A1E7A4F7556`). This is cosmetic and doesn't affect builds, but normalizing them would clean up the solution file.

---

## Post-Favourites Review

### Medium Priority

- [ ] **`Favourites.RecordPlay` writes to disk on every single play — potential performance issue**
  Every song play triggers a JSON serialize + file write. For rapid `:f` / `:n` usage this could cause noticeable lag. Consider debouncing the save (e.g. save on exit or after a short delay) or only writing periodically.

- [ ] **`Favourites` has no thread safety — concurrent access could corrupt state**
  If `RecordPlay` and `GetRandomFavourite` were ever called from different threads (e.g. future async playback), the `_playCounts` dictionary could be corrupted. Not a problem today with the single-threaded loop, but worth noting if the architecture evolves.

- [ ] **`Favourites.RecordPlay` records the play even if `SongPlayer.PlaySong` fails**
  In `JukeboxEngine.PlaySong`, `RecordPlay` is called unconditionally after `SongPlayer.PlaySong`. If the player catches an exception (invalid file, etc.), the song still gets counted as played. Move `RecordPlay` inside a success path or have `PlaySong` return a success indicator.

- [ ] **`PlayRandomFavourite` can pick the same song repeatedly**
  `GetRandomFavourite` picks any random entry from the dictionary. There's no deduplication, so `:f` then `:n` could play the same song twice in a row. Consider tracking the last played favourite and excluding it from the next random pick.

- [ ] **`JukeboxLibrary` references `JukeboxDomain` but only uses interfaces — unnecessary coupling**
  `JukeboxLibrary.csproj` has a `ProjectReference` to `JukeboxDomain`, but `JukeboxEngine` only depends on interfaces from `JukeboxInterfaces`. The reference exists because the engine previously used `FileSystemParser` directly, but that's now injected. Remove the reference to keep the library layer clean.

### Low Priority

- [ ] **`Favourites` stores full file paths as dictionary keys — fragile if files move**
  If a user reorganises their music library, all favourites become orphaned entries that can never be played again. Consider storing a normalised key (e.g. artist + filename) or validating paths on load.

- [x] **`ListFavourites` shows all 100 entries with no pagination**
  If the user has 100 favourites, `:l` dumps them all at once which scrolls off screen. Consider showing the top 20 with a "press any key for more" prompt, or limiting the display count.

- [ ] **`ShowHelp` uses `ConsoleEngine` directly — inconsistent with other display methods**
  Most text output goes through `DisplayEngine` (which handles colours), but `ShowHelp` writes directly via `ConsoleEngine.WriteALine`. This means help text is always the default console colour. Consider using `DisplayEngine` for consistency, or at minimum document the intentional difference.

- [ ] **`ISongSources.DisplaySongCounts()` is a UI concern on a data interface**
  The interface mixes data access (`Sources`) with presentation (`DisplaySongCounts`). This makes it harder to reuse `ISongSources` in a non-console context. Consider moving the display logic to the engine or a dedicated presenter.

- [x] **No unit tests for the `Favourites` class**
  `Favourites` has file I/O, trimming logic, and JSON serialization but no dedicated tests. The engine tests mock `IFavourites`, so the actual implementation is untested. Add tests using a temp file to verify `RecordPlay`, `GetTopFavourites`, `GetRandomFavourite`, trimming at 100, and corrupt JSON handling.

- [x] **`@` artist marker in search pattern is undocumented in help**
  The `:?` help shows commands but doesn't mention the `@artist` search syntax (e.g. `*@stones`). Users won't discover this feature without reading the source code.
