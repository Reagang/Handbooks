using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gcode.Analyzers.Rules.Organization;

/// <summary>
/// ORG1016: flags <c>str += ...</c> on a string-typed variable inside a
/// loop. Each iteration allocates a new string; a StringBuilder amortizes
/// that cost.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StringConcatenationInLoopAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ORG1016";

    private static readonly LocalizableString Title = "String concatenation in a loop";

    private static readonly LocalizableString MessageFormat =
        "Use a StringBuilder instead of '+=' to build a string across loop iterations";

    private static readonly LocalizableString Description =
        "Repeated string concatenation inside a loop allocates a new string on " +
        "every iteration. Use a StringBuilder and call ToString() once after " +
        "the loop.";

    private const string Category = "Performance";

    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: $"https://github.com/reagang/handbooks/blob/main/gcode/docs/rules/{DiagnosticId}.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.AddAssignmentExpression);
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;

        if (!IsInsideLoop(assignment))
        {
            return;
        }

        var leftType = context.SemanticModel.GetTypeInfo(assignment.Left, context.CancellationToken).Type;
        if (leftType?.SpecialType == SpecialType.System_String)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.GetLocation()));
        }
    }

    private static bool IsInsideLoop(SyntaxNode node)
    {
        foreach (var ancestor in node.Ancestors())
        {
            switch (ancestor)
            {
                case ForStatementSyntax:
                case ForEachStatementSyntax:
                case WhileStatementSyntax:
                case DoStatementSyntax:
                    return true;
                case MemberDeclarationSyntax:
                case AnonymousFunctionExpressionSyntax:
                case LocalFunctionStatementSyntax:
                    // Stop at the enclosing method/lambda boundary - a loop in an
                    // outer scope doesn't make an assignment inside a nested
                    // function "inside the loop" on every outer iteration.
                    return false;
            }
        }

        return false;
    }
}
