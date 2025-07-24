using payment_backend.Interfaces;
using payment_backend.models;
using payment_backend.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;

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
        var filters = new BsonDocument[1];

        filters[0] = new BsonDocument("status", true);

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("sended", true)),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$Type" },
                { "TotalAmount", new BsonDocument("$sum", "$amount") },
                { "TotalRequests", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "TotalAmount", 1 },
                { "TotalRequests", 1 },
            })
        };

        var response = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancel)
            .ToListAsync(cancellationToken: cancel);

        return new Summary(
            new(response[0].AsBsonDocument["TotalRequests"].AsInt32, response[0].AsBsonDocument["TotalAmount"].AsDouble),
            new(response[1].AsBsonDocument["TotalRequests"].AsInt32, response[1].AsBsonDocument["TotalAmount"].AsDouble)
        );
    }

    public async Task InsertAsync(Payments payment, CancellationToken cancel)
        => await _collection.InsertOneAsync(payment, cancellationToken: cancel);

    public async Task UpdateStatusAsync(string id, string type, bool sended, int attempeds, CancellationToken cancel)
    {
        var filter = Builders<Payments>.Filter.Eq(x => x.CorrelationId, id);

        var updateDefinition = Builders<Payments>.Update
            .Set(p => p.Type, type)
            .Set(p => p.Sended, sended)
            .Set(p => p.Attemped, attempeds);

        await _collection.UpdateOneAsync(filter, updateDefinition, cancellationToken: cancel);
    }
}
