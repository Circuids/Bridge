namespace Circuids.Bridge.Maui.Internal;

internal sealed class BridgeSafeAreaMaui : IBridgeSafeArea, IDisposable
{
    private bool _isInitialized;
    private bool _isWindowSizeListenerAttached;

    public SafeAreaInsets SafeArea { get; private set; } = SafeAreaInsets.Zero;

    public event EventHandler<SafeAreaInsets>? SafeAreaChanged;

    public Task InitializeAsync()
    {
        if (_isInitialized) return Task.CompletedTask;

        SafeArea = GetSafeAreaInsets();
        _isInitialized = true;
        MainThread.BeginInvokeOnMainThread(AttachWindowSizeListener);
        SafeAreaChanged?.Invoke(this, SafeArea);

        return Task.CompletedTask;
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        var insets = GetSafeAreaInsets();

        if (SafeArea != insets)
        {
            SafeArea = insets;
            SafeAreaChanged?.Invoke(this, insets);
        }
    }

    public void Dispose()
    {
        if (_isInitialized)
        {
            MainThread.BeginInvokeOnMainThread(DetachWindowSizeListener);
            _isInitialized = false;
        }
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

    private static SafeAreaInsets GetSafeAreaInsets()
    {
#if ANDROID
        return GetAndroidSafeArea();
#elif IOS || MACCATALYST
        return GetIosSafeArea();
#else
        return SafeAreaInsets.Zero;
#endif
    }

#if ANDROID
    private static SafeAreaInsets GetAndroidSafeArea()
    {
        var activity = Platform.CurrentActivity;
        if (activity?.Window?.DecorView?.RootView is not Android.Views.View rootView)
            return SafeAreaInsets.Zero;

        var insets = AndroidX.Core.View.ViewCompat.GetRootWindowInsets(rootView);
        if (insets is null)
            return SafeAreaInsets.Zero;

        var systemBars = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars());
        if (systemBars is null)
            return SafeAreaInsets.Zero;

        var density = Android.App.Application.Context.Resources?.DisplayMetrics?.Density ?? 1f;

        return new SafeAreaInsets(
            Top: systemBars.Top / density,
            Right: systemBars.Right / density,
            Bottom: systemBars.Bottom / density,
            Left: systemBars.Left / density
        );
    }
#endif

#if IOS || MACCATALYST
    private static SafeAreaInsets GetIosSafeArea()
    {
        var scene = UIKit.UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIKit.UIWindowScene>()
            .FirstOrDefault();

        var window = scene?.Windows.FirstOrDefault(w => w.IsKeyWindow);
        if (window is null)
            return SafeAreaInsets.Zero;

        var insets = window.SafeAreaInsets;
        return new SafeAreaInsets(
            Top: insets.Top,
            Right: insets.Right,
            Bottom: insets.Bottom,
            Left: insets.Left
        );
    }
#endif
}
