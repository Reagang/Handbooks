using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gcode.Analyzers.Rules.DependencyInjection;

/// <summary>
/// DI1002: flags a second <c>AddSingleton&lt;TService&gt;</c> /
/// <c>AddScoped&lt;TService&gt;</c> / <c>AddTransient&lt;TService&gt;</c>
/// registration for a service type already registered earlier in the same
/// method. <c>TryAddSingleton</c> and friends are exempt - they are the
/// idiomatic way to register conditionally.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DuplicateServiceRegistrationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DI1002";

    private static readonly LocalizableString Title = "Duplicate service registration";

    private static readonly LocalizableString MessageFormat =
        "'{0}' is already registered earlier in this method; the earlier registration will be overridden";

    private static readonly LocalizableString Description =
        "Registering the same service type more than once creates ambiguous, " +
        "order-dependent behavior and makes the container configuration harder " +
        "to reason about. Centralize the registration, or use TryAddSingleton / " +
        "TryAddScoped / TryAddTransient if the intent is \"register if absent\".";

    private const string Category = "DependencyInjection";

    private static readonly ImmutableHashSet<string> RegistrationMethodNames =
        ImmutableHashSet.Create("AddSingleton", "AddScoped", "AddTransient");

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

        context.RegisterCodeBlockAction(AnalyzeCodeBlock);
    }

    private static void AnalyzeCodeBlock(CodeBlockAnalysisContext context)
    {
        var seen = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var invocation in context.CodeBlock.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
                !RegistrationMethodNames.Contains(memberAccess.Name.Identifier.Text))
            {
                continue;
            }

            if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            var serviceType = GetServiceType(context, invocation, methodSymbol);
            if (serviceType is null)
            {
                continue;
            }

            if (!seen.Add(serviceType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), serviceType.ToDisplayString()));
            }
        }
    }

    private static ITypeSymbol? GetServiceType(
        CodeBlockAnalysisContext context,
        InvocationExpressionSyntax invocation,
        IMethodSymbol methodSymbol)
    {
        if (methodSymbol.TypeArguments.Length > 0)
        {
            return methodSymbol.TypeArguments[0];
        }

        var firstArgument = invocation.ArgumentList.Arguments.Count > 0
            ? invocation.ArgumentList.Arguments[0].Expression
            : null;

        if (firstArgument is TypeOfExpressionSyntax typeOf)
        {
            return context.SemanticModel.GetTypeInfo(typeOf.Type, context.CancellationToken).Type;
        }

        return null;
    }
}
