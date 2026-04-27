namespace Circuids.Bridge.Maui.Internal;

internal sealed class BridgeConnectivityMaui : IBridgeConnectivity, IDisposable
{
    private bool _isInitialized;

    public bool IsConnected { get; private set; }

    public event EventHandler<bool>? ConnectionChanged;

    public Task InitializeAsync(ConnectivityOptions? options = null)
    {
        if (_isInitialized) return Task.CompletedTask;

        IsConnected = Connectivity.NetworkAccess == NetworkAccess.Internet;
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
        _isInitialized = true;
        ConnectionChanged?.Invoke(this, IsConnected);

        return Task.CompletedTask;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var connected = e.NetworkAccess == NetworkAccess.Internet;
        if (IsConnected != connected)
        {
            IsConnected = connected;
            ConnectionChanged?.Invoke(this, connected);
        }
    }

    public void Dispose()
    {
        if (_isInitialized)
        {
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
            _isInitialized = false;
        }
    }
}
