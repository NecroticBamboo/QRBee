using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
            using var cursor = await collection.FindAsync($"{{ _id: \"{id}\" }}");
            if (!await cursor.MoveNextAsync())
            {
                return null;
            }

            return cursor.Current.FirstOrDefault();
        }

        public async Task<TransactionInfo> GetTransactionInfoByTransactionId(string id)
        {
            var transaction = await TryGetTransactionInfo(id);
            return transaction ?? throw new ApplicationException($"Transaction with Id: {id} not found.");
        }

        public async Task UpdateTransaction(TransactionInfo info)
        {
            var collection = _database.GetCollection<TransactionInfo>("Transactions");
            await collection.ReplaceOneAsync($"{{ _id: \"{info.Id}\" }}", info, new ReplaceOptions() { IsUpsert = false });
        }

        public async Task InsertCertificate(CertificateInfo info)
        {
            var collection = _database.GetCollection<CertificateInfo>("Certificates");

            if (info.Id == null)
            {
                throw new ApplicationException("Info Id is null.");
            }

            var certificate = await TryGetCertificateInfoByEmail(info.Email ?? throw new ApplicationException("Email is not set"));

            if (certificate == null)
            {
                await collection.InsertOneAsync(info);
                _logger.LogInformation($"Inserted new certificate with ID: {info.Id}");
                return;
            }

            await collection.DeleteOneAsync($"{{ _id: \"{certificate.Id}\" }}");
            await collection.ReplaceOneAsync($"{{ _id: \"{info.Id}\" }}", info, new ReplaceOptions() { IsUpsert = true });

            _logger.LogInformation($"Found certificate with old ID: {certificate.Id} and replaced with new ID: {info.Id}");

        }

        /// <summary>
        /// Try to find if the Certificate already exists in the database
        /// </summary>
        /// <param name="id">parameter by which to find CertificateInfo</param>
        /// <returns>null if certificate doesn't exist or CertificateInfo</returns>
        private async Task<CertificateInfo?> TryGetCertificateInfo(string id)
        {
            var collection = _database.GetCollection<CertificateInfo>("Certificates");
            using var cursor = await collection.FindAsync($"{{ Id: \"{id}\" }}");
            if (!await cursor.MoveNextAsync())
            {
                return null;
            }

            return cursor.Current.FirstOrDefault();
        }

        /// <summary>
        /// Try to find if the Certificate already exists in the database
        /// </summary>
        /// <param name="email">parameter by which to find CertificateInfo</param>
        /// <returns>null if certificate doesn't exist or CertificateInfo</returns>
        private async Task<CertificateInfo?> TryGetCertificateInfoByEmail(string email)
        {
            var collection = _database.GetCollection<CertificateInfo>("Certificates");
            using var cursor = await collection.FindAsync($"{{ Email: \"{email}\" }}");
            if (!await cursor.MoveNextAsync())
            {
                return null;
            }

            return cursor.Current.FirstOrDefault();
        }

        public async Task<CertificateInfo> GetCertificateInfoByCertificateId(string id)
        {
            var certificate = await TryGetCertificateInfo(id);
            return certificate ?? throw new ApplicationException($"Certificate with Id: {id} not found.");
        }

        public async Task<CertificateInfo> GetCertificateInfoByUserId(string clientId)
        {
            var collection = _database.GetCollection<CertificateInfo>("Certificates");
            using var cursor = await collection.FindAsync($"{{ ClientId: \"{clientId}\" }}");
            if (!await cursor.MoveNextAsync())
            {
                throw new ApplicationException($"Certificate with ClientId: {clientId} not found.");
            }

            return cursor.Current.FirstOrDefault() ?? throw new ApplicationException($"Certificate with ClientId: {clientId} not found.");
        }

    }
}
