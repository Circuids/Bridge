namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class BlazorOnlyReturningHostHandler : BridgeHostHandler<string>
{
    public BlazorOnlyReturningHostHandler(IBridge bridge) : base(bridge)
    {
    }

    protected override string OnBlazor() => nameof(Host.Blazor);
}
