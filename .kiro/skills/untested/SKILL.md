---
name: untested
description: Finds all .cs source files in the solution that have no corresponding unit tests.
---

# Untested Skill

## Description

Scans the JukeBox solution for `.cs` source files that have no corresponding unit test coverage. Identifies classes and modules in `JukeBox/`, `JukeboxDomain/`, `JukeboxLibrary/`, `JukeboxInterfaces/`, and `JukeboxTypes/` that are not exercised by any test in `JukeboxDomain.Test/` or `JukeboxLibrary.Test/`.

## Trigger

When the user asks to find "untested", "missing tests", "test coverage gaps", or "what needs tests".

## Instructions

Identify all `.cs` source files across the solution (excluding `obj/`, `bin/`, and the test projects themselves) that do not have a corresponding test file or are not referenced in any test.

### Procedure

1. Read all `.cs` source files in `JukeBox/`, `JukeboxDomain/`, `JukeboxLibrary/`, `JukeboxInterfaces/`, and `JukeboxTypes/` (excluding `obj/` and `bin/` directories).
2. Read all `.cs` test files in `JukeboxDomain.Test/` and `JukeboxLibrary.Test/` (excluding `obj/`, `bin/`, and `Usings.cs`).
3. For each source file, determine whether it is tested by checking:
   - Whether a test file exists that references the class name (e.g. `Favourites` → `FavouritesTest.cs`)
   - Whether the class name appears in any test file (instantiated, mocked, or referenced)
4. A source file is considered **untested** if no test file references its primary class by name.
5. Interfaces are considered **untested** only if they are never referenced in any test file (even indirectly via mocks).
6. Write the results to `untested.md` in the repository root using the Output Format below.

### Output Format

Produce a markdown report with the following structure:

```markdown
# Untested Modules — JukeBox

## Summary

<One paragraph summarising how many source files were checked, how many are untested, and overall coverage health.>

---

## Untested Files

| # | File | Project | Class/Interface | Reason |
|---|------|---------|-----------------|--------|
| 1 | `path/to/file.cs` | Project Name | ClassName | No test file references this class |

---

## Tested Files

| # | File | Project | Class/Interface | Test File |
|---|------|---------|-----------------|-----------|
| 1 | `path/to/file.cs` | Project Name | ClassName | `TestFile.cs` |
```

### Rules

- Read the actual source and test files before reporting — do not guess at content.
- Only flag a file as untested if you have confirmed no test references its class name.
- Exclude auto-generated files (`AssemblyInfo.cs`, `GlobalUsings.g.cs`, etc.).
- Exclude `Program.cs` from the untested list (entry point — tested indirectly).
- Exclude I/O wrapper classes marked with `[ExcludeFromCodeCoverage]` from the untested list.
- Write the report to `untested.md` in the repository root.
