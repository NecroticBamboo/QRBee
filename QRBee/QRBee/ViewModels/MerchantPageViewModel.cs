using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using QRBee.Core.Data;
using QRBee.Core.Security;
using QRBee.Services;
using Xamarin.Forms;

namespace QRBee.ViewModels
{
    internal class MerchantPageViewModel : BaseViewModel
    {
        private readonly IQRScanner _scanner;
        private readonly ILocalSettings _settings;
        private readonly ISecurityService _securityService;
        private bool _isVisible;
        private decimal _amount;
        private string _qrCode;

        public Command GenerateQrCommand { get; }
        public Command ScanCommand{ get; }

        public MerchantPageViewModel(IQRScanner scanner, ILocalSettings settings, ISecurityService securityService)
        {
            _scanner = scanner;
            _settings = settings;
            _securityService = securityService;
            ScanCommand = new Command(OnScanButtonClicked);
            GenerateQrCommand = new Command(OnGenerateQrClicked);
            var localSettings = DependencyService.Resolve<ILocalSettings>();
            Name = localSettings.LoadSettings().Name;
        }

        private async void OnScanButtonClicked(object sender)
        {
            try
            {
                var result = await _scanner.ScanQR();
                if (result == null) 
                    return;

                var client = new HttpClient(GetInsecureHandler());

                var service = new Core.Client.Client(_settings.QRBeeApiUrl, client);
                var paymentRequest = PaymentRequest.FromString(result);

                //QrCode = null;
                IsVisible = false;

                var response = await service.PayAsync(paymentRequest);

                //TODO handle response
                await Application.Current.MainPage.DisplayAlert("Success", "The transaction completed successfully ", "Ok");
            }
            catch (Exception e)
            {
                //TODO: delete exception message in error message
                await Application.Current.MainPage.DisplayAlert("Error", $"The Backend isn't working: {e.Message}", "Ok");
            }
        }

        // This method must be in a class in a platform project, even if
        // the HttpClient object is constructed in a shared project.
        public HttpClientHandler GetInsecureHandler()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            return handler;
        }

        public string Name { get; }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount == value)
                    return;

                _amount = value;

                OnPropertyChanged(nameof(Amount));
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value == _isVisible)
                {
                    return;
                }
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public string QrCode
        {
            get => _qrCode;
            set
            {
                // _qrCode = $"{Amount}/{Name}";
                if (_qrCode == value)
                    return;

                _qrCode = value;
                OnPropertyChanged(nameof(QrCode));
            }
        }

        /// <summary>
        /// Reaction on Generate QR code button
        /// </summary>
        /// <param name="obj"></param>
        public async void OnGenerateQrClicked(object obj)
        {
            if (string.IsNullOrWhiteSpace(Name) || Amount==0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "The fields must be filled", "Ok");
            }
            else
            {
                var trans = new MerchantToClientRequest
                {
                    MerchantId = _settings.LoadSettings().ClientId,
                    MerchantTransactionId = Guid.NewGuid().ToString("D"),
                    Name = Name,
                    Amount = Amount,
                    TimeStampUTC = DateTime.UtcNow
                };

                var merchantSignature = _securityService.Sign(Encoding.UTF8.GetBytes(trans.AsDataForSignature()));
                trans.MerchantSignature = Convert.ToBase64String(merchantSignature);

                QrCode = trans.AsQRCodeString();
                IsVisible = true;
            }

        }

    }
}