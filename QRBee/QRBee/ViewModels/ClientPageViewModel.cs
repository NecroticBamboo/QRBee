using System;
using QRBee.Core.Data;
using QRBee.Services;
using QRBee.Views;
using Xamarin.Forms;

namespace QRBee.ViewModels
{
    internal class ClientPageViewModel: BaseViewModel
    {
        public bool _isVisible;
        public string _amount;
        private string _qrCode;
        private MerchantToClientRequest _merchantToClientRequest;
        private readonly ClientPage _clientPage;

        public ClientPageViewModel(Views.ClientPage clientPage)
        {
            ScanCommand = new Command(OnScanButtonClicked);
            GenerateQrCommand = new Command(OnGenerateQrClicked);
            _clientPage = clientPage;
        }

        public Command ScanCommand
        {
            get;
        }

        public Command GenerateQrCommand 
        { 
            get; 
        }

        private async void OnScanButtonClicked(object sender)
        {
            try
            {
                var scanner = DependencyService.Get<IQRScanner>();
                var result = await scanner.ScanQR();
                if (result != null)
                {
                    _merchantToClientRequest = MerchantToClientRequest.FromString(result);
                    Amount = $"{_merchantToClientRequest.Amount:N2}";
                    IsVisible = true;
                }
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
        /// Reaction on GenerateQR button clicked
        /// </summary>
        /// <param name="obj"></param>
        public async void OnGenerateQrClicked(object obj)
        {

            bool answer = await _clientPage.DisplayAlert("Confirmation", "Would you like to accept the offer?", "Yes", "No");
            if (answer)
            {
                var response = new ClientToMerchantResponse
                {
                    ClientId = Guid.NewGuid().ToString("D"),
                    TimeStampUTC = DateTime.UtcNow,
                    Request = _merchantToClientRequest
                    
                };
                // TODO Create merchant signature.
                QrCode = response.AsString();
            }

            
        }

    }
}
