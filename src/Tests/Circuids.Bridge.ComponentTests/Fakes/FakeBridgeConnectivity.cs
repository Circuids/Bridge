namespace Circuids.Bridge.ComponentTests.Fakes;

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
        return Task.CompletedTask;
    }

    public void RaiseConnectionChanged(bool isConnected)
    {
        IsConnected = isConnected;
        ConnectionChanged?.Invoke(this, isConnected);
    }
}
