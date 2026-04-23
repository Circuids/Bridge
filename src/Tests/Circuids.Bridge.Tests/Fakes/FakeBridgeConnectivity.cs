namespace Circuids.Bridge.Tests.Fakes;

internal sealed class FakeBridgeConnectivity : IBridgeConnectivity
{
    public bool IsConnected { get; set; } = true;
    public int InitializeCallCount { get; private set; }
    public ConnectivityOptions? LastOptions { get; private set; }

    public event EventHandler<bool>? ConnectionChanged;

    public Task InitializeAsync(ConnectivityOptions? options = null)
    {
        InitializeCallCount++;
        LastOptions = options;
        ConnectionChanged?.Invoke(this, IsConnected);
        return Task.CompletedTask;
    }

    public void RaiseConnectionChanged(bool isConnected)
    {
        IsConnected = isConnected;
        ConnectionChanged?.Invoke(this, isConnected);
    }
}
