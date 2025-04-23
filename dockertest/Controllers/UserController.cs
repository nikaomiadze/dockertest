using dockertest.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using dockertest.DTOs;
using dockertest.auth;
using dockertest.Services;
using dockertest.Models;
using Microsoft.AspNetCore.Authorization;


namespace dockertest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Device> _device;
        private readonly IjwtManager _jwtManager;

        public UserController(MongoService mongoService, IjwtManager jwtManager)
        {
            _users = mongoService.UsersCollection;
            _jwtManager = jwtManager;
            _device = mongoService.DeviceCollection;
        }

        [HttpPost("register")]
        public async Task<User> Register(RegisterDTO registerDto)
        {
            // Check if user already exists
            if (await _users.Find(u => u.Username == registerDto.Username).AnyAsync())
                throw new Exception("Username already exists");

            var user = new User
            {
                Username = registerDto.Username,
                Password = registerDto.Password,
            };

            await _users.InsertOneAsync(user);
            return user;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var user = await _users.Find(u => u.Username == loginDto.Username).FirstOrDefaultAsync();

            if (user == null)
                return Unauthorized("Invalid username or password");

            var token = _jwtManager.GetToken(user);
            return Ok(token);
        }


        [Authorize]
        [HttpPost("add-device")]
        public async Task<IActionResult> AddDevice([FromBody] AddDeviceDTO deviceDto)
        {
            string? userId = User.FindFirst("UserID")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var device = new Device
            {
                UserId = int.Parse(userId),
                DeviceId = deviceDto.DeviceId,
                Title = deviceDto.Title
            };

            await _device.InsertOneAsync(device);
            return Ok(device);
        }

        [Authorize]
        [HttpGet("devices")]
        public async Task<IActionResult> GetDevicesByUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserID");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid token");

            var devices = await _device
                .Find(d => d.UserId == userId)
                .ToListAsync();

            return Ok(devices);
        }



    }
}
