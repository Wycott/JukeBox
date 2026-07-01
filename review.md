# Code Review — JukeBox

## Summary

The solution is well-structured with clean separation of concerns across projects, consistent coding style, and good test coverage (77 tests, all passing). The build compiles with zero warnings. I found 6 issues — one high-priority potential bug, three medium-priority maintainability concerns, and two low-priority convention items.

---

## Issues

### 1. PlayRandomFavourite can return a banned favourite with misleading error message

**File:** `JukeboxLibrary/JukeboxEngine.cs` — `PlayRandomFavourite`
**Priority:** High
**Description:** When `GetRandomFavourite()` returns a banned song, the error message shown is "No favourites yet" — which is misleading since favourites do exist, they're just banned. More critically, only a single random pick is attempted. If that one happens to be banned, the user gets an error even though other non-banned favourites may exist. With a large ban list and modest favourites list, this will frequently fail unnecessarily.
**Task:** Retry the random pick (with a sensible max-attempts limit), and show a more accurate message like "No eligible favourites" when all attempts are exhausted.
- [ ] To do

---

### 2. Duplicated file extension list

**File:** `JukeboxDomain/Helpers/FileSystemParser.cs` and `JukeboxDomain/SongSources.cs`
**Priority:** Medium
**Description:** Valid extensions (`".mp3", ".m4a"`) are defined separately in `FileSystemParser.ValidExtensions` and inline in `SongSources.CountSongsInSource`. If a new format is added (e.g. `.flac`), one location could easily be missed — violating DRY.
**Task:** Extract a shared constant (e.g. `SupportedExtensions`) in a common location referenced by both classes.
- [ ] To do

---

### 3. `IsSimilarToBanned` uses O(n) list scan per candidate

**File:** `JukeboxDomain/BanList.cs` — `IsBanned` and `IsSimilarToBanned`
**Priority:** Medium
**Description:** `IsBanned` does a `List.Contains` with a `StringComparer`, and `IsSimilarToBanned` iterates the full list for each song candidate. During a file system search that may return hundreds of results, this results in O(songs × banned) comparisons. `IsBanned` could be O(1) with a `HashSet`, and while `IsSimilarToBanned` necessarily remains O(n), the exact-match check inside it could be made cheaper.
**Task:** Replace `List<string>` with a `HashSet<string>(StringComparer.OrdinalIgnoreCase)` for `IsBanned` lookups. Keep the list for ordered operations (`GetBannedSongs`, serialization) alongside the set.
- [ ] To do

---

### 4. `ConsoleEngine` missing `[ExcludeFromCodeCoverage]`

**File:** `JukeboxDomain/Helpers/ConsoleEngine.cs`
**Priority:** Medium
**Description:** Per project conventions, classes that wrap I/O should be marked `[ExcludeFromCodeCoverage]`. `ConsoleEngine` directly wraps `System.Console` but lacks this attribute.
**Task:** Add `[ExcludeFromCodeCoverage]` to the class.
- [ ] To do

---

### 5. `SongPlayer` missing `[ExcludeFromCodeCoverage]`

**File:** `JukeboxDomain/SongPlayer.cs`
**Priority:** Low
**Description:** Same convention as above — `SongPlayer` wraps NAudio I/O and cannot be meaningfully unit tested.
**Task:** Add `[ExcludeFromCodeCoverage]` to the class.
- [ ] To do

---

### 6. `Favourites.RecordPlay` uses `ContainsKey` + indexer instead of `TryGetValue`

**File:** `JukeboxDomain/Favourites.cs` — `RecordPlay`
**Priority:** Low
**Description:** The pattern `if (dict.ContainsKey(k)) dict[k]++; else dict[k] = 1;` performs two hash lookups on the hit path. Modern C# offers `CollectionsMarshal.GetValueRefOrAddDefault` or at minimum a single-lookup approach with `TryGetValue`.
**Task:** Replace with a single-lookup pattern for minor efficiency improvement.
- [ ] To do

---

## Summary Table

| # | Issue | Priority | Status |
|---|-------|----------|--------|
| 1 | Misleading error & single-attempt random favourite | High | ⬜ To do |
| 2 | Duplicated file extension list (DRY) | Medium | ⬜ To do |
| 3 | O(n) list scan in BanList per candidate | Medium | ⬜ To do |
| 4 | ConsoleEngine missing [ExcludeFromCodeCoverage] | Medium | ⬜ To do |
| 5 | SongPlayer missing [ExcludeFromCodeCoverage] | Medium | ⬜ To do |
| 6 | Favourites.RecordPlay double hash lookup | Low | ⬜ To do |
