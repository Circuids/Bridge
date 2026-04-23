namespace Circuids.Bridge;

/// <summary>
/// Describes the current form factor and viewport dimensions.
/// </summary>
public sealed record FormFactorInfo(FormFactor FormFactor, double Width, double Height)
{
    public static FormFactorInfo Unknown() => new(FormFactor.Unknown, 0, 0);

    public static FormFactorInfo Unknown(double width, double height) => new(FormFactor.Unknown, width, height);
}
