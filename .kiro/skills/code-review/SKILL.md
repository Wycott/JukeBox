---
name: code-review
description: Performs a thorough code review checking for bugs, maintainability issues, and adherence to project coding standards.
---

# Code Review Skill

## Description

Performs a thorough code review of the JukeBox solution, checking for bugs, maintainability issues, and adherence to project coding standards.

## Trigger

When the user asks to "review", "code review", or "check the code".

## Instructions

Perform a code review of the C# source files in the `JukeBox/`, `JukeboxDomain/`, `JukeboxLibrary/`, `JukeboxInterfaces/`, and `JukeboxTypes/` project directories. Evaluate the code against the project's coding standards and best practices.

### Review Checklist

Work through each of these questions. Only raise an issue if the answer is unsatisfactory.

#### Correctness & Functionality

1. Does the code solve the problem?
2. Does the code compile?
3. Does the code compile without warnings?
4. Does the code run?
5. Are parameters correctly validated?
6. Are exceptions handled gracefully?

#### Efficiency

7. Is there a more efficient way to achieve the same result?

#### Readability & Maintainability

8. Is the code readable and maintainable?
9. Is the code style consistent?
10. Are class and variable names meaningful?
11. Is the code free of magic numbers?
12. Is the code free of magic literals?

#### Comments & Dead Code

13. Has commented-out code been removed?
14. Are there comments that simply describe the code without adding value?
15. Are there comments that describe complex code that should be refactored instead?
16. Are there comments that will require maintenance for a simple code change?
17. Have TODOs been dealt with?
18. Is there dead code that could be removed (e.g. orphan or empty methods)?

#### Design Principles

19. Does the code follow SOLID principles?
20. Does the code follow DRY principles?

#### Testing

21. Are unit tests present?
22. Do the unit tests pass?
23. Is there any code that isn't adequately covered by unit tests?

#### Tooling

24. Have ReSharper warnings been dealt with?

#### Project-Specific Conventions

25. **Naming** — PascalCase for public members, camelCase for private fields (no underscore prefix)
26. **Structure** — one class per file, partial classes used appropriately, helpers in `Helpers/`
27. **Code quality** — methods short and focused, expression-bodied members for single expressions, guard clauses preferred
28. **Nullable safety** — nullable reference types used correctly, no redundant null checks on non-nullable types
29. **Console abstraction** — all console I/O through `IConsoleEngine`, never direct `System.Console` calls in engine logic
30. **Attributes** — `[ExcludeFromCodeCoverage]` on I/O wrappers, `[AiGenerated]` on AI-written code
31. **Modern C# usage** — collection expressions `[]`, primary constructors, `[GeneratedRegex]` where appropriate
32. **Encapsulation** — prefer properties over fields, restrict setters where mutation isn't needed externally

### Procedure

1. Read all source files in `JukeBox/`, `JukeboxDomain/`, `JukeboxLibrary/`, `JukeboxInterfaces/`, and `JukeboxTypes/` (excluding `obj/` and `bin/`)
2. Run `dotnet build` and check for warnings
3. Run `dotnet test` and confirm all tests pass
4. Work through the Review Checklist above against the source code
5. Report any issues found using the Output Format below

### Output Format

Produce a markdown report with the following structure:

```markdown
# Code Review — JukeBox

## Summary

<One paragraph summarising overall quality and the number/severity of issues found.>

---

## Issues

### <N>. <Short title>

**File:** `<filename>` — `<method or context>`
**Priority:** High | Medium | Low
**Description:** <What the problem is and why it matters.>
**Task:** <What to do to fix it.>
- [ ] To do

---

## Summary Table

| # | Issue | Priority | Status |
|---|-------|----------|--------|
| 1 | ... | ... | ⬜ To do |
```

### Priority Definitions

- **High** — Potential runtime bug, data loss, or incorrect behaviour visible to the user
- **Medium** — Maintainability problem, performance concern, or deviation from project conventions that could cause future bugs
- **Low** — Style issue, minor improvement, or defensive hardening

### Rules

- Only report genuine issues. Do not pad the review with trivial style nitpicks.
- Group related issues together if they share a root cause.
- Read the actual source files before reporting — do not guess at code content.
- Reference the specific file and method where the issue occurs.
- Check `#[[file:../../.kiro/steering/coding-standards.md]]` for the project conventions to validate against.
- Write the report to `review.md` in the repository root.
