using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace payment_backend.models;

public record Payments
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string CorrelationId { get; init; } = string.Empty;

    [BsonElement("amount")]
    public double Amount { get; init; }

    [BsonElement("sended")]
    [JsonIgnore]
    public bool Sended { get; set; }

    [BsonElement("Type")]
    [JsonIgnore]
    public string Type { get; set; } = string.Empty;

    [BsonElement("attemped")]
    [JsonIgnore]
    public short Attemped { get; set; }

    [BsonElement("createdDate")]
    [JsonIgnore]
    public DateTime CreatedDate { get; init; } = DateTime.Now;
}

