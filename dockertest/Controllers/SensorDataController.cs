using dockertest.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace dockertest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorDataController : ControllerBase
    {
        private readonly IMongoCollection<SensorData> _collection;

        public SensorDataController(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase("Users");
            _collection = database.GetCollection<SensorData>(settings.Value.CollectionName);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SensorData data)
        {
            await _collection.InsertOneAsync(data);
            return Ok();
        }


        [Authorize]
        [HttpGet("{deviceId}")]
        [Authorize]
        public async Task<IActionResult> Get(string deviceId)
        {
            var data = await _collection.Find(d => d.DeviceId == deviceId)
                .SortByDescending(d => d.Timestamp)
                .Limit(1440) // Last 24h (1/min)
                .ToListAsync();
            return Ok(data);
        }
    }
}
