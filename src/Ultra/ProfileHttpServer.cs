using ByteSizeLib;
using Spectre.Console;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Ultra
{
    internal class ProfileHttpServer
    {
        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static async Task StartAndOpen(string jsonFilePath, CancellationToken token)
        {
            int port = GetFreeTcpPort();
            string localUrl = $"http://127.0.0.1:{port}/profile.json";

            string encodedUrl = WebUtility.UrlEncode(localUrl);
            string firefoxProfilerUrl = $"https://profiler.firefox.com/from-url/{encodedUrl}";

            using var listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");

            var tcs = new TaskCompletionSource<bool>();
            token.Register(() => tcs.TrySetResult(true));

            try
            {
                listener.Start();
                AnsiConsole.MarkupLine($"Server running at [blue]{localUrl}[/]");

                Process.Start(new ProcessStartInfo(firefoxProfilerUrl) { UseShellExecute = true });

                while (listener.IsListening && !token.IsCancellationRequested)
                {
                    var getContextTask = listener.GetContextAsync();
                    var cancelTask = tcs.Task;

                    var completedTask = await Task.WhenAny(getContextTask, cancelTask);
                    if (completedTask == cancelTask)
                    {
                        break;
                    }

                    HttpListenerContext context = await getContextTask;
                    _ = Task.Run(() => HandleRequest(context, jsonFilePath));
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Encountered error when starting browser and open server:[/]. {ex.Message}");
                AnsiConsole.MarkupLine($"Go to [blue]https://profiler.firefox.com/ [/] and upload the file manually:");
                AnsiConsole.MarkupLine($"[green]{jsonFilePath}[/]");
            }
            finally
            {
                if (listener.IsListening)
                {
                    listener.Stop();
                }
            }
        }

        private static async Task HandleRequest(HttpListenerContext context, string jsonFilePath)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/profile.json")
                {
                    AnsiConsole.MarkupLine($"[grey]Serving file to {request.RemoteEndPoint} ...[/]");
                    response.ContentType = "application/json";
                    response.Headers.Add("Access-Control-Allow-Origin", "*");

                    await using var fileStream = File.OpenRead(jsonFilePath);
                    await fileStream.CopyToAsync(response.OutputStream);
                    AnsiConsole.MarkupLine("[grey]File sent.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]Received unknown request: {request.Url}[/]");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error handling request: {ex.Message}[/]");
                if (response.OutputStream.CanWrite)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            finally
            {
                response.OutputStream.Close();
            }
        }
    }
}
