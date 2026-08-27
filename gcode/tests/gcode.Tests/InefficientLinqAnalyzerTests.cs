using Gcode.Analyzers.Rules.CleanCode;
using Xunit;
using Verify = Gcode.Analyzers.Tests.CSharpAnalyzerVerifier<Gcode.Analyzers.Rules.CleanCode.InefficientLinqAnalyzer>;

namespace Gcode.Analyzers.Tests;

public class InefficientLinqAnalyzerTests
{
    [Fact]
    public async Task ToListFollowedByAnotherLinqCall_IsFlagged()
    {
        const string source = """
            using System.Collections.Generic;
            using System.Linq;

            class Order
            {
                public bool Active { get; set; }
            }

            class C
            {
                Order? M(List<Order> orders)
                {
                    return {|#0:orders.Where(o => o.Active).ToList()|}.FirstOrDefault();
                }
            }
            """;

        var expected = Verify.Diagnostic(InefficientLinqAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("ToList");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ComposedLinqQuery_IsNotFlagged()
    {
        const string source = """
            using System.Collections.Generic;
            using System.Linq;

            class Order
            {
                public bool Active { get; set; }
            }

            class C
            {
                Order? M(List<Order> orders)
                {
                    return orders.FirstOrDefault(o => o.Active);
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ToListNotFollowedByAnotherCall_IsNotFlagged()
    {
        const string source = """
            using System.Collections.Generic;
            using System.Linq;

            class Order
            {
                public bool Active { get; set; }
            }

            class C
            {
                List<Order> M(List<Order> orders)
                {
                    return orders.Where(o => o.Active).ToList();
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }
}
