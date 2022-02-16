using QRBee.Core.Security;

namespace QRBee.Services
{
    /// <summary>
    /// Private key handler for Client side
    /// </summary>
    public class ClientPrivateKeyHandler : PrivateKeyHandlerBase
    {

        private const string VeryBadAndInsecureCertificatePassword = "Rî‹T=›'ÄÎ¶gÚrÊ¯ÑŽ™pudF";

        public ClientPrivateKeyHandler(ILocalSettings settings)
        {
            CertificatePassword = VeryBadAndInsecureCertificatePassword;

            var clientSettings = settings.LoadSettings();
            CommonName = clientSettings?.ClientId;
        }

    }
}

