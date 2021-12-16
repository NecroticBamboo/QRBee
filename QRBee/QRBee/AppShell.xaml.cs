using QRBee.ViewModels;
using QRBee.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace QRBee
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage),typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        }

    }
}