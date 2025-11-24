using MiniHTTPServer2.Core.Abstract;
using MiniHTTPServer2.Shared;
using System.Net;
using System.Text.RegularExpressions;


namespace MiniHTTPServer2.Core.Handlers
{
    internal class StaticFilesHandler : Handler
    {
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
        public override async Task HandleRequest(HttpListenerContext context, SettingsManager settingsModel)
        {
            // некоторая обработка запроса
            var cntxtReq = context.Request;
            var isGetMethod = cntxtReq.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
            var isStaticFile = cntxtReq.Url.AbsolutePath.Split('/').Any(x => x.Contains("."));

            if (isGetMethod && isStaticFile)
            {
                // завершение выполнения запроса;

                var otvet = context.Response;

                var put = cntxtReq.Url.AbsolutePath;

                put = put.TrimStart('/');
                    
                try
                {
                    var fileInfo = ResolveFileRecursive(put);

                    otvet.ContentType = _ContentTypes[fileInfo.Extension];
                    await using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                    {
                        await fileStream.CopyToAsync(otvet.OutputStream);
                    }
                    Console.WriteLine("Запрос обработан");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"Ошибка, отсутствует файл по указанном пути: {put}");
                    otvet.StatusCode = 404;
                }
                finally
                {
                    otvet.Close();
                }
            }
            // передача запроса дальше по цепи при наличии в ней обработчиков
            else if (Successor != null)
            {
                await Successor.HandleRequest(context, settingsModel);
            }
        }
        private FileInfo ResolveFileRecursive(string fileName)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));

            var result = Directory.EnumerateFiles(projectRoot, "*.*", SearchOption.AllDirectories)
                                 .First(f => Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (result == null)
                throw new FileNotFoundException($"Файл '{fileName}' не найден.");

            return new FileInfo(result);
        }
    }
}
