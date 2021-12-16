using System;
using Xamarin.Forms;

namespace QRBee.ViewModels
{
    internal class MerchantPageViewModel : BaseViewModel
    {
        private string _name;
        private decimal _amount;
        private string _qrCode;

        public Command GenerateQrCommand { get; }

        public MerchantPageViewModel()
        {
            GenerateQrCommand = new Command(OnGenerateQrClicked);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

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

        public async void OnGenerateQrClicked(object obj)
        {
            QrCode = $"{Name}.{Amount:0.00}.{DateTime.UtcNow:O}";
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            // await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
        }

    }
}