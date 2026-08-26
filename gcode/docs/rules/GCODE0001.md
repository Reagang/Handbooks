# GCODE0001: Empty catch block

| Property | Value       |
|----------|-------------|
| Category | Reliability |
| Severity | Warning     |

## Cause

A `catch` block contains no statements and no explanatory comment, so an
exception is caught and silently discarded.

## Rule description

Swallowing an exception without handling it, logging it, or at least
documenting why it's safe to ignore hides failures that are hard to
diagnose later. This rule flags empty `catch` blocks so the decision to
ignore an exception is always deliberate and visible.

## How to fix violations

Do one of the following:

- Handle the exception (retry, fall back, surface a user-facing error).
- Log it.
- Narrow the `catch` to the specific exception type you intend to ignore,
  and add a comment explaining why.

## Example

```csharp
// Violation
try
{
    File.Delete(path);
}
catch (Exception)
{
}
```

```csharp
// Fixed
try
{
    File.Delete(path);
}
catch (IOException ex)
{
    // Best-effort cleanup; the file may already be gone.
    logger.LogWarning(ex, "Could not delete {Path}", path);
}
```

## When to suppress

Suppress with a comment inside the `catch` block explaining why the
exception is intentionally ignored, rather than disabling the rule.
