namespace Circuids.Bridge;

/// <summary>
/// Safe area insets for notched/cutout devices.
/// All values in CSS pixels (device-independent).
/// </summary>
public sealed record SafeAreaInsets(double Top, double Right, double Bottom, double Left)
{
    public static SafeAreaInsets Zero => new(0, 0, 0, 0);

    /// <summary>
    /// Whether any inset is non-zero (i.e., device has notch/cutout/nav bar).
    /// </summary>
    public bool HasInsets => Top > 0 || Right > 0 || Bottom > 0 || Left > 0;
}
