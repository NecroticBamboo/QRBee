using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QRBee.Api.Controllers;

namespace QRBee.Api.Services.Database
{
    public class Storage: IStorage
    {

        private readonly IMongoDatabase _database;
        private readonly ILogger<Storage> _logger;

        public Storage(IMongoClient client, IOptions<DatabaseSettings> settings, ILogger<Storage> logger)
        {
            var name = settings.Value.DatabaseName;
            
            _database = client.GetDatabase(name);
            _logger = logger;
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
                _logger.LogInformation($"Inserted new user with Email: {info.Email} and ID: {info.ClientId}");
                return info.ClientId ?? throw new ApplicationException($"ClientId is null while adding user {info.Email}");
            }

            // Update command will not be used due to not knowing if the user is legitimate
            _logger.LogInformation($"Found user with Email: {info.Email} and ID: {info.ClientId}");
            return user.ClientId ?? throw new ApplicationException($"ClientId is null while adding user {info.Email}");
        }

        /// <summary>
        /// Check if user already exists in database
        /// </summary>
        /// <param name="email">Parameter by which to find UserInfo</param>
        /// <returns>null if user doesn't exist or UserInfo</returns>
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

        public async Task UpdateUser(UserInfo info)
        {
            var collection = _database.GetCollection<UserInfo>("Users");
            await collection.ReplaceOneAsync($"{{ _id: \"{info.Id}\" }}",info, new ReplaceOptions(){IsUpsert = false});
        }

        public async Task PutTransactionInfo(TransactionInfo info)
        {
            var collection = _database.GetCollection<TransactionInfo>("Transactions");

            var transaction = await TryGetTransactionInfo(info.Id);

            if (transaction == null)
            {
                await collection.InsertOneAsync(info);
                _logger.LogInformation($"Inserted new transaction with ID: {info.Id}");
                return;
            }

            _logger.LogInformation($"Found transaction with ClientId: {info.Id}");
        }

        /// <summary>
        /// Try to find if the Transaction already exists in the database
        /// </summary>
        /// <param name="id">parameter by which to find TransactionInfo</param>
        /// <returns>null if transaction doesn't exist or TransactionInfo</returns>
        private async Task<TransactionInfo?> TryGetTransactionInfo(string id)
        {
            var collection = _database.GetCollection<TransactionInfo>("Transactions");
            using var cursor = await collection.FindAsync($"{{ Id: \"{id}\" }}");
            if (!await cursor.MoveNextAsync())
            {
                return null;
            }

            return cursor.Current.FirstOrDefault();
        }
    }
}
