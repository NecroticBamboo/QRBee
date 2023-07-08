using MongoDB.Driver;

namespace QRBee.Api
{
    public class DatabaseSettings
    {
        public string? Connection { get; set;}
        public string? DatabaseName { get; set; }

        public MongoClientSettings ToMongoDbSettings()
        {
            var settings = MongoClientSettings.FromConnectionString(Connection);

            return settings;

        }
    }
}
