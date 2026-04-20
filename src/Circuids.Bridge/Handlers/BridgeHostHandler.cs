namespace Circuids.Bridge;

/// <summary>
/// Executes host-specific side effects (void return).
/// </summary>
public abstract class BridgeHostHandler(IBridge bridge)
{
    protected abstract void OnMaui();
    protected abstract void OnBlazor();
    protected virtual void OnWpf() => OnBlazor();
    protected virtual void OnWinForms() => OnBlazor();
    protected virtual void OnUnknown() =>
        throw new BridgeException("Host is Unknown. Ensure Bridge is initialized via BridgeProvider.");

    public void Execute()
    {
        switch (bridge.Host)
        {
            case Host.Maui: OnMaui(); break;
            case Host.Blazor: OnBlazor(); break;
            case Host.Wpf: OnWpf(); break;
            case Host.WinForms: OnWinForms(); break;
            default: OnUnknown(); break;
        }
    }
}
