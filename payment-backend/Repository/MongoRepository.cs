using payment_backend.Interfaces;
using payment_backend.models;
using payment_backend.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace payment_backend.Repository;

public class MongoRepository : IRepository
{
    private readonly IMongoCollection<Payments> _collection;

    public MongoRepository(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["DATABASE:MONGODB:CONNECTION"]);
        var _database = client.GetDatabase(configuration["DATABASE:MONGODB:DATABASE"]);
        _collection = _database.GetCollection<Payments>(configuration["DATABASE:MONGODB:COLLECTION"]);
    }

    public async Task<IEnumerable<Payments>> GetPaymentsPendingAsync(CancellationToken cancel)
    {
        var filter = new BsonDocument("sended", false);
        return await _collection.Find(filter).ToListAsync(cancel);
    }

    public async Task<Summary> GetSummaryAsync(DateTime From, DateTime To, CancellationToken cancel)
    {
        var filters = new BsonDocument[3];

        filters[0] = new BsonDocument("status", true);
        filters[1] = new BsonDocument("date", new BsonDocument("$gte", From.Date));
        filters[2] = new BsonDocument("date", new BsonDocument("$lte", From.Date));

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument($"and", new BsonArray(filters))),
            new BsonDocument("$group", new BsonDocument
            {
                { "IsMain", "$IsMain" },
                { "TotalAmount", new BsonDocument("$sum", "$amount") }
            }),
            new BsonDocument("$count", "TotalRequets"),
            new BsonDocument("$project", new BsonDocument
            {
                { "IsMain", "$IsMain" },
                { "TotalAmount", "$TotalAmount" },
                { "TotalRequets", "$TotalRequets" },
            })
        };

        var response = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancel)
            .ToListAsync();

        return new Summary(new(1, 1), new(1, 1));
    }

    public async Task InsertAsync(Payments payment, CancellationToken cancel)
        => await _collection.InsertOneAsync(payment, cancellationToken: cancel);

    public Task UpdateStatusAsync(Payments payments, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}
