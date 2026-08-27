using Gcode.Analyzers.Rules.MemorySafety;
using Xunit;
using Verify = Gcode.Analyzers.Tests.CSharpAnalyzerVerifier<Gcode.Analyzers.Rules.MemorySafety.UndetachedEventHandlerAnalyzer>;

namespace Gcode.Analyzers.Tests;

public class UndetachedEventHandlerAnalyzerTests
{
    private const string Publisher = """
        using System;

        class Publisher
        {
            public event EventHandler? Changed;
        }
        """;

    [Fact]
    public async Task SubscribedButNeverUnsubscribed_IsFlagged()
    {
        var source = $$"""
            {{Publisher}}

            class Subscriber : IDisposable
            {
                private readonly Publisher _publisher;

                public Subscriber(Publisher publisher)
                {
                    _publisher = publisher;
                    {|#0:_publisher.Changed += OnChanged|};
                }

                private void OnChanged(object? sender, EventArgs e) { }

                public void Dispose() { }
            }
            """;

        var expected = Verify.Diagnostic(UndetachedEventHandlerAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("Changed", "Subscriber");

        await Verify.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task SubscribedAndUnsubscribedInDispose_IsNotFlagged()
    {
        var source = $$"""
            {{Publisher}}

            class Subscriber : IDisposable
            {
                private readonly Publisher _publisher;

                public Subscriber(Publisher publisher)
                {
                    _publisher = publisher;
                    _publisher.Changed += OnChanged;
                }

                private void OnChanged(object? sender, EventArgs e) { }

                public void Dispose()
                {
                    _publisher.Changed -= OnChanged;
                }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SubscribedOnNonDisposableClass_IsNotFlagged()
    {
        var source = $$"""
            {{Publisher}}

            class Subscriber
            {
                public Subscriber(Publisher publisher)
                {
                    publisher.Changed += OnChanged;
                }

                private void OnChanged(object? sender, EventArgs e) { }
            }
            """;

        await Verify.VerifyAnalyzerAsync(source);
    }
}
