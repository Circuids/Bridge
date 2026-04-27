namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class ReturningHostHandler : BridgeHostHandler<string>
{
    public ReturningHostHandler(IBridge bridge) : base(bridge)
    {
    }

    protected override string OnBlazor() => nameof(Host.Blazor);

    protected override string OnMaui() => nameof(Host.Maui);

    protected override string OnWpf() => nameof(Host.Wpf);

    protected override string OnWinForms() => nameof(Host.WinForms);
}
