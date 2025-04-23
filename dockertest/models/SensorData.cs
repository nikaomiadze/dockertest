using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace dockertest.models
{
    public class SensorData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "DeviceId must be exactly 6 characters.")]
        [BsonElement("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Range(-40, 80, ErrorMessage = "Temperature must be between -40 and 80 Celsius.")]
        [BsonElement("temperature")]
        public double Temperature { get; set; }

        [Range(300, 5000, ErrorMessage = "CO2 level must be between 300 and 5000 ppm.")]
        [BsonElement("co2")]
        public int CO2 { get; set; }

        [Range(0, 100, ErrorMessage = "Humidity must be between 0% and 100%.")]
        [BsonElement("humidity")]
        public int Humidity { get; set; }
    }
}
