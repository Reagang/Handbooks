# All rules

Every rule from the five uploaded packs (clean-code, dependency-injection,
observability, memory-safety, organization) plus GCODE0001, combined into one
deduplicated list. Machine-readable source: [`catalog/all-rules.json`](../../catalog/all-rules.json).

## Deduplication

Where the same concrete check appeared under more than one ID across packs -
same indicators, same fix - it's listed once here, under whichever ID is
canonical (the domain-specific pack's ID, or GCODE0001 for the pre-existing
empty-catch rule). The other ID(s) are shown in the **Aliases** column rather
than getting a second row:

| Canonical | Alias | Why |
|-----------|-------|-----|
| CC1004 | ORG1002 | both flag `.Result`/`.Wait()`/`.GetAwaiter().GetResult()`/`Thread.Sleep` in async code |
| CC1003 | ORG1007 | both flag hardcoded numeric/string literals in comparisons |
| DI1005 | ORG1004 | both flag missing `ValidateScopes`/`ValidateOnBuild` at startup |
| DI1001 | OBS1005 | both flag service-locator/`IServiceProvider` injection |
| GCODE0001 | OBS1004 | OBS1004's concrete case (empty `catch`) is GCODE0001 |

`ORG1009` (prefer static private methods) isn't an alias of another pack rule -
it's its own row, marked as covered by the built-in `CA1822` instead.

## Severity

**Governance-type rules use `warning` only**, regardless of what severity the
source pack originally gave them (several were `critical` or `info`) - governance
rules are policy reminders for review, not compiler-checkable facts with a
graduated scale, so a single consistent severity is more meaningful than a
borrowed one. `deterministic`/`heuristic` rules keep the source pack's severity.

