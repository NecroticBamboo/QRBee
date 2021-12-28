using MongoDB.Driver;

namespace QRBee.Api
{
    public class DatabaseSettings
    {
        public string? ConnectionString { get; set;}
        public string? DatabaseName { get; set; }

        public MongoClientSettings ToMongoDbSettings()
        {
            var settings = MongoClientSettings.FromConnectionString(ConnectionString);

            return settings;

        }
    }
}
