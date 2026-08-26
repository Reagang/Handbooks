using Gcode.Analyzers.Rules;
using Microsoft.CodeAnalysis;
using Xunit;
using Verify = Gcode.Analyzers.Tests.CSharpAnalyzerVerifier<Gcode.Analyzers.Rules.EmptyCatchBlockAnalyzer>;

namespace Gcode.Analyzers.Tests;

public class EmptyCatchBlockAnalyzerTests
{
    [Fact]
    public async Task EmptyCatchBlock_IsFlagged()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try
                    {
                        throw new InvalidOperationException();
                    }
                    {|#0:catch (Exception)
                    {
                    }|}
                }
            }
            """;

        var expected = Verify.Diagnostic(EmptyCatchBlockAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithSeverity(DiagnosticSeverity.Warning);

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task CatchBlockThatHandlesTheException_IsNotFlagged()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try
                    {
                        throw new InvalidOperationException();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task EmptyCatchBlockWithExplanatoryComment_IsNotFlagged()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try
                    {
                        throw new InvalidOperationException();
                    }
                    catch (Exception)
                    {
                        // Intentionally ignored: best-effort cleanup.
                    }
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }
}
