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
            await _sensorData.InsertOneAsync(data);
            return Ok();
        }


        [Authorize]
        [HttpGet("{deviceId}")]
        [Authorize]
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
