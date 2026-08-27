using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gcode.Analyzers.Rules.CleanCode;

/// <summary>
/// CC1004: flags sync-over-async code - <c>.Result</c>, <c>.Wait()</c>,
/// <c>GetAwaiter().GetResult()</c>, and <c>Thread.Sleep</c> inside an async
/// method - which blocks a thread pool thread and can deadlock.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BlockingAsyncAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CC1004";

    private static readonly LocalizableString Title = "Blocking call on async code";

    private static readonly LocalizableString MessageFormat =
        "'{0}' blocks the calling thread on async code; use 'await' instead";

    private static readonly LocalizableString Description =
        "Blocking on a Task (via .Result, .Wait(), GetAwaiter().GetResult(), or " +
        "Thread.Sleep inside an async method) occupies a thread pool thread and " +
        "can deadlock under a synchronization context. Await the task instead.";

    private const string Category = "Async";

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

        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        if (memberAccess.Name.Identifier.Text != "Result")
        {
            return;
        }

        // Parent of `x.Result` is an InvocationExpression only for `x.Result()`,
        // which isn't the Task<T>.Result property - skip method calls named Result.
        if (memberAccess.Parent is InvocationExpressionSyntax)
        {
            return;
        }

        var expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        if (IsTaskLike(expressionType))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccess.GetLocation(), ".Result"));
        }
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var methodName = memberAccess.Name.Identifier.Text;

        switch (methodName)
        {
            case "Wait":
                {
                    var expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
                    if (IsTaskLike(expressionType))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), ".Wait()"));
                    }

                    break;
                }

            case "GetResult":
                {
                    var symbol = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol;
                    if (symbol is IMethodSymbol { ContainingType: { Name: var typeName } }
                        && typeName.Contains("Awaiter"))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), "GetAwaiter().GetResult()"));
                    }

                    break;
                }

            case "Sleep":
                {
                    var symbol = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol;
                    if (symbol is IMethodSymbol { ContainingType.Name: "Thread", ContainingType.ContainingNamespace.Name: "Threading" }
                        && IsInAsyncContext(invocation))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), "Thread.Sleep"));
                    }

                    break;
                }
        }
    }

    private static bool IsTaskLike(ITypeSymbol? type)
    {
        if (type is null)
        {
            return false;
        }

        var displayName = type.OriginalDefinition.ToDisplayString();
        return displayName is "System.Threading.Tasks.Task"
            or "System.Threading.Tasks.Task<TResult>"
            or "System.Threading.Tasks.ValueTask"
            or "System.Threading.Tasks.ValueTask<TResult>";
    }

    private static bool IsInAsyncContext(SyntaxNode node)
    {
        foreach (var ancestor in node.Ancestors())
        {
            switch (ancestor)
            {
                case MethodDeclarationSyntax method:
                    return method.Modifiers.Any(SyntaxKind.AsyncKeyword);
                case LocalFunctionStatementSyntax localFunction:
                    return localFunction.Modifiers.Any(SyntaxKind.AsyncKeyword);
                case AnonymousFunctionExpressionSyntax anonymousFunction:
                    return anonymousFunction.Modifiers.Any(SyntaxKind.AsyncKeyword);
            }
        }

        return false;
    }
}
