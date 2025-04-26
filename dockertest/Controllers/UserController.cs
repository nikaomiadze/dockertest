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
        private readonly MongoService _mongoService;

        public UserController(MongoService mongoService, IjwtManager jwtManager)
        {
            _users = mongoService.UsersCollection;
            _jwtManager = jwtManager;
            _device = mongoService.DeviceCollection;
            _mongoService = mongoService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDto)
        {
            try
            {
                // Check if user already exists
                if (await _users.Find(u => u.Username == registerDto.Username).AnyAsync())
                    return BadRequest("Username already exists");

                var user = new User
                {
                    Id = await _mongoService.GetNextUserIdSequence(),
                    Username = registerDto.Username,
                    Password = registerDto.Password,
                };

                await _users.InsertOneAsync(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while registering user: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                var user = await _users.Find(u => u.Username == loginDto.Username).FirstOrDefaultAsync();

                if (user == null)
                    return Unauthorized("Invalid username or password");

                var token = _jwtManager.GetToken(user);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred during login: {ex.Message}");
            }
        }

        [HttpPost("add-device")]
        public async Task<IActionResult> AddDevice([FromBody] AddDeviceDTO deviceDto)
        {
            try
            {
                string? userId = User.FindFirst("UserID")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token");

                if (!int.TryParse(userId, out int parsedUserId))
                    return BadRequest("Invalid User ID format");

                var device = new Device
                {
                    UserId = parsedUserId,
                    DeviceId = deviceDto.DeviceId,
                    Title = deviceDto.Title
                };

                await _device.InsertOneAsync(device);
                return Ok(device);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding device: {ex.Message}");
            }
        }

        
        [HttpGet("devices")]
        public async Task<IActionResult> GetDevicesByUserId()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserID");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized("Invalid token");

                var devices = await _device
                    .Find(d => d.UserId == userId)
                    .ToListAsync();

                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving devices: {ex.Message}");
            }
        }
    }
}