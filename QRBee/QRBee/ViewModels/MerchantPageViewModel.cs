using QRBee.Core.Data;
using QRBee.Core.Security;
using QRBee.Services;
using System;
using System.Net.Http;
using System.Text;
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

        private MerchantToClientRequest _lastRequest;

        public Command GenerateQrCommand { get; }
        public Command ScanCommand{ get; }

        public MerchantPageViewModel(IQRScanner scanner, ILocalSettings settings, ISecurityService securityService)
        {
            _scanner          = scanner;
            _settings         = settings;
            _securityService  = securityService;
            ScanCommand       = new Command(OnScanButtonClicked);
            GenerateQrCommand = new Command(OnGenerateQrClicked);
            var localSettings = DependencyService.Resolve<ILocalSettings>();
            Name              = localSettings.LoadSettings().Name;
        }

        private async void OnScanButtonClicked(object sender)
        {
            try
            {
                var result          = await _scanner.ScanQR();
                if (string.IsNullOrWhiteSpace(result)) 
                    return;

                var clientResponse  = ClientToMerchantResponse.FromString(result, _lastRequest);

                if (string.IsNullOrWhiteSpace(clientResponse.ClientSignature))
                    throw new ApplicationException("Request is not signed by a client");
                if (string.IsNullOrWhiteSpace(clientResponse.EncryptedClientCardData))
                    throw new ApplicationException("Request does not contain client's card data");

                var paymentRequest  = new PaymentRequest
                {
                    ClientResponse  = clientResponse
                };

                //QrCode = null;
                IsVisible = false;

                // ------------------------------------- SEND PAYMENT REQUEST ------------------------------------------
                // 
                //                             ____   _ __   ____  __ _____ _   _ _____ 
                //                            |  _ \ / \\ \ / /  \/  | ____| \ | |_   _|
                //                            | |_) / _ \\ V /| |\/| |  _| |  \| | | |  
                //                            |  __/ ___ \| | | |  | | |___| |\  | | |  
                //                            |_| /_/   \_\_| |_|  |_|_____|_| \_| |_|  
                //                                           
                //
                var apiService = new Core.Client.Client(_settings.QRBeeApiUrl, new HttpClient(GetInsecureHandler()));
                var response   = await apiService.PayAsync(paymentRequest);
                //
                // -----------------------------------------------------------------------------------------------------

                if (response.Success)
                {
                    var check = _securityService.Verify(
                        Encoding.UTF8.GetBytes(response.AsDataForSignature()),
                        Convert.FromBase64String(response.ServerSignature),
                        _securityService.APIServerCertificate
                        );

                    if (check)
                    {
                        var paymentConfirmation = new PaymentConfirmation
                        {
                            MerchantId = _settings.LoadSettings().ClientId,
                            MerchantTransactionId = response.PaymentRequest.ClientResponse.MerchantRequest.MerchantTransactionId,
                            GatewayTransactionId = response.GatewayTransactionId
                        };

                        await apiService.ConfirmPayAsync(paymentConfirmation);
                        await Application.Current.MainPage.DisplayAlert("Success", "The transaction completed successfully ", "Ok");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Failure", "Invalid server signature", "Ok");
                    }
                    
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Failure", $"The transaction failed: {response.RejectReason}", "Ok");
                }
                
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
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (cert.Issuer.Equals("CN=localhost"))
                        return true;
                    return errors == System.Net.Security.SslPolicyErrors.None;
                }
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
                var request = new MerchantToClientRequest
                {
                    MerchantId            = _settings.LoadSettings().ClientId,
                    MerchantTransactionId = Guid.NewGuid().ToString("D"),
                    Name                  = Name,
                    Amount                = Amount,
                    TimeStampUTC          = DateTime.UtcNow
                };

                var merchantSignature = _securityService.Sign(Encoding.UTF8.GetBytes(request.AsDataForSignature()));
                request.MerchantSignature = Convert.ToBase64String(merchantSignature);

                _lastRequest = request;

                QrCode = request.AsQRCodeString();
                IsVisible = true;
            }

        }

    }
}