using dockertest.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using dockertest.Services;


namespace dockertest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorDataController : ControllerBase
    {
        
            private readonly IMongoCollection<SensorData> _sensorData;

            public SensorDataController(MongoService mongoService)
            {
                _sensorData = mongoService.SensorDataCollection;
            }



        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SensorData data)
        {
            var filter = Builders<SensorData>.Filter.Eq(d => d.DeviceId, data.DeviceId);

            var update = Builders<SensorData>.Update
                .Set(d => d.Temperature, data.Temperature)
                .Set(d => d.Humidity, data.Humidity)
                .Set(d => d.Timestamp, data.Timestamp);

            var options = new UpdateOptions { IsUpsert = true };

            await _sensorData.UpdateOneAsync(filter, update, options);

            return Ok(new { message = "Sensor data inserted or updated successfully." });
        }

        [Authorize]
        [HttpGet("{deviceId}")]
        public async Task<IActionResult> Get(string deviceId)
        {
            var data = await _sensorData.Find(d => d.DeviceId == deviceId)
                .SortByDescending(d => d.Timestamp)
                .Limit(1440) // Last 24h (1/min)
                .ToListAsync();
            return Ok(data);
        }
    }
}
