namespace Circuids.Bridge.ComponentTests.Fakes;

internal sealed class FakeBridge : IBridge
{
    public Host Host { get; set; } = Host.Blazor;
    public PlatformIdentity Platform { get; set; } = PlatformIdentity.Windows;
    public string PlatformVersion { get; set; } = "10.0.22000";
    public bool IsInitialized { get; private set; }
    public int InitializeCallCount { get; private set; }

    public event EventHandler<PlatformIdentity>? PlatformChanged;

    public Task InitializeAsync()
    {
        InitializeCallCount++;
        IsInitialized = true;
        PlatformChanged?.Invoke(this, Platform);
        return Task.CompletedTask;
    }

    public void RaisePlatformChanged(PlatformIdentity platform)
    {
        Platform = platform;
        PlatformChanged?.Invoke(this, platform);
    }
}
