namespace Circuids.Bridge.Component.Tests.Scaffolding;

internal sealed class RecordingBridge : IBridge
{
    private readonly InitializationRecorder _recorder;

    public RecordingBridge(InitializationRecorder recorder)
    {
        _recorder = recorder;
    }

    public Host Host => Host.Blazor;

    public PlatformIdentity Platform { get; private set; } = PlatformIdentity.Windows;

    public string PlatformVersion => "10.0.22000";

    public bool IsInitialized { get; private set; }

    public int InitializeCallCount { get; private set; }

    public event EventHandler<PlatformIdentity>? PlatformChanged;

    public Task InitializeAsync()
    {
        InitializeCallCount++;
        _recorder.Calls.Add("Bridge");
        IsInitialized = true;
        PlatformChanged?.Invoke(this, Platform);
        return Task.CompletedTask;
    }
}
