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
                [FromBody] Payments request,
                [FromServices] ISendProcessor service,
                CancellationToken cancel
            ) =>
            {
                await service.Send(request, cancel);

                return Results.Created(
                    $"/payment/{request.CorrelationId}",
                    request
                );
            });

        app.MapGet("/payments-summary",
            async (
                [FromQuery] DateTime from, 
                [FromQuery] DateTime to,
                [FromServices] ISendProcessor service,
                CancellationToken cancel
            ) =>
            {
                var response = await service.GetSummaryAsync(from, to, cancel); 
                return Results.Ok(response); 
            });

        app.MapPost("/startstop", () => {
            ProcessorBackground.StartStop = !ProcessorBackground.StartStop;

            return Results.Ok(new
            {
                message = $"altered to {ProcessorBackground.StartStop}"
            });
        });

        return app;
    }
}
