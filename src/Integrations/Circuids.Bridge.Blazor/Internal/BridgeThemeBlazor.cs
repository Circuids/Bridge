using Microsoft.JSInterop;

namespace Circuids.Bridge.Blazor.Internal;

internal sealed class BridgeThemeBlazor : IBridgeTheme, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private readonly DotNetObjectReference<BridgeThemeBlazor> _dotNetRef;

    private const string ModulePath = "./_content/Circuids.Bridge.Blazor/BridgeTheme.js";

    private bool _isInitialized;

    public ThemeMode Theme { get; private set; } = ThemeMode.Unknown;

    public event EventHandler<ThemeMode>? ThemeChanged;

    public BridgeThemeBlazor(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask());
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        var module = await _moduleTask.Value
            ?? throw new BridgeException("Failed to import BridgeTheme.js");

        var themeStr = await module.InvokeAsync<string>("getTheme");
        if (Enum.TryParse<ThemeMode>(themeStr, true, out var mode))
            Theme = mode;

        await module.InvokeVoidAsync("initializeListener", _dotNetRef);

        ThemeChanged?.Invoke(this, Theme);
        _isInitialized = true;
    }

    [JSInvokable]
    public void NotifyThemeChanged(string themeStr)
    {
        if (Enum.TryParse<ThemeMode>(themeStr, true, out var mode) && Theme != mode)
        {
            Theme = mode;
            ThemeChanged?.Invoke(this, mode);
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
}
