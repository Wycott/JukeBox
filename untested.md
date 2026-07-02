# Untested Modules — JukeBox

## Summary

17 source files were checked across 5 projects. 2 classes have no corresponding unit tests. Overall test coverage is good — the core engine, display, file parsing, favourites, songs, and sources are all directly tested. The gaps are the newly added `BanList` and `LevenshteinDistance` classes.

---

## Untested Files

| # | File | Project | Class/Interface | Reason |
|---|------|---------|-----------------|--------|
| 1 | `JukeboxDomain/BanList.cs` | JukeboxDomain | BanList | Only mocked via `IBanList` in tests — no direct tests of Ban, IsBanned, IsSimilarToBanned, or persistence logic |
| 2 | `JukeboxDomain/Helpers/LevenshteinDistance.cs` | JukeboxDomain | LevenshteinDistance | No test file references this class |

---

## Tested Files

| # | File | Project | Class/Interface | Test File |
|---|------|---------|-----------------|-----------|
| 1 | `JukeboxDomain/Favourites.cs` | JukeboxDomain | Favourites | `FavouritesTest.cs` |
| 2 | `JukeboxDomain/Song.cs` | JukeboxDomain | Song | `SongTest.cs` |
| 3 | `JukeboxDomain/SongList.cs` | JukeboxDomain | SongList | `SongListTest.cs` |
| 4 | `JukeboxDomain/SongSources.cs` | JukeboxDomain | SongSources | `SongSourcesTest.cs` |
| 5 | `JukeboxDomain/SongPlayer.cs` | JukeboxDomain | SongPlayer | `SongPlayerTest.cs` |
| 6 | `JukeboxDomain/Helpers/ConsoleEngine.cs` | JukeboxDomain | ConsoleEngine | `ConsoleEngineTest.cs` |
| 7 | `JukeboxDomain/Helpers/Display.cs` | JukeboxDomain | Display | `DisplayTest.cs` |
| 8 | `JukeboxDomain/Helpers/FileSystemParser.cs` | JukeboxDomain | FileSystemParser | `FileSystemParserTest.cs` |
| 9 | `JukeboxLibrary/JukeboxEngine.cs` | JukeboxLibrary | JukeboxEngine | `JukeboxEngineTest.cs` |

---

## Excluded Files

| # | File | Project | Reason |
|---|------|---------|--------|
| 1 | `JukeBox/Program.cs` | JukeBox | Entry point — excluded per convention |
| 2 | `JukeboxInterfaces/*.cs` | JukeboxInterfaces | Interfaces — referenced via mocks in tests |
| 3 | `JukeboxTypes/JukeboxStateType.cs` | JukeboxTypes | Enum — used indirectly by engine tests |
| 4 | `JukeboxInterfaces/SongResponse.cs` | JukeboxInterfaces | Enum — referenced directly in tests |
