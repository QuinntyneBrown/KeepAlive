# Contributing

Thanks for your interest in contributing to KeepAlive.

## Getting Started

1. Fork and clone the repository.
2. Ensure the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) is installed.
3. Build the solution:
   ```
   dotnet build
   ```

## Development Workflow

1. Create a branch from `main`:
   ```
   git checkout -b your-branch-name
   ```
2. Make your changes.
3. Ensure the project builds with no warnings or errors:
   ```
   dotnet build --warnaserrors
   ```
4. Commit with a clear, descriptive message.
5. Open a pull request against `main`.

## Project Layout

```
KeepAlive.sln
src/
  KeepAlive.Cli/          # CLI application
```

## Coding Guidelines

- Target **.NET 9**.
- Use C# language features up to **C# 13**.
- Follow the existing code style (file-scoped types, top-level statements in `Program.cs`).
- Keep P/Invoke declarations in `NativeMethods.cs`.
- Prefer `sealed` on internal classes that are not intended for inheritance.
- Avoid adding dependencies unless strictly necessary.

## Reporting Issues

Open a GitHub issue with:

- A clear description of the problem or suggestion.
- Steps to reproduce (if reporting a bug).
- Your OS version and .NET SDK version (`dotnet --info`).

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.
