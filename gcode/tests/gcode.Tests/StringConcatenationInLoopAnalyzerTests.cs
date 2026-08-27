using Gcode.Analyzers.Rules.Organization;
using Xunit;
using Verify = Gcode.Analyzers.Tests.CSharpAnalyzerVerifier<Gcode.Analyzers.Rules.Organization.StringConcatenationInLoopAnalyzer>;

namespace Gcode.Analyzers.Tests;

public class StringConcatenationInLoopAnalyzerTests
{
    [Fact]
    public async Task StringConcatenationInForeach_IsFlagged()
    {
        const string source = """
            using System.Collections.Generic;

            class C
            {
                string M(List<string> items)
                {
                    var result = "";
                    foreach (var item in items)
                    {
                        {|#0:result += item|};
                    }

                    return result;
                }
            }
            """;

        var expected = Verify.Diagnostic(StringConcatenationInLoopAnalyzer.DiagnosticId).WithLocation(0);

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task StringConcatenationOutsideLoop_IsNotFlagged()
    {
        const string source = """
            class C
            {
                string M(string a, string b)
                {
                    var result = a;
                    result += b;
                    return result;
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task StringConcatenationInLambdaInsideLoop_IsNotFlagged()
    {
        const string source = """
            using System;
            using System.Collections.Generic;

            class C
            {
                void M(List<string> items)
                {
                    foreach (var item in items)
                    {
                        Action a = () =>
                        {
                            var local = "";
                            local += item;
                        };
                        a();
                    }
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }
}
