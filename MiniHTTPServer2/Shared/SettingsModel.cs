using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MiniHTTPServer2.Shared
{
    public class SettingsModel
    {
        public string StaticDirectoryPath { get; set; }
        [RegularExpression(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$", ErrorMessage = "Домен указан неверно")]
        public string Domain { get; set; }
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Порт не указан")]
        public string Port { get; set; }
        public static SettingsModel settingsModel;

        public static SettingsModel GetInstance()
        {
            var settingPath = @".\settings.json";
            
            try
            {
                settingsModel = JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingPath))!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отсутствует файл по пути: {settingPath}.\n Error: {ex.Message}");
                return null;
            }
            
            var validationContext = new ValidationContext(settingsModel);

            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(settingsModel, validationContext, validationResults, true))
            {
                foreach(ValidationResult result in validationResults)
                {
                    Console.WriteLine($"Ошибка: {result.ErrorMessage}");
                }
                return null;
            }
            if (!File.Exists(settingsModel.StaticDirectoryPath))
            {
                Console.WriteLine("Ошибка: отсутствует страница index.html");
                return null;
            }

            return settingsModel;
        }
    }
}