using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gcode.Analyzers.Rules.MemorySafety;

/// <summary>
/// MEM1003: flags a class that declares a finalizer but never calls
/// <c>GC.SuppressFinalize(this)</c> anywhere in the class. A finalized
/// object survives an extra GC generation before collection; disposing it
/// deterministically should suppress that finalization.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingSuppressFinalizeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MEM1003";

    private static readonly LocalizableString Title = "Finalizer without GC.SuppressFinalize";

    private static readonly LocalizableString MessageFormat =
        "'{0}' declares a finalizer but never calls GC.SuppressFinalize(this)";

    private static readonly LocalizableString Description =
        "A finalized object survives an extra GC generation before it can be " +
        "collected. When the type is disposed deterministically, call " +
        "GC.SuppressFinalize(this) from Dispose() so the finalizer is skipped.";

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

        var finalizer = classDeclaration.Members.OfType<DestructorDeclarationSyntax>().FirstOrDefault();
        if (finalizer is null)
        {
            return;
        }

        var callsSuppressFinalize = classDeclaration
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation => IsSuppressFinalizeCall(context, invocation));

        if (!callsSuppressFinalize)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, finalizer.GetLocation(), classDeclaration.Identifier.Text));
        }
    }

    private static bool IsSuppressFinalizeCall(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.Text: "SuppressFinalize" } memberAccess)
        {
            return false;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol;
        return symbol is IMethodSymbol { ContainingType.Name: "GC", ContainingType.ContainingNamespace.Name: "System" };
    }
}
