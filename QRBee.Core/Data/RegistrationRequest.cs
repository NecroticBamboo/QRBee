namespace QRBee.Core.Data
{
    public record RegistrationRequest
    {
        public string Name
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public string DateOfBirth
        {
            get;
            set;
        }

        public string CertificateRequest
        {
            get;
            set;
        }

        public bool RegisterAsMerchant
        {
            get;
            set;
        }


    }
}
