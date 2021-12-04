using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using QRBee.Views;
using Xamarin.Forms;
using ZXing;
using ZXing.Common;

namespace QRBee.ViewModels
{
    internal class MerchantPageViewModel : BaseViewModel
    {
        private string _name;
        private double _amount;
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
                if(_name==value) 
                    return;
                _name= value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public double Amount
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
            QrCode = $"{Name}.{Amount}.{DateTime.Now}";
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            // await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
        }

    }
}
