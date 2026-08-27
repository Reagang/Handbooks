using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gcode.Analyzers.Rules.MemorySafety;

/// <summary>
/// MEM1001: flags an event subscription (<c>publisher.Event += Handler;</c>)
/// in an <see cref="System.IDisposable"/> class with no matching
/// unsubscription (<c>publisher.Event -= Handler;</c>) anywhere in the same
/// class. An undetached handler keeps the publisher (and everything the
/// subscriber captures) alive for as long as the publisher lives.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UndetachedEventHandlerAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MEM1001";

    private static readonly LocalizableString Title = "Event handler never detached";

    private static readonly LocalizableString MessageFormat =
        "'{0}' is subscribed here but never unsubscribed ('-=') anywhere in '{1}'";

    private static readonly LocalizableString Description =
        "A subscribed event handler keeps the publisher - and anything the " +
        "handler captures - alive for as long as the publisher lives. In a " +
        "type that implements IDisposable, unsubscribe every handler it " +
        "attaches, typically from Dispose().";

    private const string Category = "Memory";

    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: $"https://github.com/reagang/handbooks/blob/main/gcode/docs/rules/{DiagnosticId}.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken) is not { } classSymbol ||
            !classSymbol.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable"))
        {
            return;
        }

        var assignments = classDeclaration.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(a => a.IsKind(SyntaxKind.AddAssignmentExpression) || a.IsKind(SyntaxKind.SubtractAssignmentExpression))
            .ToList();

        var unsubscribed = new HashSet<(ISymbol Event, string Target)>(EventKeyComparer.Instance);
        foreach (var assignment in assignments.Where(a => a.IsKind(SyntaxKind.SubtractAssignmentExpression)))
        {
            if (TryGetEventKey(context, assignment, out var key))
            {
                unsubscribed.Add(key);
            }
        }

        foreach (var assignment in assignments.Where(a => a.IsKind(SyntaxKind.AddAssignmentExpression)))
        {
            if (TryGetEventKey(context, assignment, out var key) && !unsubscribed.Contains(key))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    assignment.GetLocation(),
                    key.Event.Name,
                    classDeclaration.Identifier.Text));
            }
        }
    }

    private static bool TryGetEventKey(
        SyntaxNodeAnalysisContext context,
        AssignmentExpressionSyntax assignment,
        out (ISymbol Event, string Target) key)
    {
        key = default;

        if (context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol is not IEventSymbol eventSymbol)
        {
            return false;
        }

        var target = assignment.Left is MemberAccessExpressionSyntax memberAccess
            ? memberAccess.Expression.ToString()
            : "this";

        key = (eventSymbol, target);
        return true;
    }

    private sealed class EventKeyComparer : IEqualityComparer<(ISymbol Event, string Target)>
    {
        public static readonly EventKeyComparer Instance = new();

        public bool Equals((ISymbol Event, string Target) x, (ISymbol Event, string Target) y) =>
            SymbolEqualityComparer.Default.Equals(x.Event, y.Event) && x.Target == y.Target;

        public int GetHashCode((ISymbol Event, string Target) obj) =>
            SymbolEqualityComparer.Default.GetHashCode(obj.Event) ^ obj.Target.GetHashCode();
    }
}
