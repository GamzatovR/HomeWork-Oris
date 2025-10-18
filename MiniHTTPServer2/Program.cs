using MiniHTTPServer2.Shared;

Start();
static void Start()
{
    var settingsModel = SettingsModel.GetInstance();
    if (settingsModel == null)
    {
        Console.WriteLine("Ошибка: Проблема с моделью настройки или отсутствует index.html");
        return;
    }

    var httpServer = new HttpServer(settingsModel);

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
