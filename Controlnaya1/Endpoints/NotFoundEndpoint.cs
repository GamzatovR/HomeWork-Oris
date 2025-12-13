using MiniHTTPServer2.Core.Attributes;
using System.Net;

namespace MiniHTTPServer2.Endpoints
{
    [Endpoint]
    internal class NotFoundEndpoint
    {
        [HttpGet]
        public void NotStranicy(HttpListenerContext context)
        {
            string html = File.ReadAllText("C:/Users/gamza/source/repos/MiniHTTPServer2/MiniHTTPServer2/Static/Html при 404/Index.html");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "text/html, charset=utf-8";
            using var writer = new StreamWriter(context.Response.OutputStream);
            writer.Write(html);
            writer.Flush();
            context.Response.Close();
        }
        [HttpGet]
        public void NotMethod(HttpListenerContext context)
        {
            string html = File.ReadAllText("C:/Users/gamza/source/repos/MiniHTTPServer2/MiniHTTPServer2/Static/Html при 501/Index.html");
            context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            context.Response.ContentType = "text/html, charset=utf-8";
            using var writer = new StreamWriter(context.Response.OutputStream);
            writer.Write(html);
            writer.Flush();
            context.Response.Close();
        }
    }
}