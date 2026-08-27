# Rule catalog

Machine-readable source of truth for every rule under consideration for this
analyzer, not just the ones currently shipping as diagnostics. Each file is a
JSON array of rule objects (title, problem, why, indicators, fix,
`aiReviewPrompt`, before/after `examples`, tags, owner) plus a `status` field
added on top of the original pack data:

| `status`             | Meaning                                                                 |
|----------------------|--------------------------------------------------------------------------|
| `analyzer`           | Shipped as a Roslyn diagnostic. `analyzerId` names the rule/diagnostic ID. |
| `covered-by`         | Duplicates another rule - same indicators, same fix - already covered elsewhere. `coveredBy` names the canonical ID, which may live in a different pack. |
| `covered-by-builtin` | Already enforced by a built-in .NET analyzer. `coveredBy` names its ID.  |
| `guidance`           | Documented for human/AI code review, not statically enforced — the pack authors marked it `heuristic` or `governance` (judgment calls, cross-cutting architecture, or things like telemetry coverage that syntax alone can't verify reliably). |

## Files

- `clean-code.json` — CC1001-1004
- `dependency-injection.json` — DI1001-1005
- `observability.json` — OBS1001-1005 (`OBS1004` → `covered-by` GCODE0001,
  `OBS1005` → `covered-by` DI1001)
- `memory-safety.json` — MEM1001-1005
- `organization.json` — ORG1001-1021 (organization-wide philosophy; several
  entries restate a domain pack's rule under a different ID — those carry
  `covered-by` rather than a second analyzer, so the same code doesn't get
  flagged twice: `ORG1002` → CC1004, `ORG1004` → DI1005, `ORG1007` → CC1003)
- `all-rules.json` — **the deduplicated list**: every rule above merged into
  one array, with `covered-by` entries folded into their canonical rule's
  `aliases` array instead of appearing as a second row, plus GCODE0001 (which
  predates these packs and lives in `src/gcode/`, not a pack file). This is
  what [`../docs/rules/ALL_RULES.md`](../docs/rules/ALL_RULES.md) renders.

## Severity normalization

Rules keep the source pack's own `severity` (`info`/`warning`/`error`/
`critical`) **except** `type: "governance"` rules, which are normalized to
`warning` regardless of what the source pack said — governance rules are
policy reminders for review, not compiler-checkable facts with a graduated
scale, so a single consistent severity is more meaningful than a borrowed
one. This does not apply to `deterministic`/`heuristic` rules, and it's
independent from the actual `DiagnosticSeverity` an `analyzer`-status rule
ships with in code (see `../README.md`).

## Why keep the `guidance`-only rules at all

The `aiReviewPrompt` and `examples` fields exist so this catalog can double
as context for an AI code reviewer, not only for a compiler analyzer — a
rule that can't be checked syntactically (e.g. "require telemetry and
structured logging") can still be handed to a reviewer, human or AI, as a
checklist item with a concrete good/bad example.

See `../docs/rules/` for the human-readable pages: one page per implemented
rule ID, and one consolidated guidance page per pack for everything else.
