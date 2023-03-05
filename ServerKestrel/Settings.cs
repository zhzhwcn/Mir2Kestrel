using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerKestrel
{
    public class Settings
    {
        private static readonly string SettingsFilePath = "settings.json";

        public IPAddress ListenIp { get; set; } = IPAddress.Loopback;

        public int ListenPort { get; set; } = 7000;

        public static Settings Load()
        {
            var settings = new Settings();
            if (File.Exists(SettingsFilePath))
            {
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsFilePath));
            }

            return settings ?? new Settings();
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(SettingsFilePath, JsonSerializer.Serialize(this));
        }
    }
}
