using Gcode.Analyzers.Rules.DependencyInjection;
using Xunit;
using Verify = Gcode.Analyzers.Tests.CSharpAnalyzerVerifier<Gcode.Analyzers.Rules.DependencyInjection.CircularDependencyAnalyzer>;

namespace Gcode.Analyzers.Tests;

public class CircularDependencyAnalyzerTests
{
    [Fact]
    public async Task DirectCycleBetweenConcreteClasses_IsFlagged()
    {
        const string source = """
            {|#0:class ServiceA
            {
                public ServiceA(ServiceB b) { }
            }|}

            class ServiceB
            {
                public ServiceB(ServiceA a) { }
            }
            """;

        var expected = Verify.Diagnostic(CircularDependencyAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("ServiceA -> ServiceB -> ServiceA");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task CycleThroughSingleImplementorInterface_IsFlagged()
    {
        const string source = """
            interface IServiceB { }

            {|#0:class ServiceA
            {
                public ServiceA(IServiceB b) { }
            }|}

            class ServiceB : IServiceB
            {
                public ServiceB(ServiceA a) { }
            }
            """;

        var expected = Verify.Diagnostic(CircularDependencyAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("ServiceA -> ServiceB -> ServiceA");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task LinearDependencyChain_IsNotFlagged()
    {
        const string source = """
            class ServiceC { }

            class ServiceB
            {
                public ServiceB(ServiceC c) { }
            }

            class ServiceA
            {
                public ServiceA(ServiceB b) { }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AmbiguousInterfaceWithMultipleImplementors_IsNotFlagged()
    {
        const string source = """
            interface IServiceB { }

            class ServiceA
            {
                public ServiceA(IServiceB b) { }
            }

            class ServiceB : IServiceB
            {
                public ServiceB(ServiceA a) { }
            }

            class AlternateServiceB : IServiceB
            {
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }
}
