using Circuids.Bridge.TestSupport.Handlers;
using Circuids.Pulse;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Blazor.Conformance.Tests.Conformance;

public sealed class BridgeHostHandlersBlazorConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeHostHandlersBlazorConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task BridgeHostHandler_dispatches_Blazor_branch_for_runtime_host()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var handler = new RecordingHostHandler(bridge);

        handler.Execute();

        PulseAssert.Equal(nameof(Host.Blazor), handler.Branch);
    }

    [PulseCase]
    public async Task BridgeHostHandler_generic_returns_Blazor_branch_for_runtime_host()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var handler = new ReturningHostHandler(bridge);

        var branch = handler.Execute();

        PulseAssert.Equal(nameof(Host.Blazor), branch);
    }

    [PulseCase]
    public async Task BridgeHostHandlerAsync_dispatches_Blazor_branch_for_runtime_host()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var handler = new RecordingAsyncHostHandler(bridge);

        await handler.ExecuteAsync();

        PulseAssert.Equal(nameof(Host.Blazor), handler.Branch);
    }

    [PulseCase]
    public async Task BridgeHostHandlerAsync_generic_returns_Blazor_branch_for_runtime_host()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var handler = new ReturningAsyncHostHandler(bridge);

        var branch = await handler.ExecuteAsync();

        PulseAssert.Equal(nameof(Host.Blazor), branch);
    }

    [PulseCase]
    public Task BridgeHostHandler_uses_Blazor_fallback_for_Wpf_and_WinForms()
    {
        var wpfHandler = new FallbackHostHandler(new StaticBridge(Host.Wpf));
        var winFormsHandler = new FallbackHostHandler(new StaticBridge(Host.WinForms));

        PulseAssert.Equal("BlazorFallback", wpfHandler.Execute());
        PulseAssert.Equal("BlazorFallback", winFormsHandler.Execute());

        return Task.CompletedTask;
    }

    [PulseCase]
    public Task BridgeHostHandler_unknown_host_throws_BridgeException()
    {
        var handler = new FallbackHostHandler(new StaticBridge(Host.Unknown));
        var threw = false;

        try
        {
            _ = handler.Execute();
        }
        catch (BridgeException exception)
        {
            threw = true;
            PulseAssert.False(string.IsNullOrWhiteSpace(exception.Message));
        }

        PulseAssert.True(threw, "Unknown host should throw BridgeException.");
        return Task.CompletedTask;
    }
}
