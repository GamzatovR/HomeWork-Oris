using MiniHTTPServer2.Shared;

Start();
static void Start()
{

    var httpServer = new HttpServer(SettingsManager.Instance);

    httpServer.ServerStart();
    var stopServer = string.Empty;
    while (!httpServer.IsStop)
    {
        stopServer = Console.ReadLine();
        if (stopServer == "stop")
        {
            httpServer.IsStop = true;
        }
    }
    httpServer.Stop();
}
