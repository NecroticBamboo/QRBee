using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QRBee.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRBee.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnButtonClicked(object sender, EventArgs args)
        {
            try
            {
                var scanner = DependencyService.Get<IQRScanner>();
                var result = await scanner.ScanQR();
                if (result != null)
                {
                    QrCodeScanner.Text = result;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}