using dockertest.models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using dockertest.Models;

namespace dockertest.Services
{
    public class MongoService
    {
        public IMongoCollection<User> UsersCollection { get; }
        public IMongoCollection<SensorData> SensorDataCollection { get; }
        public IMongoCollection<Device> DeviceCollection { get; }
        private readonly IMongoDatabase _usersDb; // Store reference to users database for counters

        public MongoService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);

            _usersDb = client.GetDatabase(settings.Value.UsersDatabaseName);
            var co2Db = client.GetDatabase(settings.Value.SensorDataDatabaseName);
            var deviceDb = client.GetDatabase(settings.Value.DeviceDatabaseName);

            UsersCollection = _usersDb.GetCollection<User>(settings.Value.UsersCollectionName);
            SensorDataCollection = co2Db.GetCollection<SensorData>(settings.Value.SensorDataCollectionName);
            DeviceCollection = deviceDb.GetCollection<Device>(settings.Value.DeviceCollectionName);

            // Initialize counters collection if it doesn't exist
            InitializeCounters();
        }

        private void InitializeCounters()
        {
            var countersCollection = _usersDb.GetCollection<BsonDocument>("counters");

            // Create index if it doesn't exist
            if (!countersCollection.Indexes.List().ToList().Any())
            {
                var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending("_id");
                countersCollection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(indexKeys));
            }
        }

        public async Task<int> GetNextUserIdSequence()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "userId");
            var update = Builders<BsonDocument>.Update.Inc("seq", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var result = await _usersDb.GetCollection<BsonDocument>("counters")
                .FindOneAndUpdateAsync(filter, update, options);

            return result["seq"].AsInt32;
        }
    }
}