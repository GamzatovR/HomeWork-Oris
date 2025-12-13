using MiniHTTPServer2.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Controlnaya1.Endpoints
{
    [Endpoint]
    internal class HealthEndpoint
    {
        [HttpGet]
        public void Answer(HttpListenerContext cntxt)
        {
            HttpListenerResponse rspns = cntxt.Response;
            rspns.StatusCode = 200;
            using var writer = new StreamWriter(rspns.OutputStream);
            writer.Write("OK");
            writer.Flush();
            rspns.Close();
        }
    }
}
