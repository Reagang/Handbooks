# Organization Philosophy pack - rule guidance

This pack restates the org-wide engineering philosophy; several entries just point back at a rule with fuller detail in a domain-specific pack (`clean-code`, `dependency-injection`, etc.) rather than needing a second, duplicate analyzer. The rest are architecture/process guidance no syntax check can safely automate.

Machine-readable source: [`catalog/organization.json`](../../catalog/organization.json). For the full deduplicated list across all packs, see [`ALL_RULES.md`](ALL_RULES.md).

---

### ORG1001: Prefer property patterns for DTO and JSON validation

**Domain:** readability &nbsp;·&nbsp; **Category:** pattern-matching &nbsp;·&nbsp; **Severity:** info &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Traditional nested conditional checks reduce readability and increase validation complexity.

**Why it matters:**
- Property patterns improve readability
- Nested matching becomes easier to follow
- Reduces validation boilerplate

**Fix:**
- Use C# property patterns
- Leverage switch expressions where appropriate

---

### ORG1002: Avoid blocking async operations

**Domain:** async &nbsp;·&nbsp; **Category:** performance &nbsp;·&nbsp; **Severity:** critical &nbsp;·&nbsp; **Type:** deterministic

**Covered by [`CC1004`](CC1004.md)** - same check, documented there.

> Blocking async code reduces scalability and may deadlock applications.

---

### ORG1003: Validate options during startup

**Domain:** dependency-injection &nbsp;·&nbsp; **Category:** runtime-governance &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Invalid configuration should fail during startup instead of runtime.

**Fix:**
- Use ValidateOnStart()
- Use configuration validation

---

### ORG1004: Validate dependency injection scopes and registrations

**Domain:** dependency-injection &nbsp;·&nbsp; **Category:** runtime-governance &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Covered by [`DI1005`](dependency-injection-guidance.md#di1005-validate-dependency-injection-registrations-on-startup)** - same check, documented there.

> Incorrect scopes and missing registrations create runtime instability.

---

### ORG1005: Validate ASP.NET middleware order

**Domain:** architecture &nbsp;·&nbsp; **Category:** runtime-pipeline &nbsp;·&nbsp; **Severity:** critical &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Incorrect middleware order may break authentication, authorization, routing, and telemetry.

**Fix:**
- Validate middleware ordering during startup
- Ensure auth and routing are correctly configured

---

### ORG1006: Never automatically modify appsettings files

**Domain:** governance &nbsp;·&nbsp; **Category:** configuration-management &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Automatic modification of configuration files may create unsafe or unintended configuration drift.

**Fix:**
- Provide copy-paste configuration examples only
- Allow engineers to manually review config changes

---

### ORG1007: Avoid magic strings and numbers

**Domain:** clean-code &nbsp;·&nbsp; **Category:** maintainability &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** deterministic

**Covered by [`CC1003`](CC1003.md)** - same check, documented there.

> Magic values reduce readability and increase maintenance complexity.

---

### ORG1008: Encourage helper methods and extension methods

**Domain:** maintainability &nbsp;·&nbsp; **Category:** reusability &nbsp;·&nbsp; **Severity:** info &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Duplicated logic reduces maintainability and readability.

**Fix:**
- Extract reusable helpers
- Create extension methods for shared logic

---

### ORG1009: Prefer static private methods where possible

**Domain:** performance &nbsp;·&nbsp; **Category:** memory &nbsp;·&nbsp; **Severity:** info &nbsp;·&nbsp; **Type:** deterministic

**Covered by the built-in .NET analyzer `CA1822`** ("Mark members as static") - enable it instead of reimplementing it here.

> Non-static methods may capture unnecessary instance state.

---

### ORG1010: Break code into smaller readable components

**Domain:** architecture &nbsp;·&nbsp; **Category:** maintainability &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Large classes and methods reduce readability and maintainability.

**Fix:**
- Extract focused components
- Use descriptive naming
- Split orchestration responsibilities

---

### ORG1011: Avoid god classes and god methods

**Domain:** architecture &nbsp;·&nbsp; **Category:** maintainability &nbsp;·&nbsp; **Severity:** critical &nbsp;·&nbsp; **Type:** heuristic

**Problem:** God classes centralize too many responsibilities and create tight coupling.

**Fix:**
- Split responsibilities
- Introduce cohesive services
- Apply SRP

---

### ORG1012: Ensure methods have a single responsibility

**Domain:** clean-code &nbsp;·&nbsp; **Category:** maintainability &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Methods performing multiple actions become difficult to test and maintain.

**Fix:**
- Extract smaller focused methods
- Separate orchestration from business logic

---

### ORG1013: Prefer records over classes where applicable

**Domain:** architecture &nbsp;·&nbsp; **Category:** immutability &nbsp;·&nbsp; **Severity:** info &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Mutable DTOs and value objects increase accidental state mutation risks.

**Fix:**
- Use records for immutable data structures
- Prefer immutability by default

---

### ORG1014: Avoid multiple nested loops

**Domain:** performance &nbsp;·&nbsp; **Category:** algorithms &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Nested loops may create significant performance bottlenecks.

**Fix:**
- Use dictionaries/lookups
- Precompute indexes
- Refactor algorithms

---

### ORG1015: Add XML documentation where appropriate

**Domain:** maintainability &nbsp;·&nbsp; **Category:** documentation &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Lack of documentation reduces maintainability and onboarding efficiency.

**Fix:**
- Document public APIs
- Add XML summaries for complex logic

---

### ORG1016: Use StringBuilder for repeated concatenation

**Domain:** performance &nbsp;·&nbsp; **Category:** allocations &nbsp;·&nbsp; **Severity:** info &nbsp;·&nbsp; **Type:** deterministic

**Implemented as an analyzer.** See [`docs/rules/ORG1016.md`](ORG1016.md).

---

### ORG1017: Organize code into appropriate architectural boundaries

**Domain:** architecture &nbsp;·&nbsp; **Category:** project-structure &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Poor file organization reduces maintainability and discoverability.

**Fix:**
- Separate handlers, services, processors, mappers, and helpers
- Align folders to architecture boundaries

---

### ORG1018: Extract complex validation logic into dedicated components

**Domain:** maintainability &nbsp;·&nbsp; **Category:** readability &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** heuristic

**Problem:** Large inline validation logic reduces readability and reuse.

**Fix:**
- Extract validators/helpers
- Use descriptive method naming

---

### ORG1019: Use named or typed HttpClients

**Domain:** distributed-systems &nbsp;·&nbsp; **Category:** http &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Direct HttpClient usage reduces resiliency and observability.

**Fix:**
- Use IHttpClientFactory
- Use named or typed clients

---

### ORG1020: Retries must implement backoff strategy

**Domain:** resiliency &nbsp;·&nbsp; **Category:** distributed-systems &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Retries without backoff can amplify outages and overload dependencies.

**Fix:**
- Use exponential backoff
- Add jitter
- Limit retry attempts

---

### ORG1021: Prefer service discovery over hardcoded endpoints

**Domain:** cloud-native &nbsp;·&nbsp; **Category:** service-discovery &nbsp;·&nbsp; **Severity:** warning &nbsp;·&nbsp; **Type:** governance

**Problem:** Hardcoded URLs reduce portability, failover capability, and scalability.

**Fix:**
- Use service discovery
- Use centralized configuration
- Use ingress/load balancers

---
