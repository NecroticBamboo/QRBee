namespace QRBee.Api.Services.Database
{
    /// <summary>
    /// Database interface
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Insert userInfo into database
        /// </summary>
        /// <param name="info"> Information to be inserted</param>
        /// <returns></returns>
        Task<string> PutUserInfo(UserInfo info);

        /// <summary>
        /// Retrieve user information from database
        /// </summary>
        /// <param name="email">Identifier by which user information will be retrieved</param>
        /// <returns>User information</returns>
        Task<UserInfo> GetUserInfo(string email);

        Task UpdateUser(UserInfo info);

    }
}
