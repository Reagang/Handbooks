using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gcode.Analyzers.Rules.DependencyInjection;

/// <summary>
/// DI1003: flags a cycle in constructor-injected dependencies (A depends on
/// B, B depends on A - directly or through intermediates). Best-effort: it
/// only follows a dependency through an interface when exactly one class in
/// the compilation implements it, and only considers each class's
/// constructor with the most parameters.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CircularDependencyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DI1003";

    private static readonly LocalizableString Title = "Circular constructor dependency";

    private static readonly LocalizableString MessageFormat = "Circular dependency: {0}";

    private static readonly LocalizableString Description =
        "A cycle in constructor-injected dependencies means none of the types " +
        "involved can be constructed without the others already existing - the " +
        "container can only break the cycle via property/method injection or a " +
        "factory, and the underlying design usually needs an interface or " +
        "mediator to break the coupling instead.";

    private const string Category = "DependencyInjection";

    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: $"https://github.com/reagang/handbooks/blob/main/gcode/docs/rules/{DiagnosticId}.md",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var classes = CollectSourceClasses(context.Compilation);
        if (classes.Count == 0)
        {
            return;
        }

        var classSet = new HashSet<INamedTypeSymbol>(classes, SymbolEqualityComparer.Default);
        var implementors = BuildInterfaceImplementorMap(classes);
        var graph = BuildDependencyGraph(classes, classSet, implementors);

        var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var reportedCycles = new HashSet<string>();

        foreach (var node in graph.Keys)
        {
            if (!visited.Contains(node))
            {
                DetectCycles(node, graph, visited, new List<INamedTypeSymbol>(), reportedCycles, context);
            }
        }
    }

    private static void DetectCycles(
        INamedTypeSymbol node,
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> graph,
        HashSet<INamedTypeSymbol> visited,
        List<INamedTypeSymbol> stack,
        HashSet<string> reportedCycles,
        CompilationAnalysisContext context)
    {
        visited.Add(node);
        stack.Add(node);

        if (graph.TryGetValue(node, out var dependencies))
        {
            foreach (var dependency in dependencies)
            {
                var cycleStart = IndexOfSymbol(stack, dependency);
                if (cycleStart >= 0)
                {
                    ReportCycle(stack, cycleStart, reportedCycles, context);
                }
                else if (!visited.Contains(dependency))
                {
                    DetectCycles(dependency, graph, visited, stack, reportedCycles, context);
                }
            }
        }

        stack.RemoveAt(stack.Count - 1);
    }

    private static void ReportCycle(
        List<INamedTypeSymbol> stack,
        int cycleStart,
        HashSet<string> reportedCycles,
        CompilationAnalysisContext context)
    {
        var cycle = stack.Skip(cycleStart).ToList();
        var dedupeKey = string.Join("|", cycle.Select(t => t.ToDisplayString()).OrderBy(n => n, System.StringComparer.Ordinal));
        if (!reportedCycles.Add(dedupeKey))
        {
            return;
        }

        var path = string.Join(" -> ", cycle.Select(t => t.Name).Append(cycle[0].Name));
        var location = cycle[0].DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, path));
    }

    private static int IndexOfSymbol(List<INamedTypeSymbol> stack, INamedTypeSymbol symbol)
    {
        for (var i = 0; i < stack.Count; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(stack[i], symbol))
            {
                return i;
            }
        }

        return -1;
    }

    private static List<INamedTypeSymbol> CollectSourceClasses(Compilation compilation)
    {
        var result = new List<INamedTypeSymbol>();
        CollectSourceClasses(compilation.GlobalNamespace, result);
        return result;
    }

    private static void CollectSourceClasses(INamespaceOrTypeSymbol container, List<INamedTypeSymbol> result)
    {
        foreach (var member in container.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol nestedNamespace:
                    CollectSourceClasses(nestedNamespace, result);
                    break;
                case INamedTypeSymbol { TypeKind: TypeKind.Class, IsAbstract: false } type
                    when type.DeclaringSyntaxReferences.Length > 0:
                    result.Add(type);
                    CollectSourceClasses(type, result);
                    break;
                case INamedTypeSymbol nestedType:
                    CollectSourceClasses(nestedType, result);
                    break;
            }
        }
    }

    private static Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> BuildInterfaceImplementorMap(List<INamedTypeSymbol> classes)
    {
        var map = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(SymbolEqualityComparer.Default);

        foreach (var classSymbol in classes)
        {
            foreach (var iface in classSymbol.AllInterfaces)
            {
                if (!map.TryGetValue(iface, out var implementors))
                {
                    implementors = new List<INamedTypeSymbol>();
                    map[iface] = implementors;
                }

                implementors.Add(classSymbol);
            }
        }

        return map;
    }

    private static Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> BuildDependencyGraph(
        List<INamedTypeSymbol> classes,
        HashSet<INamedTypeSymbol> classSet,
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> implementors)
    {
        var graph = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(SymbolEqualityComparer.Default);

        foreach (var classSymbol in classes)
        {
            var constructor = classSymbol.Constructors
                .Where(c => !c.IsStatic)
                .OrderByDescending(c => c.Parameters.Length)
                .FirstOrDefault();

            var dependencies = new List<INamedTypeSymbol>();
            if (constructor is not null)
            {
                foreach (var parameter in constructor.Parameters)
                {
                    if (parameter.Type is not INamedTypeSymbol parameterType)
                    {
                        continue;
                    }

                    if (parameterType.TypeKind == TypeKind.Interface)
                    {
                        if (implementors.TryGetValue(parameterType, out var candidates) && candidates.Count == 1)
                        {
                            dependencies.Add(candidates[0]);
                        }
                    }
                    else if (parameterType.TypeKind == TypeKind.Class && classSet.Contains(parameterType))
                    {
                        dependencies.Add(parameterType);
                    }
                }
            }

            graph[classSymbol] = dependencies;
        }

        return graph;
    }
}
