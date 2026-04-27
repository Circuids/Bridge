namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class BlazorOnlyActionHostHandler : BridgeHostHandler
{
    public BlazorOnlyActionHostHandler(IBridge bridge) : base(bridge)
    {
    }

    public bool BlazorCalled { get; private set; }

    protected override void OnBlazor() => BlazorCalled = true;
}
