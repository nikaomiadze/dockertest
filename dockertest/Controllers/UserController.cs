using dockertest.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using dockertest.DTOs;
using dockertest.auth;
using dockertest.Services;


namespace dockertest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly IjwtManager _jwtManager;

        public UserController(MongoService mongoService, IjwtManager jwtManager)
        {
            _users = mongoService.UsersCollection;
            _jwtManager = jwtManager;
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


    }
}
