namespace Circuids.Bridge.TestSupport.Handlers;

public sealed class StaticBridge : IBridge
{
    public StaticBridge(Host host)
    {
        Host = host;
    }

    public Host Host { get; }

    public PlatformIdentity Platform => PlatformIdentity.Unknown;

    public string PlatformVersion => "Test";

    public bool IsInitialized => true;

    public event EventHandler<PlatformIdentity>? PlatformChanged;

    public Task InitializeAsync()
    {
        PlatformChanged?.Invoke(this, Platform);
        return Task.CompletedTask;
    }
}
