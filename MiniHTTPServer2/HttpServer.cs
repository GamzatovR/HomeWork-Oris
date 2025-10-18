using System.Net;
using System.Text;

namespace MiniHTTPServer2.Shared
{
    public class HttpServer(SettingsModel settingsModel)
    {
        private HttpListener _listener = new HttpListener();
        public bool IsStop { get; set; } = false;

        public void ServerStart()
        {
            _listener.Prefixes.Add($"http://{settingsModel.Domain}:{settingsModel.Port}/");
            _listener.Start();
            Console.WriteLine("Сервер запущен, \nОжидаем запрос");
            Receive();
        }
        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Сервер остановлен и закрыт!");
        }
        private async Task Receive()
        {
            while (true)
            {
                var context = await _listener.GetContextAsync();
                Console.WriteLine("Запрос получен!");

                var otvet = context.Response;
                otvet.ContentType = "text/html";

                var otvettext = File.ReadAllText(settingsModel.StaticDirectoryPath);
                byte[] buffer = Encoding.UTF8.GetBytes(otvettext);
                otvet.ContentLength64 = buffer.Length;
                using Stream output = otvet.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();
                Console.WriteLine("Запрос обработан");
                otvet.Close();
            }
        }
    }
}
