# gcode

Personal Roslyn analyzer: house rules, style, and preferences enforced as
compiler diagnostics instead of code review comments.

## Layout

```
gcode/
├── src/gcode/                  Analyzer project (netstandard2.0, packable)
│   ├── Rules/                  One file per analyzer
│   ├── AnalyzerReleases.Shipped.md
│   └── AnalyzerReleases.Unshipped.md
├── tests/gcode.Tests/          xUnit tests using Microsoft.CodeAnalysis.Testing
└── docs/rules/                 One markdown page per rule ID (GCODE00xx.md)
```

## Building and testing

```bash
dotnet build
dotnet test
```

## Adding a new rule

1. Pick the next free ID (`GCODE0002`, `GCODE0003`, ...).
2. Add a `DiagnosticAnalyzer` under `src/gcode/Rules/`
   (`EmptyCatchBlockAnalyzer.cs` is the template — descriptor, `Initialize`,
   a syntax/symbol action).
3. Add its row to `AnalyzerReleases.Unshipped.md`.
4. Add `docs/rules/<ID>.md` describing cause, fix, and example (the
   `helpLinkUri` on each descriptor points here).
5. Add tests under `tests/gcode.Tests/` covering both the flagged and the
   allowed cases.

When a version of the package ships, move its rows from
`AnalyzerReleases.Unshipped.md` into `AnalyzerReleases.Shipped.md` under a
new `## Release x.y` heading.

## Consuming the analyzer

Pack and reference it like any other analyzer NuGet package:

```bash
dotnet pack src/gcode/gcode.csproj -c Release
```

```xml
<PackageReference Include="gcode" Version="1.0.0" PrivateAssets="all" />
```

Rules can be tuned per-consumer via `.editorconfig`, e.g.:

```ini
dotnet_diagnostic.GCODE0001.severity = error
```
