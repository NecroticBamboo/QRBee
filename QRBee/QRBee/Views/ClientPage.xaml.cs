using System;
using QRBee.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRBee.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClientPage : ContentPage
    {
        public ClientPage()
        {
            InitializeComponent();
        }

        private async void OnScanButtonClicked(object sender, EventArgs args)
        {
            try
            {
                var scanner = DependencyService.Get<IQRScanner>();
                var result = await scanner.ScanQR();
                if (result != null)
                {
                    Name.Text = result;
                    Amount.Text = result;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}