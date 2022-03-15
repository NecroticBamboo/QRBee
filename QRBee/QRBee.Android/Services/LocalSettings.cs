﻿using Newtonsoft.Json;
using QRBee.Services;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(QRBee.Droid.Services.LocalSettings))]

namespace QRBee.Droid.Services
{
    internal class LocalSettings : ILocalSettings
    {
        public string QRBeeApiUrl => "https://10.0.2.2:7000";

        public async Task SaveSettings(Settings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            Application.Current.Properties["Settings"] = json;
            await Application.Current.SavePropertiesAsync();
        }

        public Settings LoadSettings()
        {
            if (!Application.Current.Properties.ContainsKey("Settings"))
                return new Settings();

            var json = Application.Current.Properties["Settings"].ToString();
            var settings = JsonConvert.DeserializeObject<Settings>(json);

            return settings;
        }
    }
}