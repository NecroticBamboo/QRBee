using QRBee.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRBee.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginViewModel();
        }
    }
}