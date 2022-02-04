using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using QRBee.Core;
using QRBee.Core.Data;
using QRBee.Services;
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

            var localSettings = DependencyService.Resolve<ILocalSettings>();
            var settings = localSettings.LoadSettings();

            Name           = settings.Name;
            Email          = settings.Email;
            DateOfBirth    = settings.DateOfBirth;
            CardNumber     = settings.CardNumber;
            ValidFrom      = settings.ValidFrom;
            ExpirationDate = settings.ExpirationDate;
            CardHolderName = settings.CardHolderName;
            CVC            = settings.CVC;
            IssueNo        = settings.IssueNo;
        }

        public Command RegisterCommand
        {
            get;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
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
                Password1Color = (CheckPassword(_password1)) ? Color.Green : Color.Red;
                OnPropertyChanged(nameof(Password1));
                OnPropertyChanged(nameof(Password1Color));
            }
        }

        private static bool CheckPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 7)
            {
                return false;
            }

            return Regex.IsMatch(password, "(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])");
            //return string.IsNullOrWhiteSpace(password) || password.Length<8 && Regex.IsMatch(password, "[a-zA-Z0-9]");
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
        public string Pin { get; set; }

        private async void OnRegisterClicked(object obj)
        {
            using var client = new HttpClient(GetInsecureHandler());
            var localSettings = DependencyService.Resolve<ILocalSettings>();

            var service = new Core.Client.Client(localSettings.QRBeeApiUrl,client);

            try
            {
                //TODO Check if ClientId already in LocalSettings. If Yes update data in database

                //save local settings
                var settings       = new Settings
                {
                    CardHolderName = CardHolderName,
                    CardNumber     = CardNumber,
                    CVC            = CVC,
                    DateOfBirth    = DateOfBirth,
                    Email          = Email,
                    ExpirationDate = ExpirationDate,
                    IsRegistered   = true,
                    IssueNo        = IssueNo,
                    ValidFrom      = ValidFrom,
                    Name           = Name,
                    PIN            = Pin
                };
                await localSettings.SaveSettings(settings);

                await service.RegisterAsync(new RegistrationRequest
                {
                    DateOfBirth        = DateOfBirth.ToString("yyyy-MM-dd"),
                    Email              = Email,
                    Name               = Name,
                    RegisterAsMerchant = false
                });

                var page = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                await page.DisplayAlert("Success", "You have been registered successfully", "Ok");

                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            catch (Exception e)
            {
                //TODO: delete exception message in error message
                var page = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                await page.DisplayAlert("Error", $"The Backend isn't working: {e.Message}", "Ok");
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

    }
}
