using Controlnaya1.Shared;
using MiniHTTPServer2.Core.Abstract;
using MiniHTTPServer2.Core.Attributes;
using MiniHTTPServer2.Endpoints;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace MiniHTTPServer2.Core.Handlers
{
    internal class EndpointsHandler : Handler
    {
        public override async Task HandleRequest(HttpListenerContext context, SettingsManager settingsModel)
        {
            // некоторая обработка запроса

            if (true)
            {
                // завершение выполнения запроса;
                var request = context.Request;
                var endpointName = request.Url?.AbsolutePath.Split('/')[1];

                var assembly = Assembly.GetExecutingAssembly();
                var endpoint = assembly.GetTypes()
                                            .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                                            .FirstOrDefault(end => isCheckedNameEndpoint(end.Name, endpointName));

                if (endpoint == null)
                {
                    endpoint = typeof(NotFoundEndpoint);
                }

                var method = endpoint!.GetMethods().Where(t => t.GetCustomAttributes(true)
                .Any(attr =>  attr.GetType().Name.Equals($"Http{context.Request.HttpMethod}", StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();

                if (method == null)
                {
                    method = typeof(NotFoundEndpoint).GetMethod("NotMethod");
                }
                
                
                    method.Invoke(Activator.CreateInstance(endpoint), new[] { context });             
            }
            // передача запроса дальше по цепи при наличии в ней обработчиков
            else if (Successor != null)
            {
                Successor.HandleRequest(context, settingsModel);
            }
        }
        private bool isCheckedNameEndpoint(string endpointName, string className) =>
            endpointName.Equals(className, StringComparison.OrdinalIgnoreCase) ||
            endpointName.Equals($"{className}Endpoint", StringComparison.OrdinalIgnoreCase);
    }
}
