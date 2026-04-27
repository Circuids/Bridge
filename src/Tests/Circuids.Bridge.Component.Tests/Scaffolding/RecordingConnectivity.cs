namespace Circuids.Bridge.Component.Tests.Scaffolding;

internal sealed class RecordingConnectivity : IBridgeConnectivity
{
    private readonly InitializationRecorder _recorder;

    public RecordingConnectivity(InitializationRecorder recorder)
    {
        _recorder = recorder;
    }

    public bool IsConnected { get; private set; } = true;

    public ConnectivityOptions? LastOptions { get; private set; }

    public int InitializeCallCount { get; private set; }

    public event EventHandler<bool>? ConnectionChanged;

    public Task InitializeAsync(ConnectivityOptions? options = null)
    {
        InitializeCallCount++;
        _recorder.Calls.Add("Connectivity");
        LastOptions = options;
        ConnectionChanged?.Invoke(this, IsConnected);
        return Task.CompletedTask;
    }
}
