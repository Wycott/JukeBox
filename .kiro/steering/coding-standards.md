---
inclusion: always
---

# Coding Standards

## Language & Framework

- C# with .NET 10, implicit usings enabled, nullable reference types enabled
- Target latest C# language features (collection expressions `[]`, primary constructors, `field` keyword)

## Project Conventions

### Naming

- PascalCase for public members, types, and methods
- camelCase for private fields (no underscore prefix)
- Properties preferred over public fields (except `EnglishDictionary.GameLetters` which is a legacy exception)

### Structure

- One class per file (named to match the class)
- Partial classes are used to separate concerns within a single logical class (e.g. `GameEngine` + `Display`)
- Static helper classes live in the `Helpers/` folder
- Interfaces prefixed with `I` (e.g. `IGameConsole`)

### Code Quality

- Mark classes that wrap I/O with `[ExcludeFromCodeCoverage]`
- Mark AI-generated code with `[AiGenerated]` from the `AiAnnotations` package
- Keep methods short and focused — prefer extracting helpers
- Use expression-bodied members where the body is a single expression

### Error Handling

- Prefer guard clauses and early returns
- Null checks via nullable reference types rather than explicit null guards where possible

### Console Abstraction

- All console interaction goes through `IGameConsole` — never call `System.Console` directly from game logic
- This enables unit testing without console dependencies

### Persistence

- Use `System.Text.Json` for serialization
- Keep data files (like `BestGame.json`) in the working directory
- Use `JsonSerializerOptions { WriteIndented = true }` for human-readable output
