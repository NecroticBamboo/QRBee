using QRBee.Core.Data;
using QRBee.Core.Security;

namespace QRBee.Load.Generator;

public class ClientSettings
{
    private ISecurityService _securityService;

    public ClientSettings(SecurityServiceFactory securityServiceFactory, int no, bool isMerchant)
    {
        Id             = no;
        IsMerchant     = isMerchant;
        CardNumber     = IsMerchant ? "": $"123400000000{no:0000}";
        CardHolderName = IsMerchant ? $"Merchant {no}" : $"Mr {no}";
        CVC            = IsMerchant ? "" : $"{no:000}";
        ExpirationDate = IsMerchant ? "" : (DateTime.Now.Date + TimeSpan.FromDays(364)).ToString("yyyy-MM");
        ValidFrom      = IsMerchant ? "" : (DateTime.Now.Date - TimeSpan.FromDays(7)).ToString("yyyy-MM");
        Email          = IsMerchant ? $"{no}@merchant.org" : $"{no}@client.org";

        _securityService = securityServiceFactory(no);
    }

    public int    Id              { get; }
    public string? ClientId       { get; private set; }
    public string CardNumber      { get; }
    public string CardHolderName  { get; }
    public string CVC             { get; }
    public int? IssueNo           { get; }
    public string? ExpirationDate { get; }
    public string? ValidFrom      { get; }

    public bool IsMerchant        { get; }
    public string Email           { get; }
    public ISecurityService SecurityService { get => _securityService; }

    public async Task InitialSetup(QRBee.Core.Client.Client client)
    {
        var idFileName = Environment.ExpandEnvironmentVariables($"%TEMP%/!QRBee/QRBee-{Id:8X}.txt");

        if (File.Exists(idFileName))
        {
            var l = File.ReadAllLines(idFileName);
            if (l != null && l.Length > 0)
                ClientId = l[0];
        }
        
        if (!string.IsNullOrWhiteSpace(ClientId) && SecurityService.PrivateKeyHandler.Exists())
            return;

        var request = new RegistrationRequest
        {
            Name               = CardHolderName,
            Email              = Email,
            DateOfBirth        = "2000-01-01",
            RegisterAsMerchant = IsMerchant,
            CertificateRequest = SecurityService.PrivateKeyHandler.CreateCertificateRequest(Email)
        };

        var resp = await client.RegisterAsync(request);
        _securityService.APIServerCertificate = _securityService.Deserialize(resp.APIServerCertificate);

        ClientId = resp.ClientId;
        File.WriteAllText(idFileName, ClientId);

    }

}
