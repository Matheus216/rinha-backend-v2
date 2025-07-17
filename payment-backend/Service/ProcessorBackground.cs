using payment_backend.Interfaces;
using payment_backend.models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace payment_backend.Service;

public class ProcessorBackground(
    IHttpClientFactory factory,
    IRepository repository
) : BackgroundService
{
    private readonly HttpClient mainClient = factory.CreateClient("main");
    private readonly HttpClient fallbackClient = factory.CreateClient("fallback");

    protected override async Task ExecuteAsync(CancellationToken cancel)
    {
        var payments = await repository.GetPaymentsPendingAsync(cancel);

        var fallback = payments.Where(x => x.Attemped >= 3);
        var main = payments.Where(x => x.Attemped < 3);

        Task.WaitAll([
            SendMainAsync(main, cancel),
            SendFallbackAsync(fallback, cancel)
        ], cancel);
    }

    public async Task SendMainAsync(IEnumerable<Payments> payments, CancellationToken cancel)
    {
        foreach (var x in payments)
        {
            var response = await mainClient.PostAsync(
                "/payments/",
                new StringContent(
                    JsonSerializer.Serialize(x),
                    Encoding.UTF8,
                    "application/json"
                ),
                cancel
            );

            if (response.IsSuccessStatusCode)
            {
                x.Sended = true;
                x.IsMain = true;
                Debug.Write($"Sended: {response.Content}");
                await repository.UpdateStatusAsync(x, cancel);
            }
            else
            {
                x.Attemped++;
                await repository.UpdateStatusAsync(x, cancel);
                Debug.Write($"Error request, content: {response.Content}");
            }
        }
    }
    
    public async Task SendFallbackAsync(IEnumerable<Payments> payments, CancellationToken cancel)
    {
        foreach (var x in payments)
        {
            var response = await fallbackClient.PostAsync(
                "/payments/",
                new StringContent(
                    JsonSerializer.Serialize(x),
                    Encoding.UTF8,
                    "application/json"
                ),
                cancel
            );

            if (response.IsSuccessStatusCode)
            {
                x.Sended = true;
                x.IsMain = false;
                Debug.Write($"Sended: {response.Content}");
                await repository.UpdateStatusAsync(x, cancel);
            }
            else
            {
                x.Attemped = 0;
                await repository.UpdateStatusAsync(x, cancel);
                Debug.Write($"Error request, content: {response.Content}");
            }
        }
    }
}
