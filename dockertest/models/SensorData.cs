using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace dockertest.models
{
    public class SensorData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string? DeviceId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Range(-40, 80)]
        public double Temperature { get; set; }

        [Range(300, 5000)]
        public int CO2 { get; set; }

        [Range(0,100)]
        public int Humidity { get; set; }
    }
}
