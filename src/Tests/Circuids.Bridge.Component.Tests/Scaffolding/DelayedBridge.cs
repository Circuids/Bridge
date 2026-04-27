namespace Circuids.Bridge.Component.Tests.Scaffolding;

internal sealed class DelayedBridge : IBridge
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Host Host => Host.Blazor;

    public PlatformIdentity Platform { get; private set; } = PlatformIdentity.Windows;

    public string PlatformVersion => "10.0.22000";

    public bool IsInitialized { get; private set; }

    public event EventHandler<PlatformIdentity>? PlatformChanged;

    public async Task InitializeAsync()
    {
        await _completion.Task;
        IsInitialized = true;
        PlatformChanged?.Invoke(this, Platform);
    }

    public void CompleteInitialization() => _completion.TrySetResult();
}
