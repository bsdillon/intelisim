using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CodeMechanic.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog.Core;

namespace range
{
    public class DiceRollModel : PageModel
    {
        private readonly IRazorPartialRenderer _razor;
        private readonly Logger logger;

        public void OnGet()
        {
            logger.Information("GO!  DICE ROLL!");
        }

        public DieViewModel Die { get; private set; } = new(0, false);

        public DiceRollModel(IRazorPartialRenderer razor, Logger logger)
        {
            _razor = razor;
            this.logger = logger;
        }

        public async Task<IActionResult> OnGetWs()
        {
            logger.Information($"{nameof(OnGetWs)}");
            var is_not_a_ws_request = !HttpContext.WebSockets.IsWebSocketRequest;
            logger.Information($"{nameof(is_not_a_ws_request)} :>> {is_not_a_ws_request}");

            if (is_not_a_ws_request)
                return StatusCode(StatusCodes.Status400BadRequest);

            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            if (socket != null)
                logger.Information("Websocket established.");

            socket.State.Dump(printFn: logger.Information);
            socket.CloseStatus.Dump(printFn: logger.Information);

            try
            {
                await RunAsync(socket, HttpContext.RequestAborted);
            }
            catch (WebSocketException exception)
            {
                logger.Information($"{nameof(exception)} :>> {exception.ToString()}");
            }
            catch (OperationCanceledException exception)
            {
                logger.Information($"{nameof(exception)} :>> {exception.ToString()}");
            }

            logger.Information($"WS call completed. returning an emptyresult");
            return new EmptyResult();
        }

        private async Task RunAsync(WebSocket socket, CancellationToken httpCt)
        {
            logger.Information($"{nameof(RunAsync)}");

            var frames = new List<int>();
            CancellationTokenSource? rollCts = null;
            var buffer = new byte[8 * 1024];

            while (socket.State == WebSocketState.Open && !httpCt.IsCancellationRequested)
            {
                logger.Information("state is still open within while loop");
                var result = await socket.ReceiveAsync(buffer, httpCt);
                if (result.MessageType == WebSocketMessageType.Close) break;

                var cmd = ReadCmd(buffer, result.Count);
                logger.Information($"{nameof(cmd)} :>> {cmd}");
                rollCts?.Cancel();

                switch (cmd)
                {
                    case "start":
                        frames = GenerateFrames();
                        rollCts = CancellationTokenSource.CreateLinkedTokenSource(httpCt);
                        _ = StreamAsync(socket, frames, rollCts.Token);
                        break;
                    case "replay" when frames.Count > 0:
                        rollCts = CancellationTokenSource.CreateLinkedTokenSource(httpCt);
                        _ = StreamAsync(socket, frames, rollCts.Token);
                        break;
                    case "stop":
                        logger.Information("Sending back the _Status partial");
                        await SendPartial(socket, "_Status", "Stopped");
                        break;
                }
            }
        }

        private async Task StreamAsync(WebSocket socket, List<int> frames, CancellationToken ct)
        {
            try
            {
                await SendPartial(socket, "_Status", "Rolling…");
                for (var i = 0; i < frames.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var last = i == frames.Count - 1;
                    await SendPartial(socket, "_Die", new DieViewModel(frames[i], !last));
                    if (!last) await Task.Delay(80, ct);
                }

                await SendPartial(socket, "_Status", $"Result: {frames[^1]}");
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task SendPartial(WebSocket socket, string name, object? model)
        {
            logger.Information($"sending partial '{name}'");
            string html = await _razor.RenderAsync(HttpContext, $"/Pages/Sims/{name}.cshtml", model);
            byte[] bytes = Encoding.UTF8.GetBytes(html);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
        }

        private static List<int> GenerateFrames()
        {
            var n = Random.Shared.Next(12, 20);
            return Enumerable.Range(0, n).Select(_ => Random.Shared.Next(1, 7)).ToList();
        }

        private static string ReadCmd(byte[] buffer, int count)
        {
            using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(buffer, 0, count));
            return doc.RootElement.TryGetProperty("cmd", out var c) ? c.GetString() ?? "" : "";
        }
    }
}