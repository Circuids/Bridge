namespace Circuids.Bridge;

/// <summary>
/// Executes host-specific side effects (void return).
/// </summary>
public abstract class BridgeHostHandler
{
    private readonly IBridge _bridge;

    public BridgeHostHandler(IBridge bridge)
    {
        _bridge = bridge;
    }
    protected abstract void OnBlazor();
    protected virtual void OnMaui() => OnBlazor();
    protected virtual void OnWpf() => OnBlazor();
    protected virtual void OnWinForms() => OnBlazor();
    protected virtual void OnUnknown() =>
        throw new BridgeException("Host is Unknown. Ensure Bridge is initialized via BridgeProvider.");

    public void Execute()
    {
        switch (_bridge.Host)
        {
            case Host.Maui: OnMaui(); break;
            case Host.Blazor: OnBlazor(); break;
            case Host.Wpf: OnWpf(); break;
            case Host.WinForms: OnWinForms(); break;
            default: OnUnknown(); break;
        }
    }
}

/// <summary>
/// Executes different logic depending on the detected host environment.
/// Returns a value of type <typeparamref name="T"/>.
/// </summary>
public abstract class BridgeHostHandler<T>
{
    private readonly IBridge _bridge;

    public BridgeHostHandler(IBridge bridge)
    {
        _bridge = bridge;
    }
    protected abstract T OnBlazor();
    protected virtual T OnMaui() => OnBlazor();
    protected virtual T OnWpf() => OnBlazor();
    protected virtual T OnWinForms() => OnBlazor();
    protected virtual T OnUnknown() =>
        throw new BridgeException("Host is Unknown. Ensure Bridge is initialized via BridgeProvider.");

    public T Execute() => _bridge.Host switch
    {
        Host.Maui => OnMaui(),
        Host.Blazor => OnBlazor(),
        Host.Wpf => OnWpf(),
        Host.WinForms => OnWinForms(),
        _ => OnUnknown(),
    };
}

