using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace QRBee.Api.Services.Database
{
    public class Storage: IStorage
    {

        private readonly IMongoDatabase _database;

        public Storage(IMongoClient client, IOptions<DatabaseSettings> settings)
        {
            var name = settings.Value.DatabaseName;
            
            _database = client.GetDatabase(name);
        }

        public async Task<string> PutUserInfo(UserInfo info)
        {
            
            var collection = _database.GetCollection<UserInfo>("Users");

            var user = await TryGetUserInfo(info.Email);

            // Ignore re-register 
            // Potential vulnerability if user registers with other email
            if (user == null)
            {
                await collection.InsertOneAsync(info);
                return info.ClientId ?? throw new ApplicationException($"ClientId is null while adding user {info.Email}");
            }
            return user.ClientId ?? throw new ApplicationException($"ClientId is null while adding user {info.Email}");
            
            
        }

        internal async Task<UserInfo?> TryGetUserInfo(string email)
        {
            var collection = _database.GetCollection<UserInfo>("Users");
            using var cursor = await collection.FindAsync($"{{ Email: \"{email}\" }}");
            if (!await cursor.MoveNextAsync())
            {
                return null;
            }

            return cursor.Current.FirstOrDefault();
        }

        public async Task<UserInfo> GetUserInfo(string email)
        {
            var user = await TryGetUserInfo(email);
            return user ?? throw new ApplicationException($"User {email} not found.");
        }

    }
}
