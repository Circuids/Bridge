namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class RecordingAsyncHostHandler : BridgeHostHandlerAsync
{
    public RecordingAsyncHostHandler(IBridge bridge) : base(bridge)
    {
    }

    public string Branch { get; private set; } = string.Empty;

    protected override Task OnBlazor()
    {
        Branch = nameof(Host.Blazor);
        return Task.CompletedTask;
    }

    protected override Task OnMaui()
    {
        Branch = nameof(Host.Maui);
        return Task.CompletedTask;
    }

    protected override Task OnWpf()
    {
        Branch = nameof(Host.Wpf);
        return Task.CompletedTask;
    }

    protected override Task OnWinForms()
    {
        Branch = nameof(Host.WinForms);
        return Task.CompletedTask;
    }
}
