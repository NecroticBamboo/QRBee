using System.Threading.Tasks;
using QRBee.Services;
using Xamarin.Forms;
using ZXing.Mobile;


[assembly: Dependency(typeof(QRBee.Droid.Services.QRScannerService))]

namespace QRBee.Droid.Services
{
    public class QRScannerService : IQRScanner
    {
        public async Task<string> ScanQR()
        {

            var optionsCustom = new MobileBarcodeScanningOptions();

            var scanner = new MobileBarcodeScanner()
            {
                TopText = "Scan the QR Code",
                BottomText = "Please Wait",
            };

            var scanResult = await scanner.Scan(optionsCustom);

            return scanResult != null ? scanResult.Text : "Nothing was scanned. Please try again.";
        }
    }
}