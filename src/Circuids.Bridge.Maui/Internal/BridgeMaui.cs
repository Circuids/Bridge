namespace Circuids.Bridge.Maui.Internal;

internal sealed class BridgeMaui : IBridge
{
    public Host Host => Host.Maui;
    public PlatformIdentity Platform { get; private set; } = PlatformIdentity.Unknown;
    public string PlatformVersion { get; private set; } = "Unknown";
    public bool IsInitialized { get; private set; }

    public event EventHandler<PlatformIdentity>? PlatformChanged;

    public Task InitializeAsync()
    {
        if (IsInitialized) return Task.CompletedTask;

        Platform = GetPlatform();
        PlatformVersion = DeviceInfo.Version.ToString();

        PlatformChanged?.Invoke(this, Platform);
        IsInitialized = true;

        return Task.CompletedTask;
    }

    private static PlatformIdentity GetPlatform()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return PlatformIdentity.Android;
        if (DeviceInfo.Platform == DevicePlatform.iOS)
            return PlatformIdentity.IOS;
        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return PlatformIdentity.Mac;
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return PlatformIdentity.Windows;

        return PlatformIdentity.Unknown;
    }
}
