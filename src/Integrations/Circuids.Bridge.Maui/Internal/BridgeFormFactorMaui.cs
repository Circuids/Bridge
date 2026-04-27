namespace Circuids.Bridge.Maui.Internal;

internal sealed class BridgeFormFactorMaui : IBridgeFormFactor, IDisposable
{
    private bool _isInitialized;
    private bool _isDisposed;
    private bool _isWindowSizeListenerAttached;
    private int _listenerCount;
    private CancellationTokenSource _cts = new();
    private ResizeMode _resizeMode = ResizeMode.None;

    public FormFactorInfo FormFactor { get; private set; } = FormFactorInfo.Unknown();

    public event EventHandler<FormFactorInfo>? FormFactorChanged;

    public async Task InitializeAsync(ResizeMode resizeMode = ResizeMode.None)
    {
        if (_isInitialized) return;

        _resizeMode = resizeMode;
        FormFactor = GetFormFactor();
        _isInitialized = true;
        FormFactorChanged?.Invoke(this, FormFactor);

        if (resizeMode is ResizeMode.Global)
            await CreateListenerAsync();
    }

    public Task CreateListenerAsync()
    {
        if (!_isInitialized)
            throw new BridgeException("BridgeFormFactor is not initialized. Ensure BridgeProvider is in the render tree.");

        if (_resizeMode is ResizeMode.Once) return Task.CompletedTask;

        CancelPendingDispose();

        if (_listenerCount > 0 || _resizeMode is ResizeMode.Once)
        {
            _listenerCount++;
            return Task.CompletedTask;
        }

        MainThread.BeginInvokeOnMainThread(AttachWindowSizeListener);

        _listenerCount++;
        return Task.CompletedTask;
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        var newInfo = GetFormFactor();

        if (newInfo != FormFactor)
        {
            FormFactor = newInfo;
            FormFactorChanged?.Invoke(this, newInfo);
        }
    }

    public async ValueTask DisposeListenerAsync()
    {
        try
        {
            if (_resizeMode is not ResizeMode.None) return;

            if (_listenerCount is 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);

                MainThread.BeginInvokeOnMainThread(DetachWindowSizeListener);

                _listenerCount = 0;
            }
            else if (_listenerCount > 0)
            {
                _listenerCount--;
            }
        }
        catch (TaskCanceledException)
        {
            if (_listenerCount > 0)
                _listenerCount--;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _cts.Cancel();
        _cts.Dispose();

        if (_listenerCount > 0 && _resizeMode is not ResizeMode.Once)
            MainThread.BeginInvokeOnMainThread(DetachWindowSizeListener);

        _listenerCount = 0;
        _isInitialized = false;
        _isDisposed = true;
    }

    private void CancelPendingDispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new();
    }

    private static FormFactorInfo GetFormFactor()
    {
        if (Application.Current is null || Application.Current.Windows.Count is 0)
            return FormFactorInfo.Unknown();

        var width = Application.Current.Windows[0].Width;
        var height = Application.Current.Windows[0].Height;

        return CreateFormFactorInfo(width, height);
    }

    private static FormFactorInfo CreateFormFactorInfo(double width, double height)
    {
        if (width <= 767)
            return new FormFactorInfo(Bridge.FormFactor.Phone, width, height);
        if (width <= 1023)
            return new FormFactorInfo(Bridge.FormFactor.Tablet, width, height);

        return new FormFactorInfo(Bridge.FormFactor.Desktop, width, height);
    }

    private void AttachWindowSizeListener()
    {
        if (_isWindowSizeListenerAttached) return;
        if (Application.Current is null || Application.Current.Windows.Count is 0) return;

        Application.Current.Windows[0].SizeChanged += OnWindowSizeChanged;
        _isWindowSizeListenerAttached = true;
    }

    private void DetachWindowSizeListener()
    {
        if (!_isWindowSizeListenerAttached) return;

        if (Application.Current is not null && Application.Current.Windows.Count > 0)
            Application.Current.Windows[0].SizeChanged -= OnWindowSizeChanged;

        _isWindowSizeListenerAttached = false;
    }
}
