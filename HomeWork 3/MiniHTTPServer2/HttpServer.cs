using System.IO;
using System.Net;
using System.Text;

namespace MiniHTTPServer2.Shared
{
    public class HttpServer(SettingsManager settingsModel)
    {
        private HttpListener _listener = new HttpListener();
        public bool IsStop { get; set; } = false;
        private readonly static Dictionary<string, string> _ContentTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            //Текстовые
            { ".txt", "text/plain" },
            { ".csv", "text/csv" },
            { ".css", "text/css" },
            { ".html", "text/html" },
            { ".htm", "text/html" },
            { ".xml", "text/xml" },
            { ".json", "application/json" },
            { ".js", "application/javascript" },
            { ".ts", "application/typescript" },

            //Изображения
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".svg", "image/svg+xml" },
            { ".webp", "image/webp" },
            { ".ico", "image/x-icon" },
            { ".tif", "image/tiff" },
            { ".tiff", "image/tiff" },
            { ".avif", "image/avif" },

            //Аудио
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".ogg", "audio/ogg" },
            { ".m4a", "audio/mp4" },
            { ".aac", "audio/aac" },
            { ".flac", "audio/flac" },
            { ".mid", "audio/midi" },
            { ".midi", "audio/midi" },

            //Видео
            { ".mp4", "video/mp4" },
            { ".mkv", "video/x-matroska" },
            { ".webm", "video/webm" },
            { ".avi", "video/x-msvideo" },
            { ".mov", "video/quicktime" },
            { ".wmv", "video/x-ms-wmv" },
            { ".flv", "video/x-flv" },
            { ".3gp", "video/3gpp" },

            //Архивы
            { ".zip", "application/zip" },
            { ".rar", "application/vnd.rar" },
            { ".tar", "application/x-tar" },
            { ".gz", "application/gzip" },
            { ".7z", "application/x-7z-compressed" },

            //Документы
            { ".pdf", "application/pdf" },
            { ".doc", "application/msword" },
            { ".dot", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".odt", "application/vnd.oasis.opendocument.text" },
            { ".ods", "application/vnd.oasis.opendocument.spreadsheet" },

            // ----- Прочие -----
            { ".exe", "application/octet-stream" },
            { ".bin", "application/octet-stream" },
            { ".dll", "application/octet-stream" },
            { ".apk", "application/vnd.android.package-archive" },
            { ".iso", "application/x-iso9660-image" },
            { ".swf", "application/x-shockwave-flash" },
            { ".rtf", "application/rtf" },
            { ".yaml", "application/x-yaml" },
            { ".yml", "application/x-yaml" },

            // ----- Для серверов и API -----
            { ".wasm", "application/wasm" },
            { ".manifest", "text/cache-manifest" },
            { ".map", "application/json" },
        };


        public void ServerStart()
        {
            _listener.Prefixes.Add($"http://{settingsModel.Settings.Domain}:{settingsModel.Settings.Port}/");
            _listener.Start();
            Console.WriteLine("Сервер запущен, \nОжидаем запрос");
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

                    await ControlOtveta(context);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
                    break;
                }
            }
        }
        private async Task ControlOtveta(HttpListenerContext context)
        {
            var otvet = context.Response;
            var put = context.Request.Url.AbsolutePath;
            if (put == "/")
                put = settingsModel.Settings.StaticDirectoryPath;
            else
                put = "." + put;
            try
            {
                var fileInfo = new FileInfo(put);
                otvet.ContentType = _ContentTypes[fileInfo.Extension];
                await using(var fileStream = new FileStream(put, FileMode.Open, FileAccess.Read, FileShare.Read,4096, useAsync: true))
                {
                    otvet.ContentLength64 = fileStream.Length;
                    await fileStream.CopyToAsync(otvet.OutputStream);
                }
                Console.WriteLine("Запрос обработан");
            }
            catch(FileNotFoundException)
            {
                Console.WriteLine($"Ошибка, отсутствует файл по указанном пути: {put}");
                otvet.StatusCode = 404;
            }
            finally
            {
                otvet.Close();
            }
        }
    }
}
