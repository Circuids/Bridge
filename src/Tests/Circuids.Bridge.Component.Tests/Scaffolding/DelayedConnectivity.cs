namespace Circuids.Bridge.Component.Tests.Scaffolding;

internal sealed class DelayedConnectivity : IBridgeConnectivity
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsConnected { get; private set; } = true;

    public event EventHandler<bool>? ConnectionChanged;

    public async Task InitializeAsync(ConnectivityOptions? options = null)
    {
        await _completion.Task;
        ConnectionChanged?.Invoke(this, IsConnected);
    }

    public void CompleteInitialization() => _completion.TrySetResult();
}
