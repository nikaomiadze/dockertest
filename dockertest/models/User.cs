namespace dockertest.models
{
    public class User
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public List<string> DeviceIds { get; set; } = new();
    }
}
