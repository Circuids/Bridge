namespace Circuids.Bridge;

/// <summary>
/// Executes different logic depending on the detected host environment.
/// Returns a value of type <typeparamref name="T"/>.
/// </summary>
public abstract class BridgeHostHandler<T>(IBridge bridge)
{
    protected abstract T OnMaui();
    protected abstract T OnBlazor();
    protected virtual T OnWpf() => OnBlazor();
    protected virtual T OnWinForms() => OnBlazor();
    protected virtual T OnUnknown() =>
        throw new BridgeException("Host is Unknown. Ensure Bridge is initialized via BridgeProvider.");

    public T Execute() => bridge.Host switch
    {
        Host.Maui => OnMaui(),
        Host.Blazor => OnBlazor(),
        Host.Wpf => OnWpf(),
        Host.WinForms => OnWinForms(),
        _ => OnUnknown(),
    };
}
