namespace Circuids.Bridge.Maui.Internal;

internal sealed class BridgeFormFactorMaui : IBridgeFormFactor
{
    private bool _isInitialized;
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

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current is null || Application.Current.Windows.Count is 0) return;
            Application.Current.Windows[0].SizeChanged += OnWindowSizeChanged;
        });

        _listenerCount++;
        return Task.CompletedTask;
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        if (Application.Current is null || Application.Current.Windows.Count is 0) return;

        var width = Application.Current.Windows[0].Width;
        var height = Application.Current.Windows[0].Height;

        FormFactorInfo newInfo;

        if (width <= 767)
            newInfo = new FormFactorInfo(Bridge.FormFactor.Phone, width, height);
        else if (width <= 1023)
            newInfo = new FormFactorInfo(Bridge.FormFactor.Tablet, width, height);
        else
            newInfo = new FormFactorInfo(Bridge.FormFactor.Desktop, width, height);

        if (newInfo.FormFactor != FormFactor.FormFactor)
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

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (Application.Current is null || Application.Current.Windows.Count is 0) return;
                    Application.Current.Windows[0].SizeChanged -= OnWindowSizeChanged;
                });

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
    }

    private void CancelPendingDispose()
    {
        _cts.Cancel();
        _cts = new();
    }

    private static FormFactorInfo GetFormFactor()
    {
        if (Application.Current is null || Application.Current.Windows.Count is 0)
            return FormFactorInfo.Unknown();

        var width = Application.Current.Windows[0].Width;
        var height = Application.Current.Windows[0].Height;

        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            return new FormFactorInfo(Bridge.FormFactor.Phone, width, height);
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            return new FormFactorInfo(Bridge.FormFactor.Tablet, width, height);
        if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
            return new FormFactorInfo(Bridge.FormFactor.Desktop, width, height);

        return FormFactorInfo.Unknown();
    }
}
