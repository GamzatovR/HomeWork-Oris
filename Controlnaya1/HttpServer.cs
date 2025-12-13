using Controlnaya1.Invoices;
using Controlnaya1.Shared;
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
            ProcessingEndpoint();
            var invoiceService = new InvoiceServise(settingsModel.Settings.ConnectionString, settingsModel.Settings.ProcessingIntervalSecond, settingsModel.Settings.MaxErrorRetries);
        }
        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Сервер остановлен и закрыт!");
        }
        public void ProcessingEndpoint()
        {
            HttpListenerContext cntxt = _listener.GetContext();
            EndpointsHandler ndpthndlr = new EndpointsHandler();
            ndpthndlr.HandleRequest(cntxt, settingsModel);
        }
    }
}
