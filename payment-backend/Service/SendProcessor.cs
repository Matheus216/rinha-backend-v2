using payment_backend.Interfaces;
using payment_backend.models;
using payment_backend.Models;
using System.Diagnostics;

namespace payment_backend.Service;

public interface ISendProcessor
{
    Task Send(Payments request, CancellationToken cancel);
    Task<Summary> GetSummaryAsync(DateTime from, DateTime to, CancellationToken cancel);
};

public class SendProcessor(IRepository repository) : ISendProcessor
{
    public async Task<Summary> GetSummaryAsync(DateTime from, DateTime to, CancellationToken cancel)
    {
        var response = await repository.GetSummaryAsync(
            from,
            to,
            cancel
        );

        return response; 
    }

    public async Task Send(Payments request, CancellationToken cancel)
    {
        Debug.Write("Saving main request");
        await repository.InsertAsync(request, cancel);
    }
}
