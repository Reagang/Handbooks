# Observability pack - rule guidance

None of this pack's rules are implemented as analyzers: the pack's own catalog marks every one of them `governance` or `heuristic` - telemetry coverage, resiliency visibility, and sensitive-data judgment calls aren't things syntax alone can verify without a high false-positive rate. `OBS1004` (silent failure paths) is the exception worth calling out: its concrete case, an empty `catch` block, already ships as GCODE0001.

Machine-readable source: [`catalog/observability.json`](../../catalog/observability.json). For the full deduplicated list across all packs, see [`ALL_RULES.md`](ALL_RULES.md).

---

### OBS1001: Require telemetry and structured logging

**Domain:** observability &nbsp;·&nbsp; **Category:** telemetry &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Systems without telemetry and structured logging are difficult to operate and troubleshoot.

**Why it matters:**
- Reduces production visibility
- Makes incident investigation difficult
- Prevents proactive monitoring
- Impacts distributed tracing

**Indicators to look for:**
- No structured logs
- Missing telemetry events
- No metrics or traces

**Fix:**
- Use structured logging
- Emit metrics and traces
- Add centralized telemetry collection

**AI review prompt:**
- Review telemetry coverage across the service.
- Ensure logs are structured and correlated.

**Example:**

- Bad: `Console.WriteLine('Error occurred')`
- Good: `logger.LogError(ex, 'Failed processing order {OrderId}', orderId)`

---

### OBS1002: Require retry and resiliency telemetry

**Domain:** observability &nbsp;·&nbsp; **Category:** resiliency &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Retries and resiliency events without telemetry make distributed failures difficult to diagnose.

**Why it matters:**
- Hidden retries increase latency
- Circuit breaker failures become invisible
- Operational troubleshooting becomes difficult

**Indicators to look for:**
- Retry policies without logging
- Circuit breaker failures not tracked
- Missing resiliency metrics

**Fix:**
- Emit retry metrics
- Track transient failures
- Log circuit breaker state changes

**AI review prompt:**
- Review resiliency observability.
- Ensure retries and circuit breakers are visible.

**Example:**

- Bad: `Retry policy without telemetry`
- Good: `Retry attempts and failures logged with metrics`

---

### OBS1003: Prevent sensitive data leakage in telemetry

**Domain:** observability &nbsp;·&nbsp; **Category:** security &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Sensitive data in logs and telemetry creates security and compliance risks.

**Why it matters:**
- Credentials and PII may leak
- Violates compliance standards
- Increases security exposure

**Indicators to look for:**
- Logging tokens or passwords
- PII included in telemetry
- Sensitive headers logged

**Fix:**
- Sanitize sensitive values
- Mask credentials and tokens
- Use structured logging filters

**AI review prompt:**
- Review logs for sensitive data leakage.
- Ensure telemetry follows compliance standards.

**Example:**

- Bad: `logger.LogInformation('Password: {Password}', password)`
- Good: `logger.LogInformation('Authentication request received')`

---

### OBS1004: Detect dead code paths and silent failures

**Domain:** observability &nbsp;·&nbsp; **Category:** diagnostics &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** heuristic

**Covered by [`GCODE0001`](GCODE0001.md)** - same check, documented there.

> Dead code paths and silent failures reduce visibility into runtime behavior.

---

### OBS1005: Review dependency injection observability boundaries

**Domain:** observability &nbsp;·&nbsp; **Category:** dependency-injection &nbsp;·&nbsp; **Severity:** info &nbsp;·&nbsp; **Type:** heuristic

**Covered by [`DI1001`](dependency-injection-guidance.md#di1001-prevent-misuse-of-dependency-injection)** - same check, documented there.

> Improper dependency injection boundaries can reduce visibility into runtime behavior.

---
