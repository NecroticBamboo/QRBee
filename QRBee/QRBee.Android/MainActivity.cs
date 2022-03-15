using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using AndroidX.Core.App;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Fingerprint;
using QRBee.Core.Security;
using QRBee.Droid.Services;
using QRBee.Services;
using System;

namespace QRBee.Droid
{
    [Activity(Label = "QRBee", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            CrossFingerprint.SetCurrentActivityResolver(()=>Xamarin.Essentials.Platform.CurrentActivity);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App(AddServices));
            ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != (int) Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] {Manifest.Permission.Camera}, 0);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private static void AddServices(IServiceCollection services)
        {
            services
                .AddSingleton<ISecurityService,AndroidSecurityService>()
                .AddSingleton<ILocalSettings, LocalSettings>()
                .AddSingleton<IQRScanner, QRScannerService>()
                .AddSingleton<IPrivateKeyHandler, AndroidPrivateKeyHandler>()
                ;
        }

    }
}