using Gcode.Analyzers.Rules.MemorySafety;
using Xunit;
using Verify = Gcode.Analyzers.Tests.CSharpAnalyzerVerifier<Gcode.Analyzers.Rules.MemorySafety.MissingSuppressFinalizeAnalyzer>;

namespace Gcode.Analyzers.Tests;

public class MissingSuppressFinalizeAnalyzerTests
{
    [Fact]
    public async Task FinalizerWithoutSuppressFinalize_IsFlagged()
    {
        const string source = """
            class Resource
            {
                {|#0:~Resource()
                {
                    ReleaseUnmanaged();
                }|}

                public void Dispose()
                {
                    ReleaseUnmanaged();
                }

                private void ReleaseUnmanaged() { }
            }
            """;

        var expected = Verify.Diagnostic(MissingSuppressFinalizeAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("Resource");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task FinalizerWithSuppressFinalizeInDispose_IsNotFlagged()
    {
        const string source = """
            using System;

            class Resource
            {
                ~Resource()
                {
                    ReleaseUnmanaged();
                }

                public void Dispose()
                {
                    ReleaseUnmanaged();
                    GC.SuppressFinalize(this);
                }

                private void ReleaseUnmanaged() { }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoFinalizer_IsNotFlagged()
    {
        const string source = """
            using System;

            class Resource : IDisposable
            {
                public void Dispose() { }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }
}
