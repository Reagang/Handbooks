using Gcode.Analyzers.Rules.CleanCode;
using Xunit;
using Verify = Gcode.Analyzers.Tests.CSharpAnalyzerVerifier<Gcode.Analyzers.Rules.CleanCode.BlockingAsyncAnalyzer>;

namespace Gcode.Analyzers.Tests;

public class BlockingAsyncAnalyzerTests
{
    [Fact]
    public async Task DotResult_IsFlagged()
    {
        const string source = """
            using System.Threading.Tasks;

            class C
            {
                async Task M(Task<int> task)
                {
                    var value = {|#0:task.Result|};
                    await Task.Yield();
                }
            }
            """;

        var expected = Verify.Diagnostic(BlockingAsyncAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments(".Result");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task DotWait_IsFlagged()
    {
        const string source = """
            using System.Threading.Tasks;

            class C
            {
                void M(Task task)
                {
                    {|#0:task.Wait()|};
                }
            }
            """;

        var expected = Verify.Diagnostic(BlockingAsyncAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments(".Wait()");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task GetAwaiterGetResult_IsFlagged()
    {
        const string source = """
            using System.Threading.Tasks;

            class C
            {
                int M(Task<int> task)
                {
                    return {|#0:task.GetAwaiter().GetResult()|};
                }
            }
            """;

        var expected = Verify.Diagnostic(BlockingAsyncAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("GetAwaiter().GetResult()");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ThreadSleepInsideAsyncMethod_IsFlagged()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;

            class C
            {
                async Task M()
                {
                    {|#0:Thread.Sleep(1000)|};
                    await Task.Yield();
                }
            }
            """;

        var expected = Verify.Diagnostic(BlockingAsyncAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("Thread.Sleep");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ThreadSleepInsideSyncMethod_IsNotFlagged()
    {
        const string source = """
            using System.Threading;

            class C
            {
                void M()
                {
                    Thread.Sleep(1000);
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Await_IsNotFlagged()
    {
        const string source = """
            using System.Threading.Tasks;

            class C
            {
                async Task<int> M(Task<int> task)
                {
                    return await task;
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }
}
