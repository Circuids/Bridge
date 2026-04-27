namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class RecordingHostHandler : BridgeHostHandler
{
    public RecordingHostHandler(IBridge bridge) : base(bridge)
    {
    }

    public string Branch { get; private set; } = string.Empty;

    protected override void OnBlazor() => Branch = nameof(Host.Blazor);

    protected override void OnMaui() => Branch = nameof(Host.Maui);

    protected override void OnWpf() => Branch = nameof(Host.Wpf);

    protected override void OnWinForms() => Branch = nameof(Host.WinForms);
}
