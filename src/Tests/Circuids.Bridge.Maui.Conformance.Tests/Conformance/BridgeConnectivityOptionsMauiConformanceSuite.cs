namespace Circuids.Bridge.Maui.Conformance.Tests.Conformance;

public sealed class BridgeConnectivityOptionsMauiConformanceSuite
{
    [PulseCase]
    public Task ConnectivityOptions_default_values_are_stable()
    {
        var options = new ConnectivityOptions();

        PulseAssert.Equal(10, options.IntervalInSeconds);
        PulseAssert.Equal("/favicon.ico", options.TestUrl);

        return Task.CompletedTask;
    }

    [PulseCase]
    public Task ConnectivityOptions_can_be_customized()
    {
        var options = new ConnectivityOptions
        {
            IntervalInSeconds = 2,
            TestUrl = "/health"
        };

        PulseAssert.Equal(2, options.IntervalInSeconds);
        PulseAssert.Equal("/health", options.TestUrl);

        return Task.CompletedTask;
    }
}
