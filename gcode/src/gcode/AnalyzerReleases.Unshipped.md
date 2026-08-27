; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
GCODE0001 | Reliability | Warning | EmptyCatchBlockAnalyzer, [Documentation](docs/rules/GCODE0001.md)
CC1001 | Performance | Warning | InefficientLinqAnalyzer, [Documentation](docs/rules/CC1001.md)
CC1003 | Maintainability | Warning | MagicConstantsAnalyzer, [Documentation](docs/rules/CC1003.md)
CC1004 | Async | Warning | BlockingAsyncAnalyzer, [Documentation](docs/rules/CC1004.md)
DI1002 | DependencyInjection | Warning | DuplicateServiceRegistrationAnalyzer, [Documentation](docs/rules/DI1002.md)
DI1003 | DependencyInjection | Warning | CircularDependencyAnalyzer, [Documentation](docs/rules/DI1003.md)
MEM1001 | Memory | Warning | UndetachedEventHandlerAnalyzer, [Documentation](docs/rules/MEM1001.md)
MEM1003 | Memory | Warning | MissingSuppressFinalizeAnalyzer, [Documentation](docs/rules/MEM1003.md)
ORG1016 | Performance | Info | StringConcatenationInLoopAnalyzer, [Documentation](docs/rules/ORG1016.md)
