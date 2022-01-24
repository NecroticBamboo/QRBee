using System.Threading.Tasks;
using Newtonsoft.Json;
using QRBee.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(QRBee.Droid.Services.LocalSettings))]

namespace QRBee.Droid.Services
{
    internal class LocalSettings : ILocalSettings
    {
        public string QRBeeApiUrl => "https://localhost:5000";

        public async Task SaveSettings(Settings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            Application.Current.Properties["Settings"] = json;
            await Application.Current.SavePropertiesAsync();
        }

        public Task<Settings> LoadSettings()
        {
            if (!Application.Current.Properties.ContainsKey("Settings"))
                return Task.FromResult(new Settings());

            var json = Application.Current.Properties["Settings"].ToString();
            var settings = JsonConvert.DeserializeObject<Settings>(json);
            return Task.FromResult(settings);
        }
    }
}