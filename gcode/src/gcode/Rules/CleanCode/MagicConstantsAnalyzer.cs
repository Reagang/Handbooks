using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gcode.Analyzers.Rules.CleanCode;

/// <summary>
/// CC1003: flags hardcoded numeric/string literals used in comparisons or
/// switch labels (e.g. <c>if (status == 5)</c>), where a named constant or
/// enum would make the intent clear. 0, 1, -1, and "" are allowed as they
/// read as self-explanatory sentinels.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MagicConstantsAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CC1003";

    private static readonly LocalizableString Title = "Magic constant in comparison";

    private static readonly LocalizableString MessageFormat =
        "Replace magic value '{0}' with a named constant or enum member";

    private static readonly LocalizableString Description =
        "Hardcoded numeric or string literals used in comparisons hide their " +
        "meaning and get duplicated across the codebase. Use a named constant, " +
        "enum, or strongly typed option instead.";

    private const string Category = "Maintainability";

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

    private static readonly SyntaxKind[] ComparisonKinds =
    {
        SyntaxKind.EqualsExpression,
        SyntaxKind.NotEqualsExpression,
        SyntaxKind.LessThanExpression,
        SyntaxKind.GreaterThanExpression,
        SyntaxKind.LessThanOrEqualExpression,
        SyntaxKind.GreaterThanOrEqualExpression,
    };

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, ComparisonKinds);
        context.RegisterSyntaxNodeAction(AnalyzeCaseSwitchLabel, SyntaxKind.CaseSwitchLabel);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binary = (BinaryExpressionSyntax)context.Node;
        CheckOperand(context, binary.Left);
        CheckOperand(context, binary.Right);
    }

    private static void AnalyzeCaseSwitchLabel(SyntaxNodeAnalysisContext context)
    {
        var label = (CaseSwitchLabelSyntax)context.Node;
        CheckOperand(context, label.Value);
    }

    private static void CheckOperand(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        if (IsAllowedLiteral(expression) || expression is not LiteralExpressionSyntax literal)
        {
            return;
        }

        if (literal.IsKind(SyntaxKind.NumericLiteralExpression) || literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, literal.GetLocation(), literal.Token.ValueText));
        }
    }

    private static bool IsAllowedLiteral(ExpressionSyntax expression)
    {
        if (expression is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression } unary)
        {
            return IsNumericLiteralOf(unary.Operand, "1");
        }

        return expression switch
        {
            LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NumericLiteralExpression) =>
                literal.Token.ValueText is "0" or "1",
            LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) =>
                literal.Token.ValueText.Length == 0,
            _ => false,
        };
    }

    private static bool IsNumericLiteralOf(ExpressionSyntax expression, string value) =>
        expression is LiteralExpressionSyntax literal
        && literal.IsKind(SyntaxKind.NumericLiteralExpression)
        && literal.Token.ValueText == value;
}
