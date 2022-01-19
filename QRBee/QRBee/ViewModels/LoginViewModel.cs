using QRBee.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using Xamarin.Forms;

namespace QRBee.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
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

        /// <summary>
        /// Reaction on Login button
        /// </summary>
        /// <param name="obj"></param>
        private async void OnLoginClicked(object obj)
        {
            bool isFingerprintAvailable = await CrossFingerprint.Current.IsAvailableAsync(false);
            if (!isFingerprintAvailable)
            {
                //Insert PIN
                return;
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