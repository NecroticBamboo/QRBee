using System.Globalization;
using System.Text.RegularExpressions;
using QRBee.Api.Services.Database;
using QRBee.Core;
using QRBee.Core.Data;

namespace QRBee.Api.Services
{
    public class QRBeeAPI: IQRBeeAPI
    {
        private readonly IStorage _storage;
        private const int MaxNameLength = 512;
        private const int MaxEmailLength = 512;

        public QRBeeAPI(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<RegistrationResponse> Register(RegistrationRequest request)
        {

            Validate(request);

            var info = Convert(request);
            
            var clientId = await _storage.PutUserInfo(info);

            return new RegistrationResponse{ClientId = clientId};
        }

        private static void Validate(RegistrationRequest request)
        {
            if (request == null)
            {
                throw new NullReferenceException();
            }

            var name = request.Name;
            var email = request.Email;
            var dateOfBirth = request.DateOfBirth;

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

        }

        private UserInfo Convert(RegistrationRequest request)
        {
            return new UserInfo(request.Name, request.Email, request.DateOfBirth);
        }

    }
}
