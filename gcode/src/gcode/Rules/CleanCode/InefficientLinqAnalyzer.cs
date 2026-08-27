using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gcode.Analyzers.Rules.CleanCode;

/// <summary>
/// CC1001: flags a LINQ chain that materializes with <c>ToList()</c>/<c>ToArray()</c>
/// and then immediately calls another LINQ operator on the result, e.g.
/// <c>orders.Where(x => x.Active).ToList().FirstOrDefault()</c>. The
/// intermediate list is wasted allocation; the operators can be composed
/// directly over the original sequence.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InefficientLinqAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CC1001";

    private static readonly LocalizableString Title = "Unnecessary LINQ materialization";

    private static readonly LocalizableString MessageFormat =
        "'{0}()' materializes the sequence right before another LINQ call; compose the query instead of allocating an intermediate collection";

    private static readonly LocalizableString Description =
        "Calling ToList()/ToArray() and then immediately chaining another LINQ " +
        "operator allocates an intermediate collection that is discarded right " +
        "away. Compose the operators over the original IEnumerable<T> instead.";

    private const string Category = "Performance";

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

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax outerAccess)
        {
            return;
        }

        if (outerAccess.Expression is not InvocationExpressionSyntax innerInvocation ||
            innerInvocation.Expression is not MemberAccessExpressionSyntax innerAccess)
        {
            return;
        }

        var materializer = innerAccess.Name.Identifier.Text;
        if (materializer is not ("ToList" or "ToArray"))
        {
            return;
        }

        if (!IsLinqExtensionMethod(context, innerAccess) || !IsLinqExtensionMethod(context, outerAccess))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, innerInvocation.GetLocation(), materializer));
    }

    private static bool IsLinqExtensionMethod(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax memberAccess)
    {
        var symbol = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol;
        var containingType = (symbol as IMethodSymbol)?.ContainingType;
        return containingType is { Name: "Enumerable", ContainingNamespace: { Name: "Linq", ContainingNamespace.Name: "System" } };
    }
}
