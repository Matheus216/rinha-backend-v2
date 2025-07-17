using payment_backend.Interfaces;
using payment_backend.models;
using System.Diagnostics;

namespace payment_backend.Service;

public interface ISendProcessor { Task Send(Payments request, CancellationToken cancel); };

public class SendProcessor(IRepository repository) : ISendProcessor
{
    public async Task Send(Payments request, CancellationToken cancel)
    {
        Debug.Write("Saving main request");
        await repository.InsertAsync(request, cancel);
    }
}
