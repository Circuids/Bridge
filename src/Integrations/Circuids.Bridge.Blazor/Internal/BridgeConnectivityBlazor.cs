using Microsoft.JSInterop;

namespace Circuids.Bridge.Blazor.Internal;

internal sealed class BridgeConnectivityBlazor : IBridgeConnectivity, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private readonly DotNetObjectReference<BridgeConnectivityBlazor> _dotNetRef;

    private const string ModulePath = "./_content/Circuids.Bridge.Blazor/BridgeConnectivity.js";

    private bool _isInitialized;

    public bool IsConnected { get; private set; } = true;

    public event EventHandler<bool>? ConnectionChanged;

    public BridgeConnectivityBlazor(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask());
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    public async Task InitializeAsync(ConnectivityOptions? options = null)
    {
        if (_isInitialized) return;

        options ??= new ConnectivityOptions();

        var module = await _moduleTask.Value
            ?? throw new BridgeException("Failed to import BridgeConnectivity.js");

        IsConnected = await module.InvokeAsync<bool>("getNetworkStatus", options.TestUrl);
        ConnectionChanged?.Invoke(this, IsConnected);

        await module.InvokeVoidAsync("initializeListener", _dotNetRef, options.IntervalInSeconds, options.TestUrl);

        _isInitialized = true;
    }

    [JSInvokable]
    public void NotifyConnectivityStatusChanged(bool onlineStatus)
    {
        if (IsConnected != onlineStatus)
        {
            IsConnected = onlineStatus;
            ConnectionChanged?.Invoke(this, onlineStatus);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isInitialized || !_moduleTask.IsValueCreated) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("disposeListener");
        await module.DisposeAsync();

        _dotNetRef.Dispose();
        _isInitialized = false;
    }
}
