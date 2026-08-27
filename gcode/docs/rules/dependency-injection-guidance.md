# Dependency Injection pack - rule guidance

Rules from the dependency-injection pack that aren't implemented as a Roslyn analyzer here - detecting them reliably needs contextual reasoning about intent, not just syntax.

Machine-readable source: [`catalog/dependency-injection.json`](../../catalog/dependency-injection.json).

---

### DI1001: Prevent misuse of dependency injection

**Domain:** dependency-injection &nbsp;·&nbsp; **Category:** architecture &nbsp;·&nbsp; **Severity:** critical &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Improper dependency injection usage creates tightly coupled and difficult-to-maintain systems.

**Why it matters:**
- Creates hidden runtime dependencies
- Makes testing more difficult
- Introduces service resolution ambiguity
- Can create circular dependency chains

**Indicators to look for:**
- Injecting IServiceProvider
- Service locator usage
- Runtime dependency resolution

**Fix:**
- Use constructor injection
- Avoid service locator patterns
- Make dependencies explicit

**AI review prompt:**
- Review dependency boundaries.
- Ensure dependencies are explicit and testable.

**Example:**

- Bad: `serviceProvider.GetService<IMyService>()`
- Good: `Inject IMyService through constructor injection`

---

### DI1002: Avoid duplicate service registrations

**Domain:** dependency-injection &nbsp;·&nbsp; **Category:** registration &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** deterministic

**Implemented as an analyzer.** See [`docs/rules/DI1002.md`](DI1002.md).

---

### DI1003: Prevent circular dependencies

**Domain:** dependency-injection &nbsp;·&nbsp; **Category:** architecture &nbsp;·&nbsp; **Severity:** critical &nbsp;·&nbsp; **Type:** deterministic

**Implemented as an analyzer.** See [`docs/rules/DI1003.md`](DI1003.md).

---

### DI1004: Prevent excessive constructor dependencies

**Domain:** dependency-injection &nbsp;·&nbsp; **Category:** maintainability &nbsp;·&nbsp; **Severity:** info &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Too many constructor dependencies often indicate orchestration leakage or god classes.

**Why it matters:**
- Violates SRP
- Increases object graph complexity
- Makes testing difficult

**Indicators to look for:**
- More than 5 constructor parameters
- Large orchestration services

**Fix:**
- Split responsibilities
- Extract orchestration layers
- Reduce coupling

**AI review prompt:**
- Review service cohesion.
- Check for orchestration leakage.

**Example:**

- Bad: `Service with 10 injected dependencies`
- Good: `Focused cohesive service`

---

### DI1005: Validate dependency injection registrations on startup

**Domain:** dependency-injection &nbsp;·&nbsp; **Category:** runtime-governance &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Invalid service registrations may only fail at runtime without startup validation.

**Why it matters:**
- Runtime failures are difficult to diagnose
- Missing registrations may remain hidden
- Invalid scopes can cause memory leaks

**Indicators to look for:**
- No service provider validation
- Missing ValidateScopes
- Missing ValidateOnBuild

**Fix:**
- Enable service provider validation
- Validate scopes during startup
- Fail fast on invalid registrations

**AI review prompt:**
- Review DI startup validation.
- Ensure invalid scopes are detected early.

**Example:**

- Bad: `Builder without service validation`
- Good: `ValidateScopes and ValidateOnBuild enabled`

---
