namespace Circuids.Bridge.Shared.Sample.Services;

public sealed class SampleHostHandlerRunner(IBridge bridge)
{
    public IReadOnlyList<string> RunSyncScenario()
    {
        List<string> log = [];

        new SyncSideEffectHandler(bridge, log).Execute();
        var value = new SyncValueHandler(bridge).Execute();

        log.Add($"Sync value handler returned: {value}");
        return log;
    }

    public async Task<IReadOnlyList<string>> RunAsyncScenarioAsync()
    {
        List<string> log = [];

        await new AsyncSideEffectHandler(bridge, log).ExecuteAsync();
        var value = await new AsyncValueHandler(bridge).ExecuteAsync();

        log.Add($"Async value handler returned: {value}");
        return log;
    }

    private sealed class SyncSideEffectHandler(IBridge bridge, List<string> log) : BridgeHostHandler(bridge)
    {
        protected override void OnBlazor() => log.Add("Sync branch selected the Blazor implementation.");

        protected override void OnMaui() => log.Add("Sync branch selected the MAUI implementation.");
    }

    private sealed class SyncValueHandler(IBridge bridge) : BridgeHostHandler<string>(bridge)
    {
        protected override string OnBlazor() => "Blazor sync result";

        protected override string OnMaui() => "MAUI sync result";
    }

    private sealed class AsyncSideEffectHandler(IBridge bridge, List<string> log) : BridgeHostHandlerAsync(bridge)
    {
        protected override async Task OnBlazor()
        {
            log.Add("Async branch entered the Blazor implementation.");
            await Task.Yield();
            log.Add("Async Blazor branch resumed after await.");
        }

        protected override async Task OnMaui()
        {
            log.Add("Async branch entered the MAUI implementation.");
            await Task.Yield();
            log.Add("Async MAUI branch resumed after await.");
        }
    }

    private sealed class AsyncValueHandler(IBridge bridge) : BridgeHostHandlerAsync<string>(bridge)
    {
        protected override Task<string> OnBlazor() => Task.FromResult("Blazor async result");

        protected override Task<string> OnMaui() => Task.FromResult("MAUI async result");
    }
}