using Gcode.Analyzers.Rules.CleanCode;
using Xunit;
using Verify = Gcode.Analyzers.Tests.CSharpAnalyzerVerifier<Gcode.Analyzers.Rules.CleanCode.MagicConstantsAnalyzer>;

namespace Gcode.Analyzers.Tests;

public class MagicConstantsAnalyzerTests
{
    [Fact]
    public async Task MagicNumberInComparison_IsFlagged()
    {
        const string source = """
            class C
            {
                bool M(int status) => status == {|#0:5|};
            }
            """;

        var expected = Verify.Diagnostic(MagicConstantsAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("5");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task MagicStringInSwitchCase_IsFlagged()
    {
        const string source = """
            class C
            {
                bool M(string status)
                {
                    switch (status)
                    {
                        case {|#0:"completed"|}:
                            return true;
                        default:
                            return false;
                    }
                }
            }
            """;

        var expected = Verify.Diagnostic(MagicConstantsAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("completed");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task NamedConstant_IsNotFlagged()
    {
        const string source = """
            class C
            {
                private const int CompletedStatus = 5;

                bool M(int status) => status == CompletedStatus;
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("status == 0")]
    [InlineData("status == 1")]
    [InlineData("status == -1")]
    public async Task AllowlistedSentinelValues_AreNotFlagged(string comparison)
    {
        var source = $$"""
            class C
            {
                bool M(int status) => {{comparison}};
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }
}
