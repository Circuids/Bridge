namespace Circuids.Bridge;

/// <summary>
/// Executes host-specific async side effects (void return).
/// </summary>
public abstract class BridgeHostHandlerAsync
{
    private readonly IBridge _bridge;

    public BridgeHostHandlerAsync(IBridge bridge)
    {
        _bridge = bridge;
    }


    protected abstract Task OnBlazor();
    protected virtual Task OnMaui() => OnBlazor();
    protected virtual Task OnWpf() => OnBlazor();
    protected virtual Task OnWinForms() => OnBlazor();
    protected virtual Task OnUnknown() =>
        throw new BridgeException("Host is Unknown. Ensure Bridge is initialized via BridgeProvider.");

    public Task ExecuteAsync() => _bridge.Host switch
    {
        Host.Maui => OnMaui(),
        Host.Blazor => OnBlazor(),
        Host.Wpf => OnWpf(),
        Host.WinForms => OnWinForms(),
        _ => OnUnknown(),
    };
}

/// <summary>
/// Executes different async logic depending on the detected host environment.
/// Returns a value of type <typeparamref name="T"/>.
/// </summary>
public abstract class BridgeHostHandlerAsync<T>
{
    private readonly IBridge _bridge;

    public BridgeHostHandlerAsync(IBridge bridge)
    {
        _bridge = bridge;
    }
    protected abstract Task<T> OnBlazor();
    protected virtual Task<T> OnMaui() => OnBlazor();
    protected virtual Task<T> OnWpf() => OnBlazor();
    protected virtual Task<T> OnWinForms() => OnBlazor();
    protected virtual Task<T> OnUnknown() =>
        throw new BridgeException("Host is Unknown. Ensure Bridge is initialized via BridgeProvider.");

    public Task<T> ExecuteAsync() => _bridge.Host switch
    {
        Host.Maui => OnMaui(),
        Host.Blazor => OnBlazor(),
        Host.Wpf => OnWpf(),
        Host.WinForms => OnWinForms(),
        _ => OnUnknown(),
    };
}

