using payment_backend.Interfaces;
using payment_backend.Repository;
using payment_backend.Service;

namespace payment_backend.Configuration;

public static class StartupConfiguration
{
    public static IServiceCollection ConfigureDI(this IServiceCollection service)
    {
        service.AddSingleton<IRepository, MongoRepository>();
        service.AddSingleton<ISendProcessor, SendProcessor>();

        service.AddHostedService<ProcessorBackground>();

        return service;
    }

    public static IServiceCollection ConfigureHttpClient(
        this IServiceCollection service,
        IConfiguration configuration
    )
    {
        service.AddEndpointsApiExplorer();
        service.AddHttpClient("main", client =>
        {
            client.BaseAddress = new(configuration["PROCESSOR:PAYMENT_PROCESSOR_URL"] ?? string.Empty);
            client.DefaultRequestHeaders.Accept.Add(new("application/json"));
        });

        service.AddHttpClient("fallback", client =>
        {
            client.BaseAddress = new(configuration["PROCESSOR:PAYMENT_PROCESSOR_FALLBACK_URL"] ?? string.Empty);
            client.DefaultRequestHeaders.Accept.Add(new("application/json"));
        });

        return service;
    }
}
