using QRBee.Api.Services.Database;
using QRBee.Core.Data;
using QRBee.Core.Security;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

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
        private readonly IPaymentGateway _paymentGateway;
        private static readonly object _lock = new ();

        private const int MaxNameLength = 512;
        private const int MaxEmailLength = 512;

        public QRBeeAPIService(IStorage storage, ISecurityService securityService, IPrivateKeyHandler privateKeyHandler, IPaymentGateway paymentGateway)
        {
            _storage = storage;
            _securityService = securityService;
            _privateKeyHandler = privateKeyHandler;
            _paymentGateway = paymentGateway;
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

            ValidateRegistration(request);

            var info = Convert(request);

            var clientId = await _storage.PutUserInfo(info);

            using var rsa = LoadRsaPublicKey(request.CertificateRequest.RsaPublicKey);
            var bytes = rsa.ExportRSAPublicKey();

            var clientCertificate =  _securityService.CreateCertificate(clientId,bytes);
            
            var convertedClientCertificate = Convert(clientCertificate, clientId);
            await _storage.InsertCertificate(convertedClientCertificate);

            return new RegistrationResponse
            {
                ClientId = clientId,
                ClientCertificate = _securityService.Serialize(clientCertificate),
                APIServerCertificate = _securityService.Serialize(_securityService.APIServerCertificate)
            };
        }

        public Task Update(string clientId, RegistrationRequest request)
        {
            ValidateRegistration(request);
            var info = Convert(request);
            return _storage.UpdateUser(info);
        }
        private void ValidateRegistration(RegistrationRequest request)
        {
            if (request == null)
            {
                throw new NullReferenceException();
            }

            var name = request.Name;
            var email = request.Email;
            var dateOfBirth = request.DateOfBirth;
            var certificateRequest = request.CertificateRequest;

            if (string.IsNullOrEmpty(name) || name.All(char.IsLetter) == false || name.Length >= MaxNameLength)
            {
                throw new ApplicationException($"Name \"{name}\" isn't valid");
            }

            var freq = Regex.Matches(email, @"[^@]+@[^@]+").Count;

            if (string.IsNullOrEmpty(email) || email.IndexOf('@') < 0 || freq >= 2 || email.Length >= MaxEmailLength)
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
            using var rsa = LoadRsaPublicKey(certificateRequest.RsaPublicKey);

            var data = Encoding.UTF8.GetBytes(certificateRequest.AsDataForSignature());
            var signature = System.Convert.FromBase64String(certificateRequest.Signature);
            var verified = rsa.VerifyData(
                data,
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            if (!verified)
            {
                throw new ApplicationException($"Digital signature is not valid.");
            }
        }

        public async Task<PaymentResponse> Pay(PaymentRequest value)
        {
            //1. Check payment request parameters for validity
            ValidateTransaction(value);

            //2. Check client signature
            var t2 = CheckSignature(
                value.ClientResponse.AsDataForSignature(),
                value.ClientResponse.ClientSignature,
                value.ClientResponse.ClientId);

            //3. Check merchant signature
            var t3 = CheckSignature(
                value.ClientResponse.MerchantRequest.AsDataForSignature(),
                value.ClientResponse.MerchantRequest.MerchantSignature,
                value.ClientResponse.MerchantRequest.MerchantId);

            //4. Check if transaction was already processed
            var t4 = CheckTransaction(value.ClientResponse.MerchantRequest.MerchantTransactionId);

            //Parallel task execution
            await Task.WhenAll(t2, t3, t4);

            //5. Decrypt client card data
            var clientCardData = DecryptClientData(value.ClientResponse.EncryptedClientCardData);

            //6. Check client card data for validity
            await CheckClientCardData(clientCardData);

            //7. Register preliminary transaction record with expiry of one minute
            var info = Convert(value);
            info.Status = TransactionInfo.TransactionStatus.Pending;

            await _storage.PutTransactionInfo(info);

            //8. Send client card data to a payment gateway
            var res = await _paymentGateway.Payment(info, clientCardData);

            //9. Record transaction with result
            if (res.Success)
            {
                info.Status=TransactionInfo.TransactionStatus.Succeeded;
            }
            else
            {
                info.Status = TransactionInfo.TransactionStatus.Rejected;
                info.RejectReason = res.ErrorMessage;
            }
            await _storage.UpdateTransaction(info);

            //10. Make response for merchant
            var response = new PaymentResponse
            {
                ServerTransactionId = info.TransactionId,
                PaymentRequest = value,
                ServerTimeStampUTC = DateTime.UtcNow,
                Success = res.Success,
                RejectReason = res.ErrorMessage,
            };

            var signature = _securityService.Sign(Encoding.UTF8.GetBytes(response.AsDataForSignature()));
            response.ServerSignature = System.Convert.ToBase64String(signature);
            return response;
        }

        private void ValidateTransaction(PaymentRequest request)
        {
            if (request == null)
            {
                throw new NullReferenceException();
            }

            var clientId = request.ClientResponse.ClientId;
            var merchantId = request.ClientResponse.MerchantRequest.MerchantId;
            var transactionId = request.ClientResponse.MerchantRequest.MerchantTransactionId;
            var amount = request.ClientResponse.MerchantRequest.Amount;

            if (clientId == null || merchantId == null || transactionId == null)
            {
                throw new ApplicationException("Id isn't valid");
            }

            if (amount is <= 0 or >= 10000)
            {
                throw new ApplicationException($"Amount \"{amount}\" isn't valid");
            }
        }

        private async Task CheckSignature(string data,string signature, string id)
        {
            var info = await _storage.GetCertificateInfoByUserId(id);
            var certificate = _securityService.Deserialize(info.Certificate);

            var check = _securityService.Verify(
                Encoding.UTF8.GetBytes(data),
                System.Convert.FromBase64String(signature),
                certificate);
            
            if (!check)
            {
                throw new ApplicationException($"Signature is incorrect for Id: {id}.");
            }
        }

        private async Task CheckTransaction(string transactionId)
        {
            var info = await _storage.GetTransactionInfoByTransactionId(transactionId);
            switch (info.Status)
            {
                case TransactionInfo.TransactionStatus.Succeeded:
                    throw new ApplicationException($"Transaction with Id: {transactionId} was already made.");
                case TransactionInfo.TransactionStatus.Rejected:
                    throw new ApplicationException($"Transaction with Id: {transactionId} is not valid.");
                case TransactionInfo.TransactionStatus.Pending:
                    throw new ApplicationException($"Transaction with Id: {transactionId} is already in progress.");
                default:
                    return;
            }
        }

        private ClientCardData DecryptClientData(string encryptedClientCardData)
        {
            var info = System.Convert.FromBase64String(encryptedClientCardData);
            var bytes = _securityService.Decrypt(info);
            var s = Encoding.UTF8.GetString(bytes);
            return ClientCardData.FromString(s);
        }

        private async Task CheckClientCardData(ClientCardData data)
        {
            var transactionId = data.TransactionId;
            var expirationDate = DateTime.Parse(data.ExpirationDateMMYY);
            var validFrom = DateTime.Parse(data.ValidFrom);
            var holderName = data.CardHolderName;

            await CheckTransaction(transactionId);

            if (expirationDate <= DateTime.UtcNow)
            {
                throw new ApplicationException($"The expiration date: {expirationDate} is wrong");
            }

            if (validFrom > DateTime.UtcNow)
            {
                throw new ApplicationException($"The valid from date: {validFrom} is wrong");
            }

            if (holderName.Any(char.IsDigit))
            {
                throw new ApplicationException($"The card holder name: {holderName} is wrong");
            }
        }

        private static RSA LoadRsaPublicKey(StringRSAParameters stringParameters)
        {
            var rsaParameters = new RSAParameters
            {
                Exponent = SafeConvertFromBase64(stringParameters.StringExponent),
                Modulus  = SafeConvertFromBase64(stringParameters.StringModulus),
                P        = SafeConvertFromBase64(stringParameters.StringP),
                Q        = SafeConvertFromBase64(stringParameters.StringQ),
                DP       = SafeConvertFromBase64(stringParameters.StringDP),
                DQ       = SafeConvertFromBase64(stringParameters.StringDQ),
                InverseQ = SafeConvertFromBase64(stringParameters.StringInverseQ),
                D        = SafeConvertFromBase64(stringParameters.StringD)
            };

            var rsa = RSA.Create(rsaParameters);
            if (rsa == null)
                throw new CryptographicException("Can't create public key");
            return rsa;
        }

        private static byte[]? SafeConvertFromBase64(string? s) => string.IsNullOrWhiteSpace(s) ? null : System.Convert.FromBase64String(s);

        private static UserInfo Convert(RegistrationRequest request)
        {
            return new UserInfo(request.Name, request.Email, request.DateOfBirth);
        }

        private static TransactionInfo Convert(PaymentRequest request)
        {
            return new TransactionInfo(request, DateTime.UtcNow);
        }

        private CertificateInfo Convert(X509Certificate2 certificate, string clientId)
        {
            var convertedCertificate = _securityService.Serialize(certificate);
            return new CertificateInfo
            {
                Id              = certificate.SerialNumber, 
                ClientId        = clientId, 
                Certificate     = convertedCertificate, 
                ServerTimeStamp = DateTime.UtcNow
            };
        }

    }
}
