using QRBee.Views;
using System;
using System.Collections.Generic;
using System.Text;
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
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
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