using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MiniHTTPServer2.Shared
{
    public sealed class SettingsManager
    {
        private static SettingsManager _instance;
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
            var settingPath = @".\settings.json";

            Settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingPath))! ?? throw new InvalidOperationException($"Не удалось десериализовать настройки!");
            
            if (!File.Exists(Settings.StaticDirectoryPath))
            {
                throw new FileNotFoundException("Отсутствует файл по пути", settingPath);
            }
            
            var validationContext = new ValidationContext(Settings);

            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(Settings, validationContext, validationResults, true))
            {
                foreach(ValidationResult result in validationResults)
                {

                    if (result.ErrorMessage != string.Empty)
                        throw new ArgumentException(result.ErrorMessage);
                }
            }
        }
    }
    public class AppSettings
    {
        public string StaticDirectoryPath { get; set; }
        [RegularExpression(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$", ErrorMessage = "Домен указан неверно")]
        public string Domain { get; set; }
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Порт не указан")]
        public string Port { get; set; }
    }
}