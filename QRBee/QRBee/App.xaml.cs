using Microsoft.Extensions.DependencyInjection;
using QRBee.Services;
using QRBee.ViewModels;
using System;
using Xamarin.Forms;

namespace QRBee
{
    public partial class App : Application
    {

        public App(Action<IServiceCollection> addPlatformServices = null)
        {
            InitializeComponent();

            SetupServices(addPlatformServices);

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }
        protected static IServiceProvider ServiceProvider { get; set; }

        private void SetupServices(Action<IServiceCollection> addPlatformServices = null)
        {
            var services = new ServiceCollection();

            // Add platform specific services
            addPlatformServices?.Invoke(services);

            // TODO: Add core services here
            

            // Add ViewModels
            services
                .AddTransient<MerchantPageViewModel>()
                .AddTransient<ClientPageViewModel>()
                .AddTransient<RegisterViewModel>()
                .AddTransient<LoginViewModel>()
                ;


            ServiceProvider = services.BuildServiceProvider();
        }

        public static BaseViewModel GetViewModel<TViewModel>() where TViewModel : BaseViewModel => ServiceProvider.GetService<TViewModel>();

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
