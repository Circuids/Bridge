namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class FallbackHostHandler : BridgeHostHandler<string>
{
    public FallbackHostHandler(IBridge bridge) : base(bridge)
    {
    }

    protected override string OnBlazor() => "BlazorFallback";
}
