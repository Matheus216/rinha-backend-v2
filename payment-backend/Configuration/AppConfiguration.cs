using Microsoft.AspNetCore.Mvc;
using payment_backend.Service;
using payment_backend.models;

namespace payment_backend.Configuration;

public static class AppConfiguration
{
    public static WebApplication ConfigureApp(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.MapPost("/payment",
            async (
                Payments request,
                [FromServices] ISendProcessor service,
                CancellationToken cancel
            ) =>
            {
                await service.Send(request, cancel);

                return Results.Created(
                    $"/payment/{request.CollelationId}",
                    request
                );
            });

        app.MapGet("/payments-summary",
            async (
                Payments request,
                [FromServices] ISendProcessor service,
                CancellationToken cancel
            ) =>
            {
                await service.Send(request, cancel);

                return Results.Created(
                    $"/payment/{request.CollelationId}",
                    request
                );
            });

        return app;
    }
}
