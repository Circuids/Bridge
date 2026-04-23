using Microsoft.JSInterop;

namespace Circuids.Bridge.Blazor.Internal;

internal sealed class BridgeBlazor : IBridge, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    private const string ModulePath = "./_content/Circuids.Bridge.Blazor/Bridge.js";

    public Host Host => Host.Blazor;
    public PlatformIdentity Platform { get; private set; } = PlatformIdentity.Unknown;
    public string PlatformVersion { get; private set; } = "Unknown";
    public bool IsInitialized { get; private set; }

    public event EventHandler<PlatformIdentity>? PlatformChanged;

    public BridgeBlazor(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask());
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        var module = await _moduleTask.Value
            ?? throw new BridgeException("Failed to import Bridge.js");

        var platformStr = await module.InvokeAsync<string>("getPlatform");
        if (Enum.TryParse<PlatformIdentity>(platformStr, out var identity))
            Platform = identity;

        var versionStr = await module.InvokeAsync<string>("getPlatformVersion");
        if (!string.IsNullOrEmpty(versionStr))
            PlatformVersion = versionStr;

        PlatformChanged?.Invoke(this, Platform);
        IsInitialized = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
