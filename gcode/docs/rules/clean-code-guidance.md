# Clean Code pack - rule guidance

Rules from the clean-code pack that aren't implemented as a Roslyn analyzer here - they need judgment calls (`heuristic`) that a syntax check can't reliably make.

Machine-readable source: [`catalog/clean-code.json`](../../catalog/clean-code.json). For the full deduplicated list across all packs, see [`ALL_RULES.md`](ALL_RULES.md).

---

### CC1001: Avoid inefficient LINQ

**Domain:** clean-code &nbsp;·&nbsp; **Category:** performance &nbsp;·&nbsp; **Severity:** error &nbsp;·&nbsp; **Type:** deterministic

**Implemented as an analyzer.** See [`docs/rules/CC1001.md`](CC1001.md).

---

### CC1002: Prevent controller bloat

**Domain:** clean-code &nbsp;·&nbsp; **Category:** architecture &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Controllers containing orchestration and business logic become difficult to maintain and test.

**Why it matters:**
- Violates separation of concerns
- Creates tightly coupled endpoints
- Reduces readability and testability

**Indicators to look for:**
- Controller exceeds 300 lines
- Controller injects more than 5 services
- Contains business rules and orchestration logic

**Fix:**
- Move orchestration into application services
- Use CQRS handlers
- Extract validation and mapping logic

**AI review prompt:**
- Review controller responsibilities.
- Ensure controllers remain thin orchestration boundaries.

**Example:**

- Bad: `Controller handles validation, persistence, retries, and mapping.`
- Good: `Controller delegates work to application services or handlers.`

---

### CC1003: Avoid magic constants

**Domain:** clean-code &nbsp;·&nbsp; **Category:** maintainability &nbsp;·&nbsp; **Severity:** error &nbsp;·&nbsp; **Type:** deterministic

**Implemented as an analyzer.** See [`docs/rules/CC1003.md`](CC1003.md).

---

### CC1004: Avoid blocking async code

**Domain:** clean-code &nbsp;·&nbsp; **Category:** async &nbsp;·&nbsp; **Severity:** error &nbsp;·&nbsp; **Type:** deterministic

**Implemented as an analyzer.** See [`docs/rules/CC1004.md`](CC1004.md).

---
