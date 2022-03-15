using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using QRBee.Services;
using QRBee.Views;
using Xamarin.Forms;

namespace QRBee.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private bool _isPinVisible;
        public Command LoginCommand
        {
            get;
        }
        public Command RegisterCommand
        {
            get;
        }
        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            RegisterCommand = new Command(OnRegisterClicked);
            
        }
        public string PinCode { get; }

        /// <summary>
        /// Reaction on Login button
        /// </summary>
        /// <param name="obj"></param>
        private async void OnLoginClicked(object obj)
        {
            var isFingerprintAvailable = await CrossFingerprint.Current.IsAvailableAsync(false);
            if (!isFingerprintAvailable)
            {
                //Insert PIN
                IsPinVisible = true;
                var localSettings = DependencyService.Resolve<ILocalSettings>();
                var pin = localSettings.LoadSettings().PIN;

                if (!string.IsNullOrEmpty(pin) && pin.Equals(PinCode))
                {
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"{nameof(RegisterPage)}");
                }

            }

            var conf = new AuthenticationRequestConfiguration("Authentication", "Authenticate access to your personal data");

            var authResult = await CrossFingerprint.Current.AuthenticateAsync(conf);
            if (authResult.Authenticated)
            {
                //Success  
                // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                //Failure
                return;
            }
        }

        public bool IsPinVisible
        {
            get => _isPinVisible;
            set
            {
                if (value == _isPinVisible)
                {
                    return;
                }
                _isPinVisible = value;
                OnPropertyChanged(nameof(IsPinVisible));
            }
        }

        /// <summary>
        /// Reaction on Register button
        /// </summary>
        /// <param name="obj"></param>
        private async void OnRegisterClicked(object obj)
        {
            await Shell.Current.GoToAsync($"{nameof(RegisterPage)}");
        }
    }
}