using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QRBee.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRBee.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = App.GetViewModel<RegisterViewModel>();
        }
    }
}