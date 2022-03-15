using QRBee.Core.Data;
using QRBee.Core.Security;
using QRBee.Services;
using System;
using System.Text;
using Xamarin.Forms;

namespace QRBee.ViewModels
{
    internal class ClientPageViewModel: BaseViewModel
    {
        private readonly IQRScanner _scanner;
        private readonly ISecurityService _securityService;
        private readonly ILocalSettings _localSettings;
        public bool _isAcceptDenyButtonVisible;
        public bool _isQrVisible;
        public bool _isScanButtonVisible;


        public string _amount;
        private string _qrCode;
        private MerchantToClientRequest _merchantToClientRequest;

        public ClientPageViewModel(IQRScanner scanner, ISecurityService securityService, ILocalSettings localSettings)
        {
            _scanner = scanner;
            _securityService = securityService;
            _localSettings = localSettings;
            ScanCommand = new Command(OnScanButtonClicked);
            AcceptQrCommand = new Command(OnAcceptQrCommand);
            DenyQrCommand = new Command(OnDenyQrCommand);

            IsScanButtonVisible = true;
        }

        public Command ScanCommand
        {
            get;
        }

        public Command AcceptQrCommand 
        { 
            get; 
        }

        public Command DenyQrCommand
        {
            get;
        }


        private async void OnScanButtonClicked(object sender)
        {
            QrCode = null;
            IsQrVisible = false;
            IsAcceptDenyButtonVisible = false;

            try
            {
                var result = await _scanner.ScanQR();
                if (result == null) 
                    return;

                _merchantToClientRequest = MerchantToClientRequest.FromString(result);
                Amount = $"{_merchantToClientRequest.Amount:N2}";
                IsAcceptDenyButtonVisible = true;
                IsScanButtonVisible = false;
            }
            catch (Exception)
            {
                await Application.Current.MainPage.DisplayAlert("Error","Wrong QR code scanned","Ok");
            }
        }

        public string Amount
        {
            get => _amount;
            set
            {
                if (value == _amount)
                {
                    return;
                }
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public bool IsAcceptDenyButtonVisible
        {
            get => _isAcceptDenyButtonVisible;
            set
            {
                if (value == _isAcceptDenyButtonVisible)
                {
                    return;
                }
                _isAcceptDenyButtonVisible = value;
                OnPropertyChanged(nameof(IsAcceptDenyButtonVisible));
            }
        }

        public bool IsQrVisible
        {
            get => _isQrVisible;
            set
            {
                if (value == _isQrVisible)
                {
                    return;
                }
                _isQrVisible = value;
                OnPropertyChanged(nameof(IsQrVisible));
            }
        }

        public bool IsScanButtonVisible
        {
            get => _isScanButtonVisible;
            set
            {
                if (value == _isScanButtonVisible)
                {
                    return;
                }
                _isScanButtonVisible = value;
                OnPropertyChanged(nameof(IsScanButtonVisible));
            }
        }

        public string QrCode
        {
            get => _qrCode;
            set
            {
                if (_qrCode == value)
                    return;

                _qrCode = value;
                OnPropertyChanged(nameof(QrCode));
            }
        }

        /// <summary>
        /// Reaction on GenerateQR button clicked
        /// </summary>
        /// <param name="obj"></param>
        public async void OnAcceptQrCommand(object obj)
        {

            var answer = await Application.Current.MainPage.DisplayAlert("Confirmation", "Would you like to accept the offer?", "Yes", "No");
            if (!answer) return;
            var settings = _localSettings.LoadSettings();
            var response = new ClientToMerchantResponse
            {
                ClientId = settings.ClientId,
                TimeStampUTC = DateTime.UtcNow,
                MerchantRequest = _merchantToClientRequest,
                EncryptedClientCardData = EncryptCardData(settings, _merchantToClientRequest.MerchantTransactionId)
            };
            var clientSignature = _securityService.Sign(Encoding.UTF8.GetBytes(response.AsDataForSignature()));
            response.ClientSignature = Convert.ToBase64String(clientSignature);

            QrCode = response.AsQRCodeString();
            IsQrVisible = true;
            IsAcceptDenyButtonVisible = false;
            IsScanButtonVisible = true;
        }

        private string EncryptCardData(Settings settings, string transactionId)
        {
           var clientCardData = new ClientCardData
           {
               TransactionId      = transactionId,
               CardNumber         = settings.CardNumber,
               ExpirationDateMMYY = settings.ExpirationDate,
               ValidFrom          = settings.ValidFrom,
               CardHolderName     = settings.CardHolderName,
               CVC                = settings.CVC,
               IssueNo            = settings.IssueNo
           };

           var bytes = _securityService.Encrypt(Encoding.UTF8.GetBytes(clientCardData.AsString()),_securityService.APIServerCertificate);
           return Convert.ToBase64String(bytes);
        }

        public void OnDenyQrCommand(object obj)
        {
            QrCode                    = null;
            IsQrVisible               = false;
            IsAcceptDenyButtonVisible = false;
            IsScanButtonVisible       = true;
            Amount                    = "";
        }

    }
}
