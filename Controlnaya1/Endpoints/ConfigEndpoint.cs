using Controlnaya1.Shared;
using MiniHTTPServer2.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Controlnaya1.Endpoints
{
    [Endpoint]
    internal class ConfigEndpoint
    {
        [HttpGet]
        public void ReturnConfiguration(HttpListenerContext cntxt)
        {
            var settingPath = @".\config.json";
            HttpListenerResponse rspns = cntxt.Response;
            string config = File.ReadAllText(settingPath);
            using var writer = new StreamWriter(rspns.OutputStream);
            writer.Write(config);
            writer.Flush();
            rspns.Close();
        }
        [HttpPost]
        public void Reload(HttpListenerContext cntxt)
        {
            HttpListenerResponse rspns = cntxt.Response;
            rspns.StatusCode = 200;
            var forConfig = SettingsManager.Instance;
            forConfig.RebotConfiguration();
            using var writer = new StreamWriter(rspns.OutputStream);
            writer.Write("reloaded");
            writer.Flush();
            rspns.Close();
        }
    }
}
