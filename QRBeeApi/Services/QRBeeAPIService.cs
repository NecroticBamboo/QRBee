using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using QRBee.Api.Services.Database;
using QRBee.Core;
using QRBee.Core.Data;
using QRBee.Core.Security;

namespace QRBee.Api.Services
{
    /// <summary>
    /// Implementation of <see href="IQRBeeAPI"/>
    /// </summary>
    public class QRBeeAPIService: IQRBeeAPI
    {
        private readonly IStorage _storage;
        private readonly ISecurityService _securityService;
        private readonly IPrivateKeyHandler _privateKeyHandler;
        private static readonly object _lock = new ();

        private const int MaxNameLength = 512;
        private const int MaxEmailLength = 512;

        public QRBeeAPIService(IStorage storage, ISecurityService securityService, IPrivateKeyHandler privateKeyHandler)
        {
            _storage = storage;
            _securityService = securityService;
            _privateKeyHandler = privateKeyHandler;
            Init(_privateKeyHandler);
        }

        private static void Init(IPrivateKeyHandler privateKeyHandler)
        {
            lock (_lock)
            {
                if (!privateKeyHandler.Exists())
                {
                    privateKeyHandler.GeneratePrivateKey();
                }
            }
        }

        public async Task<RegistrationResponse> Register(RegistrationRequest request)
        {

            Validate(request);

            var info = Convert(request);

            var clientId = await _storage.PutUserInfo(info);
            var clientCertificate =  _securityService.CreateCertificate(clientId,System.Convert.FromBase64String(request.CertificateRequest.RsaPublicKey));
            //TODO save certificate to certificate mongoDB collection

            return new RegistrationResponse
            {
                ClientId = clientId,
                Certificate = _securityService.Serialize(clientCertificate)
            };
        }

        public Task Update(string clientId, RegistrationRequest request)
        {
            Validate(request);
            var info = Convert(request);
            return _storage.UpdateUser(info);
        }

        public Task InsertTransaction(PaymentRequest value)
        {
            var info = Convert(value);
            return _storage.PutTransactionInfo(info);
        }

        private void Validate(RegistrationRequest request)
        {
            if (request == null)
            {
                throw new NullReferenceException();
            }

            var name = request.Name;
            var email = request.Email;
            var dateOfBirth = request.DateOfBirth;
            var certificateRequest = request.CertificateRequest;

            if (string.IsNullOrEmpty(name) || name.All(char.IsLetter)==false || name.Length>=MaxNameLength)
            {
                throw new ApplicationException($"Name \"{name}\" isn't valid");
            }

            var freq = Regex.Matches(email, @"[^@]+@[^@]+").Count;

            if (string.IsNullOrEmpty(email) || email.IndexOf('@')<0 || freq>=2 || email.Length >= MaxEmailLength)
            {
                throw new ApplicationException($"Email \"{email}\" isn't valid");
            }

            if (!DateTime.TryParseExact(dateOfBirth, "yyyy-MM-dd", null, DateTimeStyles.AssumeUniversal, out var check)
                || check > DateTime.UtcNow - TimeSpan.FromDays(365 * 8)
                || check < DateTime.UtcNow - TimeSpan.FromDays(365 * 100)
               )
            {
                throw new ApplicationException($"DateOfBirth \"{dateOfBirth}\" isn't valid");
            }

            //Check digital signature
            var verified = _securityService.Verify(
                Encoding.UTF8.GetBytes(certificateRequest.AsDataForSignature()),
                Encoding.UTF8.GetBytes(certificateRequest.Signature),
                _privateKeyHandler.LoadPrivateKey());
            if (!verified)
            {
                throw new ApplicationException($"Digital signature is not valid.");
            }
        }

        private static UserInfo Convert(RegistrationRequest request)
        {
            return new UserInfo(request.Name, request.Email, request.DateOfBirth);
        }

        private static TransactionInfo Convert(PaymentRequest request)
        {
            return new TransactionInfo(request, DateTime.UtcNow);
        }

    }
}
