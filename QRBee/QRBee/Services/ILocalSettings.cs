using System;
using System.Threading.Tasks;

namespace QRBee.Services
{
    public class Settings
    {
        
        public string ClientId       { get; set; }
        public string PIN            { get; set; }
        public bool IsRegistered => !string.IsNullOrWhiteSpace(ClientId); 

        public string Name           { get; set; }
        public string Email          { get; set; }
        public DateTime DateOfBirth    { get; set; }
        public string CardNumber     { get; set; }
        public string ValidFrom      { get; set; }
        public string ExpirationDate { get; set; }
        public string CardHolderName { get; set; }
        public string CVC            { get; set; }
        public int? IssueNo        { get; set; }
        public string Password       { get; set; }
    }

    public interface ILocalSettings
    {
        string QRBeeApiUrl { get; }
        Task SaveSettings(Settings settings);
        Settings LoadSettings();


    }
}
