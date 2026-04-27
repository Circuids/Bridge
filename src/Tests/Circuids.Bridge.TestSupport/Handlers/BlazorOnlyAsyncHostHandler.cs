namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class BlazorOnlyAsyncHostHandler : BridgeHostHandlerAsync
{
    public BlazorOnlyAsyncHostHandler(IBridge bridge) : base(bridge)
    {
    }

    public bool BlazorCalled { get; private set; }

    protected override Task OnBlazor()
    {
        BlazorCalled = true;
        return Task.CompletedTask;
    }
}
