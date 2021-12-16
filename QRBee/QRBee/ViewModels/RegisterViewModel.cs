using System;
using System.Collections.Generic;
using System.Text;
using QRBee.Views;
using Xamarin.Forms;

namespace QRBee.ViewModels
{
    internal class RegisterViewModel: BaseViewModel
    {
        private string _password1;
        private string _password2;
        public RegisterViewModel()
        {
            RegisterCommand = new Command(OnRegisterClicked);
        }

        public Command RegisterCommand
        {
            get;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public string CardNumber { get; set; }
        public string ValidFrom { get; set; }
        public string ExpirationDate { get; set; }
        public string CardHolderName { get; set; }
        public string CVC { get; set; }
        public string IssueNo { get; set; }

        public Color Password1Color { get; set; }
        public Color Password2Color { get; set;}

        public string Password1
        {
            get => _password1;
            set
            {
                if ( value == _password1)
                    return;

                _password1 = value;
                Password1Color = (string.IsNullOrWhiteSpace(_password1) || _password1.Length < 8 ) ? Color.Red : Color.Green;
                OnPropertyChanged(nameof(Password1));
                OnPropertyChanged(nameof(Password1Color));
            }
        }

        public string Password2
        {
            get => _password2;
            set
            {
                if(value == _password2)
                    return;

                _password2 = value;
                Password2Color = (string.IsNullOrWhiteSpace(_password2) || _password2!=Password1) ? Color.Red : Color.Green;
                OnPropertyChanged(nameof(Password2));
                OnPropertyChanged(nameof(Password2Color));
            }
        }



        private async void OnRegisterClicked(object obj)
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }

    }
}
