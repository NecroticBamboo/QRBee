using System;
using QRBee.Services;
using QRBee.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRBee.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClientPage : ContentPage
    {
        public ClientPage()
        {
            BindingContext = App.GetViewModel<ClientPageViewModel>();
            InitializeComponent();
        }
    }
}