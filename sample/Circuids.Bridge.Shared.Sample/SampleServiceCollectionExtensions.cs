using Circuids.Bridge.Shared.Sample.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Shared.Sample;

public static class SampleServiceCollectionExtensions
{
    public static IServiceCollection AddBridgeSharedSample(this IServiceCollection services)
    {
        services.AddScoped<SampleHostHandlerRunner>();
        services.AddSingleton<SampleScenarioRegistry>();
        services.AddSingleton<SampleDiagnosticsFormatter>();
        return services;
    }
}