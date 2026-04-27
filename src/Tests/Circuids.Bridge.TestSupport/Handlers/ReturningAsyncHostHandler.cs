namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class ReturningAsyncHostHandler : BridgeHostHandlerAsync<string>
{
    public ReturningAsyncHostHandler(IBridge bridge) : base(bridge)
    {
    }

    protected override Task<string> OnBlazor() => Task.FromResult(nameof(Host.Blazor));

    protected override Task<string> OnMaui() => Task.FromResult(nameof(Host.Maui));

    protected override Task<string> OnWpf() => Task.FromResult(nameof(Host.Wpf));

    protected override Task<string> OnWinForms() => Task.FromResult(nameof(Host.WinForms));
}
