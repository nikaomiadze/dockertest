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
            try
            {
                if (data == null)
                {
                    return BadRequest("Sensor data cannot be null");
                }

                if (string.IsNullOrWhiteSpace(data.DeviceId))
                {
                    return BadRequest("Device ID is required");
                }

                var filter = Builders<SensorData>.Filter.Eq(d => d.DeviceId, data.DeviceId);

                var update = Builders<SensorData>.Update
                    .Set(d => d.Temperature, data.Temperature)
                    .Set(d => d.Humidity, data.Humidity)
                    .Set(d => d.CO2, data.CO2)
                    .Set(d => d.Timestamp, data.Timestamp);

                var options = new UpdateOptions { IsUpsert = true };

                await _sensorData.UpdateOneAsync(filter, update, options);

                return Ok(new { message = "Sensor data inserted or updated successfully." });
            }
            catch (MongoException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while processing sensor data: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An unexpected error occurred while processing sensor data: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("{deviceId}")]
        public async Task<IActionResult> Get(string deviceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    return BadRequest("Device ID is required");
                }

                var data = await _sensorData.Find(d => d.DeviceId == deviceId)
                    .SortByDescending(d => d.Timestamp)
                    .Limit(1440) // Last 24h (1/min)
                    .ToListAsync();

                if (data == null || data.Count == 0)
                {
                    return NotFound($"No sensor data found for device {deviceId}");
                }

                return Ok(data);
            }
            catch (MongoException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while retrieving sensor data: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An unexpected error occurred while retrieving sensor data: {ex.Message}");
            }
        }
    }
}