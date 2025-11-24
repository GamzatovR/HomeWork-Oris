using MiniHTTPServer2.Core.Abstract;
using MiniHTTPServer2.Core.Handlers;
using System.IO;
using System.Net;
using System.Text;

namespace MiniHTTPServer2.Shared
{
    public class HttpServer(SettingsManager settingsModel)
    {
        private HttpListener _listener = new HttpListener();
        public bool IsStop { get; set; } = false;

        public void ServerStart()
        {
            _listener.Prefixes.Add($"http://{settingsModel.Settings.Domain}:{settingsModel.Settings.Port}/");
            _listener.Start();
            _listener.Prefixes.ToList().ForEach(url => Console.WriteLine($"Сервер запущен, \nОжидаем запрос\n Введи: {url}"));
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
                try
                {
                    var context = await _listener.GetContextAsync();
                     
                    Console.WriteLine("Запрос получен!");

                    await ControlOtveta(context, settingsModel);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}, пожалуйста измените ваш url\n и перейдите на существующую страницу");
                    ServerStart();
                }
            }
        }
        private async Task ControlOtveta(HttpListenerContext context, SettingsManager settingsModel)
        {
            Handler staticFilesHandler = new StaticFilesHandler();
            Handler endpointsHandler = new EndpointsHandler();
            staticFilesHandler.Successor = endpointsHandler;
            await staticFilesHandler.HandleRequest(context, settingsModel);
        }
    }
}