For rules **implemented as analyzers**, the severity shown here is the source
pack's own classification, which can differ from the actual `DiagnosticSeverity`
the analyzer ships with (documented on each rule's own page) - e.g. the packs rate
several of these `error`/`critical`, but they ship as compiler `Warning` so a
consuming project isn't broken by default; tune via `.editorconfig` per
`gcode/README.md`.

## Summary (36 unique rules)

- **Implemented as a Roslyn analyzer:** 9
- **Covered by a built-in .NET analyzer:** 1
- **Guidance only (not statically enforced):** 26

## Implemented as a Roslyn analyzer (9)

Ships as a real compiler diagnostic in this project.

| ID | Title | Domain | Severity | Aliases |
|----|-------|--------|----------|---------|
| [CC1001](CC1001.md) | Avoid inefficient LINQ | clean-code | error | - |
| [CC1003](CC1003.md) | Avoid magic constants | clean-code | error | ORG1007 |
| [CC1004](CC1004.md) | Avoid blocking async code | clean-code | error | ORG1002 |
| [DI1002](DI1002.md) | Avoid duplicate service registrations | dependency-injection | warning | - |
| [DI1003](DI1003.md) | Prevent circular dependencies | dependency-injection | critical | - |
| [GCODE0001](GCODE0001.md) | Empty catch block | reliability | warning | OBS1004 |
| [MEM1001](MEM1001.md) | Detach event handlers to prevent memory leaks | memory-safety | critical | - |
| [MEM1003](MEM1003.md) | Require GC.SuppressFinalize when implementing finalizers | memory-safety | warning | - |
| [ORG1016](ORG1016.md) | Use StringBuilder for repeated concatenation | performance | info | - |

## Covered by a built-in .NET analyzer (1)

Already enforced by an existing, well-tested analyzer - no need to reimplement it.

| ID | Title | Domain | Severity | Aliases |
|----|-------|--------|----------|---------|
| [ORG1009](organization-guidance.md#org1009-prefer-static-private-methods-where-possible) | Prefer static private methods where possible (→ `CA1822`) | performance | info | - |

## Guidance only (not statically enforced) (26)

The pack authors marked these `heuristic` or `governance` - they need judgment a syntax check can't reliably provide. Documented for human/AI code review instead.

| ID | Title | Domain | Severity | Aliases |
|----|-------|--------|----------|---------|
| [CC1002](clean-code-guidance.md#cc1002-prevent-controller-bloat) | Prevent controller bloat | clean-code | warning | - |
| [DI1001](dependency-injection-guidance.md#di1001-prevent-misuse-of-dependency-injection) | Prevent misuse of dependency injection | dependency-injection | critical | OBS1005 |
| [DI1004](dependency-injection-guidance.md#di1004-prevent-excessive-constructor-dependencies) | Prevent excessive constructor dependencies | dependency-injection | info | - |
| [DI1005](dependency-injection-guidance.md#di1005-validate-dependency-injection-registrations-on-startup) | Validate dependency injection registrations on startup | dependency-injection | warning | ORG1004 |
| [MEM1002](memory-safety-guidance.md#mem1002-avoid-async-closures-without-cancellation) | Avoid async closures without cancellation | memory-safety | warning | - |
| [MEM1004](memory-safety-guidance.md#mem1004-avoid-long-lived-timers-without-disposal) | Avoid long-lived timers without disposal | memory-safety | critical | - |
| [MEM1005](memory-safety-guidance.md#mem1005-avoid-unbounded-in-memory-collections) | Avoid unbounded in-memory collections | memory-safety | critical | - |
| [OBS1001](observability-guidance.md#obs1001-require-telemetry-and-structured-logging) | Require telemetry and structured logging | observability | warning | - |
| [OBS1002](observability-guidance.md#obs1002-require-retry-and-resiliency-telemetry) | Require retry and resiliency telemetry | observability | warning | - |
| [OBS1003](observability-guidance.md#obs1003-prevent-sensitive-data-leakage-in-telemetry) | Prevent sensitive data leakage in telemetry | observability | warning | - |
| [ORG1001](organization-guidance.md#org1001-prefer-property-patterns-for-dto-and-json-validation) | Prefer property patterns for DTO and JSON validation | readability | info | - |
| [ORG1003](organization-guidance.md#org1003-validate-options-during-startup) | Validate options during startup | dependency-injection | warning | - |
| [ORG1005](organization-guidance.md#org1005-validate-aspnet-middleware-order) | Validate ASP.NET middleware order | architecture | critical | - |
| [ORG1006](organization-guidance.md#org1006-never-automatically-modify-appsettings-files) | Never automatically modify appsettings files | governance | warning | - |
| [ORG1008](organization-guidance.md#org1008-encourage-helper-methods-and-extension-methods) | Encourage helper methods and extension methods | maintainability | info | - |
| [ORG1010](organization-guidance.md#org1010-break-code-into-smaller-readable-components) | Break code into smaller readable components | architecture | warning | - |
| [ORG1011](organization-guidance.md#org1011-avoid-god-classes-and-god-methods) | Avoid god classes and god methods | architecture | critical | - |
| [ORG1012](organization-guidance.md#org1012-ensure-methods-have-a-single-responsibility) | Ensure methods have a single responsibility | clean-code | warning | - |
| [ORG1013](organization-guidance.md#org1013-prefer-records-over-classes-where-applicable) | Prefer records over classes where applicable | architecture | info | - |
| [ORG1014](organization-guidance.md#org1014-avoid-multiple-nested-loops) | Avoid multiple nested loops | performance | warning | - |
| [ORG1015](organization-guidance.md#org1015-add-xml-documentation-where-appropriate) | Add XML documentation where appropriate | maintainability | warning | - |
| [ORG1017](organization-guidance.md#org1017-organize-code-into-appropriate-architectural-boundaries) | Organize code into appropriate architectural boundaries | architecture | warning | - |
| [ORG1018](organization-guidance.md#org1018-extract-complex-validation-logic-into-dedicated-components) | Extract complex validation logic into dedicated components | maintainability | warning | - |
| [ORG1019](organization-guidance.md#org1019-use-named-or-typed-httpclients) | Use named or typed HttpClients | distributed-systems | warning | - |
| [ORG1020](organization-guidance.md#org1020-retries-must-implement-backoff-strategy) | Retries must implement backoff strategy | resiliency | warning | - |
| [ORG1021](organization-guidance.md#org1021-prefer-service-discovery-over-hardcoded-endpoints) | Prefer service discovery over hardcoded endpoints | cloud-native | warning | - |
