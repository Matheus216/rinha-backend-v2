using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace payment_backend.models;

public record Payments
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CollelationId { get; init; } = string.Empty;

    [BsonElement("amount")]
    public double Amount { get; init; }

    [BsonElement("sended")]
    [JsonIgnore]
    public bool Sended { get; set; }

    [BsonElement("IsMain")]
    [JsonIgnore]
    public bool IsMain { get; set; }

    [BsonElement("attemped")]
    [JsonIgnore]
    public short Attemped { get; set; }
}

