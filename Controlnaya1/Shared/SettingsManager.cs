using Controlnaya1;
using Controlnaya1.Invoices;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Controlnaya1.Shared
{
    public sealed class SettingsManager
    {
        private static SettingsManager _instance;

        private FileSystemWatcher fSW;

        public AppSettings Settings { get; set; }

        private static readonly object _lock = new object();

        private SettingsManager()
        {
            LoadSettings();
        }
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock(_lock)
                    {
                        if (_instance == null)
                            _instance = new SettingsManager();
                    }
                }
                return _instance;
            }
        }

        private void LoadSettings()
        {
            var settingPath = @".\config.json";

            Settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingPath))! ?? throw new InvalidOperationException($"Не удалось десериализовать настройки!");
            
            fSW = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(settingPath),
                Filter = "config.json",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            fSW.Changed += async (sender, e) =>
            {
                await Task.Delay(300);
                RebotConfiguration();
                Console.WriteLine("Конфигурация обновлена");
            };
        }
        public void RebotConfiguration()
        {
            var settingPath = @".\config.json";
            Settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingPath))! ?? throw new InvalidOperationException($"Не удалось десериализовать настройки!");
        }
    }
    public class AppSettings
    {
        public string Domain { get; set; }
        public string Port { get; set; }
        public string ConnectionString { get; }
        public int ProcessingIntervalSecond { get; }
        public int MaxErrorRetries { get; }
    }
}