# Memory Safety pack - rule guidance

Rules from the memory-safety pack that aren't implemented as a Roslyn analyzer here - reliably detecting "unbounded" or "long-lived" requires knowing the type's intended usage, which syntax alone can't tell.

Machine-readable source: [`catalog/memory-safety.json`](../../catalog/memory-safety.json). For the full deduplicated list across all packs, see [`ALL_RULES.md`](ALL_RULES.md).

---

### MEM1001: Detach event handlers to prevent memory leaks

**Domain:** memory-safety &nbsp;·&nbsp; **Category:** memory-management &nbsp;·&nbsp; **Severity:** critical &nbsp;·&nbsp; **Type:** deterministic

**Implemented as an analyzer.** See [`docs/rules/MEM1001.md`](MEM1001.md).

---

### MEM1002: Avoid async closures without cancellation

**Domain:** memory-safety &nbsp;·&nbsp; **Category:** async &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Async closures without cancellation handling can retain references and create lifecycle issues.

**Why it matters:**
- Captures object references
- Can outlive intended scope
- Increases memory retention risk

**Indicators to look for:**
- Task.Delay without cancellation token
- Long-running background tasks
- Captured references inside async lambdas

**Fix:**
- Use CancellationToken
- Avoid unnecessary captures
- Dispose long-running operations

**AI review prompt:**
- Review async closure lifetimes.
- Ensure background operations support cancellation.

**Example:**

- Bad: `await Task.Delay(TimeSpan.FromMinutes(5));`
- Good: `await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);`

---

### MEM1003: Require GC.SuppressFinalize when implementing finalizers

**Domain:** memory-safety &nbsp;·&nbsp; **Category:** resource-management &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** deterministic

**Implemented as an analyzer.** See [`docs/rules/MEM1003.md`](MEM1003.md).

---

### MEM1004: Avoid long-lived timers without disposal

**Domain:** memory-safety &nbsp;·&nbsp; **Category:** resource-management &nbsp;·&nbsp; **Severity:** critical &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Undisposed timers can retain references and continue executing indefinitely.

**Why it matters:**
- Creates hidden background execution
- Retains captured references
- Causes memory and CPU leaks

**Indicators to look for:**
- Timer instances without disposal
- Background timers without cancellation

**Fix:**
- Dispose timers properly
- Use hosted services with cancellation
- Avoid unmanaged timer lifetimes

**AI review prompt:**
- Review timer lifecycle management.
- Ensure timers are disposed and cancellable.

**Example:**

- Bad: `new Timer(callback, null, 0, 1000);`
- Good: `Dispose timer during shutdown`

---

### MEM1005: Avoid unbounded in-memory collections

**Domain:** memory-safety &nbsp;·&nbsp; **Category:** allocations &nbsp;·&nbsp; **Severity:** critical &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Unbounded collections can exhaust memory under load and destabilize services.

**Why it matters:**
- Memory usage grows indefinitely
- Can trigger OOM conditions
- Increases GC pressure

**Indicators to look for:**
- ConcurrentDictionary without eviction
- Unbounded queues
- Large in-memory caching without limits

**Fix:**
- Add eviction policies
- Use bounded channels/queues
- Implement memory limits

**AI review prompt:**
- Review in-memory collection growth.
- Ensure memory usage remains bounded.

**Example:**

- Bad: `static List<Order> Orders = new();`
- Good: `Use bounded cache with expiration`

---
