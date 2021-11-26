using QRBee.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace QRBee.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}