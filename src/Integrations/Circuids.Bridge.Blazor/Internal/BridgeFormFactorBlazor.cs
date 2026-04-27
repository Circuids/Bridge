using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Circuids.Bridge.Blazor.Internal;

internal sealed class BridgeFormFactorBlazor : IBridgeFormFactor, IAsyncDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private readonly DotNetObjectReference<BridgeFormFactorBlazor> _dotNetRef;

    private const string ModulePath = "./_content/Circuids.Bridge.Blazor/BridgeFormFactor.js";

    private bool _isInitialized;
    private ResizeMode _resizeMode = ResizeMode.None;
    private CancellationTokenSource _cts = new();
    private int _listenerCount;

    public FormFactorInfo FormFactor { get; private set; } = FormFactorInfo.Unknown();

    public event EventHandler<FormFactorInfo>? FormFactorChanged;

    public BridgeFormFactorBlazor(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask());
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    public async Task InitializeAsync(ResizeMode resizeMode = ResizeMode.None)
    {
        if (_isInitialized) return;

        _resizeMode = resizeMode;

        var module = await _moduleTask.Value
            ?? throw new BridgeException("Failed to import BridgeFormFactor.js");

        FormFactor = await GetFormFactorAsync(module);
        FormFactorChanged?.Invoke(this, FormFactor);

        _isInitialized = true;

        if (resizeMode is ResizeMode.Global)
            await CreateListenerAsync();
    }

    public async Task CreateListenerAsync()
    {
        if (!_isInitialized)
            throw new BridgeException("BridgeFormFactor is not initialized. Ensure BridgeProvider is in the render tree.");

        if (_resizeMode is ResizeMode.Once) return;

        CancelPendingDispose();

        if (_listenerCount > 0 || _resizeMode is ResizeMode.Once)
        {
            _listenerCount++;
            return;
        }

        var module = await _moduleTask.Value
            ?? throw new BridgeException("Failed to import BridgeFormFactor.js");

        await module.InvokeVoidAsync("initialize", _dotNetRef);
        _listenerCount++;
    }

    [JSInvokable]
    public ValueTask NotifyFormFactorChanged(string formFactorJson)
    {
        if (!_isInitialized)
            throw new BridgeException("BridgeFormFactor is not initialized.");

        var info = JsonSerializer.Deserialize<FormFactorInfo>(formFactorJson, _jsonOptions);

        if (info is not null && FormFactor != info)
        {
            FormFactor = info;
            FormFactorChanged?.Invoke(this, info);
        }
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeListenerAsync()
    {
        try
        {
            if (_resizeMode is not ResizeMode.None) return;

            if (_listenerCount is 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);

                if (_moduleTask.IsValueCreated)
                {
                    var module = await _moduleTask.Value;
                    await module.InvokeVoidAsync("dispose");
                }

                _listenerCount = 0;
            }
            else if (_listenerCount > 0)
            {
                _listenerCount--;
            }
        }
        catch (TaskCanceledException)
        {
            _listenerCount--;
        }
        catch (JSDisconnectedException)
        {
            _listenerCount = 0;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;

                if (_listenerCount > 0 && _resizeMode is not ResizeMode.Once)
                {
                    await module.InvokeVoidAsync("dispose");
                }

                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
        }
        finally
        {
            _dotNetRef.Dispose();
            _cts.Dispose();
            _listenerCount = 0;
            _isInitialized = false;
        }
    }

    private static async ValueTask<FormFactorInfo> GetFormFactorAsync(IJSObjectReference module)
    {
        var json = await module.InvokeAsync<string>("getFormFactor");

        if (string.IsNullOrEmpty(json))
            return FormFactorInfo.Unknown();

        return JsonSerializer.Deserialize<FormFactorInfo>(json, _jsonOptions) ?? FormFactorInfo.Unknown();
    }

    private void CancelPendingDispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new();
    }
}
