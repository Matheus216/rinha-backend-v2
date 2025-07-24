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
    private readonly HttpClient _mainClient = factory.CreateClient("main");
    private readonly HttpClient _fallbackClient = factory.CreateClient("fallback");

    public static bool StartStop = false;
    protected override async Task ExecuteAsync(CancellationToken cancel)
    {
        Debug.Write("Starting background service");
        while (!cancel.IsCancellationRequested)
        {
            if (StartStop)
            {
                try
                {
                    var payments = await repository
                        .GetPaymentsPendingAsync(cancel);

                    var paymentsEnumerable = payments as Payments[] ?? payments.ToArray();

                    var paymentsArray = new Payments[paymentsEnumerable.Count()];
                    paymentsEnumerable.CopyTo(paymentsArray, 0);

                    var fallback = paymentsArray.Where(x => x.Attemped >= 3);
                    var main = paymentsArray.Where(x => x.Attemped < 3);

                    Task.WaitAll([
                        SendMainAsync(main, cancel),
                        SendFallbackAsync(fallback, cancel)
                    ], cancel);
                }
                catch (System.Exception e)
                {
                    Debug.Fail($"Error: {e.Message}");
                }
            }
            
            await Task.Delay(1000, cancel);
        }
    }

    public async Task SendMainAsync(IEnumerable<Payments> payments, CancellationToken cancel)
    {
        foreach (var x in payments)
        {
            var response = await _mainClient.PostAsync(
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
                Debug.Write($"Sended: {response.Content}");
                await repository.UpdateStatusAsync(
                    x.CorrelationId,
                    "default",
                    true,
                    0,
                    cancel
                );
            }
            else
            {
                x.Attemped++;
                await repository.UpdateStatusAsync(
                    x.CorrelationId,
                    "default",
                    false,
                    x.Attemped,
                    cancel
                );
                Debug.Write($"Error request, content: {response.Content}");
            }
        }
    }
    
    public async Task SendFallbackAsync(IEnumerable<Payments> payments, CancellationToken cancel)
    {
        foreach (var x in payments)
        {
            var response = await _fallbackClient.PostAsync(
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
                Debug.Write($"Sended: {response.Content}");
                await repository.UpdateStatusAsync(
                    x.CorrelationId,
                    "fallback",
                    true,
                    0,
                    cancel
                );
            }
            else
            {
                x.Attemped++;
                await repository.UpdateStatusAsync(
                    x.CorrelationId,
                    "fallback",
                    false,
                    x.Attemped,
                    cancel
                );
                var bodyResponse = response.Content.ReadAsStringAsync();
                Debug.Write($"Error request, content: {bodyResponse}");
            }
        }
    }
}
