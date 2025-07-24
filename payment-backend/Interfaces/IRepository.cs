using payment_backend.models;
using payment_backend.Models;

namespace payment_backend.Interfaces;

public interface IRepository
{
    Task<Summary> GetSummaryAsync(DateTime From, DateTime To, CancellationToken cancel);
    Task InsertAsync(Payments payment, CancellationToken cancel);
    Task UpdateStatusAsync(string id, string type, bool sended, int attempeds, CancellationToken cancel);
    Task<IEnumerable<Payments>> GetPaymentsPendingAsync(CancellationToken cancel);
}
