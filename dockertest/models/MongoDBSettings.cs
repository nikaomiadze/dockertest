﻿namespace dockertest.models
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string UsersCollectionName { get; set; } = null!;
        public string DeviceCollectionName { get; set; } = null!;
        public string SensorDataCollectionName { get; set; } = null!;
        public string SensorDataDatabaseName { get; set; } = null!;
        public string UsersDatabaseName { get; set; } = null!;
        public string DeviceDatabaseName { get; set; } = null!;
    }

}
