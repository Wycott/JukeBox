# Coding Standards

- Use file-scoped namespaces (`namespace Foo;`) instead of block-scoped namespaces.
- Prefer `var` when the type is obvious from the right-hand side of the assignment.
- All `if` and `else` statements must use braces, even for single-line bodies.
- All `return` statements must be preceded by a blank line.
- Do not use underscore prefix for private member variables. Use `camelCase` without a leading underscore.
- Never use top-level statements in C# console applications. Always use an explicit `Program` class with a `Main` method.
- Always create a `.slnx` (solution) file for .NET projects.
- Unit tests must always be written using xUnit.
- Business logic must live in a separate class library project, not in the console application project. The console project should only contain the entry point and wiring.
- Do not create top level folders to put the projects in e.g. src, tests etc. Project folders should be at the top level in the same directory as the .slnx file
- Always use .NET Core 10
- When generating code from tasks, do them one at a time. Once one task is finished, start the next.
- Use the moq framework for mocking.

