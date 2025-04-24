using dockertest.models;
using dockertest.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace dockertest.Services
{
    public class MongoService
    {
        public IMongoCollection<User> UsersCollection { get; }
        public IMongoCollection<SensorData> SensorDataCollection { get; }

        public IMongoCollection<Device> DeviceCollection { get; }


        public MongoService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);

            var usersDb = client.GetDatabase(settings.Value.UsersDatabaseName);
            var co2Db = client.GetDatabase(settings.Value.SensorDataDatabaseName);
            var deviceDb = client.GetDatabase(settings.Value.DeviceDatabaseName);

            UsersCollection = usersDb.GetCollection<User>(settings.Value.UsersCollectionName);
            SensorDataCollection = co2Db.GetCollection<SensorData>(settings.Value.SensorDataCollectionName);
            DeviceCollection = deviceDb.GetCollection<Device>(settings.Value.DeviceCollectionName);
        }
    }

}
