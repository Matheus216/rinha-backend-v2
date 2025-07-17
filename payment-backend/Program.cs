using payment_backend.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureDI()
    .ConfigureHttpClient(builder.Configuration);

builder.Build()
    .ConfigureApp()
    .Run();
