namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class BlazorOnlyReturningAsyncHostHandler : BridgeHostHandlerAsync<string>
{
    public BlazorOnlyReturningAsyncHostHandler(IBridge bridge) : base(bridge)
    {
    }

    protected override Task<string> OnBlazor() => Task.FromResult(nameof(Host.Blazor));
}
