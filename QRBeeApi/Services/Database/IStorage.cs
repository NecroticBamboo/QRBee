using Microsoft.AspNetCore.Identity;

namespace QRBee.Api.Services
{
    public interface IStorage
    {

        void PutUserInfo(UserInfo info);
        UserInfo GetUserInfo(string email);

    }
}
