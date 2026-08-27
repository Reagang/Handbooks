using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gcode.Analyzers.Rules;

/// <summary>
/// GCODE0001: flags catch blocks that swallow an exception without handling or
/// logging it. Serves as the template for adding new rules to this project -
/// see README.md.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EmptyCatchBlockAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "GCODE0001";

    private static readonly LocalizableString Title =
        "Empty catch block";

    private static readonly LocalizableString MessageFormat =
        "Catch block swallows the exception without handling or logging it";

    private static readonly LocalizableString Description =
        "Catching an exception and doing nothing with it hides failures. Either " +
        "handle the exception, log it, or narrow the catch to the specific type " +
        "you intend to ignore.";

    private const string Category = "Reliability";

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

        context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
    }

    private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
    {
        var catchClause = (CatchClauseSyntax)context.Node;

        if (catchClause.Block.Statements.Count > 0)
        {
            return;
        }

        if (catchClause.Block.ContainsDirectives ||
            HasComment(catchClause.Block))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, catchClause.GetLocation()));
    }

    private static bool HasComment(BlockSyntax block) =>
        block.DescendantTrivia().Any(trivia =>
            trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));
}
