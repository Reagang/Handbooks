# Rule catalog

Machine-readable source of truth for every rule under consideration for this
analyzer, not just the ones currently shipping as diagnostics. Each file is a
JSON array of rule objects (title, problem, why, indicators, fix,
`aiReviewPrompt`, before/after `examples`, tags, owner) plus a `status` field
added on top of the original pack data:

| `status`             | Meaning                                                                 |
|----------------------|--------------------------------------------------------------------------|
| `analyzer`           | Shipped as a Roslyn diagnostic. `analyzerId` names the rule/diagnostic ID. |
| `covered-by`         | Duplicates another rule in this catalog. `coveredBy` names the canonical ID. |
| `covered-by-builtin` | Already enforced by a built-in .NET analyzer. `coveredBy` names its ID.  |
| `guidance`           | Documented for human/AI code review, not statically enforced — the pack authors marked it `heuristic` or `governance` (judgment calls, cross-cutting architecture, or things like telemetry coverage that syntax alone can't verify reliably). |

## Files

- `clean-code.json` — CC1001-1004
- `dependency-injection.json` — DI1001-1005
- `observability.json` — OBS1001-1005
- `memory-safety.json` — MEM1001-1005
- `organization.json` — ORG1001-1021 (organization-wide philosophy; several
  entries restate a domain pack's rule under a different ID — those carry
  `covered-by` rather than a second analyzer, so the same code doesn't get
  flagged twice)

## Why keep the `guidance`-only rules at all

The `aiReviewPrompt` and `examples` fields exist so this catalog can double
as context for an AI code reviewer, not only for a compiler analyzer — a
rule that can't be checked syntactically (e.g. "require telemetry and
structured logging") can still be handed to a reviewer, human or AI, as a
checklist item with a concrete good/bad example.

See `../docs/rules/` for the human-readable pages: one page per implemented
rule ID, and one consolidated guidance page per pack for everything else.
