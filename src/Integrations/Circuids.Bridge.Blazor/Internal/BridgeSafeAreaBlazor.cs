using Microsoft.JSInterop;
using System.Text.Json;

namespace Circuids.Bridge.Blazor.Internal;

internal sealed class BridgeSafeAreaBlazor : IBridgeSafeArea, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private readonly DotNetObjectReference<BridgeSafeAreaBlazor> _dotNetRef;

    private const string ModulePath = "./_content/Circuids.Bridge.Blazor/BridgeSafeArea.js";

    private bool _isInitialized;

    public SafeAreaInsets SafeArea { get; private set; } = SafeAreaInsets.Zero;

    public event EventHandler<SafeAreaInsets>? SafeAreaChanged;

    public BridgeSafeAreaBlazor(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask());
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        var module = await _moduleTask.Value
            ?? throw new BridgeException("Failed to import BridgeSafeArea.js");

        SafeArea = await GetSafeAreaAsync(module);

        await module.InvokeVoidAsync("initializeListener", _dotNetRef);

        SafeAreaChanged?.Invoke(this, SafeArea);
        _isInitialized = true;
    }

    [JSInvokable]
    public void NotifySafeAreaChanged(string json)
    {
        var insets = JsonSerializer.Deserialize<SafeAreaInsets>(json);
        if (insets is not null && SafeArea != insets)
        {
            SafeArea = insets;
            SafeAreaChanged?.Invoke(this, insets);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;

                if (_isInitialized)
                    await module.InvokeVoidAsync("disposeListener");

                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
        }
        finally
        {
            _dotNetRef.Dispose();
            _isInitialized = false;
        }
    }

    private static async ValueTask<SafeAreaInsets> GetSafeAreaAsync(IJSObjectReference module)
    {
        var json = await module.InvokeAsync<string>("getSafeAreaInsets");

        if (string.IsNullOrEmpty(json))
            return SafeAreaInsets.Zero;

        return JsonSerializer.Deserialize<SafeAreaInsets>(json) ?? SafeAreaInsets.Zero;
    }
}
