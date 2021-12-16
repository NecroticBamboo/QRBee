using Xamarin.Forms.Xaml;
using QRBee.ViewModels;

namespace QRBee.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MerchantPage
    {
        public MerchantPage()
        {
            BindingContext = new MerchantPageViewModel();
            InitializeComponent();
        }
    }
}