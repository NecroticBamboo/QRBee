using System;
using System.Linq;
using QRBee.Core.Data;
using QRBee.Services;
using Xamarin.Forms;

namespace QRBee.ViewModels
{
    internal class MerchantPageViewModel : BaseViewModel
    {
        private bool _isVisible;
        private decimal _amount;
        private string _qrCode;

        public Command GenerateQrCommand { get; }
        public Command ScanCommand{ get; }

        public MerchantPageViewModel()
        {
            ScanCommand = new Command(OnScanButtonClicked);
            GenerateQrCommand = new Command(OnGenerateQrClicked);
            var localSettings = DependencyService.Resolve<ILocalSettings>();
            Name = localSettings.LoadSettings().Name;
        }

        private async void OnScanButtonClicked(object sender)
        {
            try
            {
                var scanner = DependencyService.Get<IQRScanner>();
                var result = await scanner.ScanQR();
                if (result != null)
                {

                }
            }
            catch (Exception ex)
            {
                throw;
            }
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
                    TransactionId = Guid.NewGuid().ToString("D"),
                    Name = Name,
                    Amount = Amount,
                    TimeStampUTC = DateTime.UtcNow
                };
                // TODO Create merchant signature.
                QrCode = trans.AsString();
                IsVisible = true;
            }

        }

    }
}