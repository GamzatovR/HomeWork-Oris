using MiniHTTPServer2.Shared;
using System.Net;

namespace MiniHTTPServer2.Core.Abstract
{
    abstract class Handler
    {
        public Handler Successor { get; set; }
        public abstract Task HandleRequest(HttpListenerContext context, SettingsManager settingsModel);
    }
}
