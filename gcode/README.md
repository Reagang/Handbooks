# gcode

Personal Roslyn analyzer: house rules, style, and preferences enforced as
compiler diagnostics instead of code review comments.

## Layout

```
gcode/
├── catalog/                    Rule catalog (JSON), one file per pack/domain -
│   │                           source of truth for every rule under
│   │                           consideration, not just the shipped ones
│   └── README.md                describes the schema and status field
├── src/gcode/                  Analyzer project (netstandard2.0, packable)
│   ├── Rules/                   one file per analyzer, grouped by domain
│   │                            (CleanCode/, DependencyInjection/,
│   │                            MemorySafety/, Organization/)
│   ├── AnalyzerReleases.Shipped.md
│   └── AnalyzerReleases.Unshipped.md
├── tests/gcode.Tests/           xUnit tests using Microsoft.CodeAnalysis.Testing
└── docs/rules/                  one page per implemented rule ID, plus one
                                  consolidated `<pack>-guidance.md` per pack
                                  for the rules that aren't implemented as
                                  analyzers
```

## Rules

| ID        | Title                                          | Severity |
|-----------|-------------------------------------------------|----------|
| GCODE0001 | Empty catch block                                | Warning  |
| CC1001    | Unnecessary LINQ materialization                 | Warning  |
| CC1003    | Magic constant in comparison                     | Warning  |
| CC1004    | Blocking call on async code                      | Warning  |
| DI1002    | Duplicate service registration                   | Warning  |
| DI1003    | Circular constructor dependency                  | Warning  |
| MEM1001   | Event handler never detached                     | Warning  |
| MEM1003   | Finalizer without GC.SuppressFinalize             | Warning  |
| ORG1016   | String concatenation in a loop                   | Info     |

Every other rule considered so far (controller bloat, telemetry coverage,
DI startup validation, god classes, ...) is documented but not statically
enforced - see `catalog/README.md` for why, and
`docs/rules/<pack>-guidance.md` for the full list per pack.

## Building and testing

```bash
dotnet build
dotnet test
```

## Adding a new rule

1. Pick an ID. Reuse a pack's own ID (`CC10xx`, `DI10xx`, `MEM10xx`,
   `OBS10xx`, `ORG10xx`) if the rule comes from `catalog/`; otherwise use
   the next free `GCODE0xxx`.
2. If it's not already in `catalog/`, add it there first (see
   `catalog/README.md` for the schema) - this is the source of truth for
   problem/fix/examples, and keeps the analyzer/doc/test in sync with it.
3. Add a `DiagnosticAnalyzer` under `src/gcode/Rules/<Domain>/`
   (`EmptyCatchBlockAnalyzer.cs` is the simplest template - descriptor,
   `Initialize`, a syntax/symbol action; `CircularDependencyAnalyzer.cs`
   shows a compilation-wide analyzer for cross-type checks).
4. Set `catalog/<pack>.json`'s `status` to `"analyzer"` and add
   `"analyzerId"`.
5. Add its row to `AnalyzerReleases.Unshipped.md`.
6. Add `docs/rules/<ID>.md` describing cause, fix, and example (the
   `helpLinkUri` on each descriptor points here), and remove/replace the
   rule's entry in the relevant `docs/rules/<pack>-guidance.md`.
7. Add tests under `tests/gcode.Tests/` covering both the flagged and the
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
dotnet_diagnostic.ORG1016.severity = none
```
