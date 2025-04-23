using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dockertest.Models
{
    public class Device
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int? UserId { get; set; }

        public string? DeviceId { get; set; }

        public string? Title { get; set; }
    }
}
