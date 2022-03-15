using QRBee.ViewModels;
using Xamarin.Forms.Xaml;

namespace QRBee.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MerchantPage
    {
        public MerchantPage()
        {
            BindingContext = App.GetViewModel<MerchantPageViewModel>();
            InitializeComponent();
        }
    }
}