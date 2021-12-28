namespace QRBee.Api.Services.Database
{
    public interface IStorage
    {

        Task<string> PutUserInfo(UserInfo info);
        Task<UserInfo> GetUserInfo(string email);

    }
}
