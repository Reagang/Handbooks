using Gcode.Analyzers.Rules.DependencyInjection;
using Xunit;
using Verify = Gcode.Analyzers.Tests.CSharpAnalyzerVerifier<Gcode.Analyzers.Rules.DependencyInjection.DuplicateServiceRegistrationAnalyzer>;

namespace Gcode.Analyzers.Tests;

public class DuplicateServiceRegistrationAnalyzerTests
{
    private const string ServiceCollectionShim = """
        interface IServiceCollection { }

        static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services) => services;
            public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services) => services;
            public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services) => services;
        }

        interface IMyService { }
        class MyService : IMyService { }

        interface IOtherService { }
        class OtherService : IOtherService { }
        """;

    [Fact]
    public async Task SameServiceRegisteredTwice_IsFlagged()
    {
        var source = $$"""
            {{ServiceCollectionShim}}

            class Startup
            {
                void ConfigureServices(IServiceCollection services)
                {
                    services.AddSingleton<IMyService, MyService>();
                    {|#0:services.AddSingleton<IMyService, MyService>()|};
                }
            }
            """;

        var expected = Verify.Diagnostic(DuplicateServiceRegistrationAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("IMyService");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task DifferentServicesRegistered_IsNotFlagged()
    {
        var source = $$"""
            {{ServiceCollectionShim}}

            class Startup
            {
                void ConfigureServices(IServiceCollection services)
                {
                    services.AddSingleton<IMyService, MyService>();
                    services.AddScoped<IOtherService, OtherService>();
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }
}
