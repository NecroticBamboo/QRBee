using System.Text.RegularExpressions;
using QRBee.Core;
using QRBee.Core.Data;

namespace QRBee.Api.Services
{
    public class QRBeeAPI: IQRBeeAPI
    {
        private readonly IStorage _storage;

        public QRBeeAPI(IStorage storage)
        {
            _storage = storage;
        }

        public Task<RegistrationResponse> Register(RegistrationRequest request)
        {
            throw new NotImplementedException();

            Validate(request);

            var info = Convert(request);

            if (UserExists(info))
            {
                // throw error
            }
            else
            {
                _storage.PutUserInfo(info);
            }

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

            if (string.IsNullOrEmpty(name) || name.All(char.IsLetter)==false || name.Length>=30)
            {
                // throw exception
            }

            var freq = Regex.Matches(email, '@'.ToString()).Count;

            if (string.IsNullOrEmpty(email) || email.Contains('@')==false || freq>=2 || email.Length >= 30)
            {
                // throw exception
            }

            // DateOfBirth check 

        }

        private UserInfo Convert(RegistrationRequest request)
        {
            return new UserInfo(request.Name, request.Email, request.DateOfBirth);
        }

        private bool UserExists(UserInfo info)
        {
            var user = _storage.GetUserInfo(info.Email);

            return user == info;
        }
    }
}
