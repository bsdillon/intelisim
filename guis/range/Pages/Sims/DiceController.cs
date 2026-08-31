using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace range;

[ApiController]
public class DiceWsController : ControllerBase
{
    private readonly DiceHtmlRenderer _html;

    public DiceWsController(DiceHtmlRenderer html) => _html = html;

    [Route("/ws/dice")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var session = new DiceSession(socket, _html);
        await session.RunAsync(HttpContext.RequestAborted);
    }
}

public sealed class DiceSession
{
    private readonly WebSocket _ws;
    private readonly DiceHtmlRenderer _html;
    private CancellationTokenSource? cancellationTokenSource;
    private readonly List<int> _lastFrames = new();
    private static readonly string[] Faces = ["⚀", "⚁", "⚂", "⚃", "⚄", "⚅"];

    public DiceSession(WebSocket ws, DiceHtmlRenderer html)
    {
        _ws = ws;
        _html = html;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8 * 1024];
        while (_ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await _ws.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close) break;

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var cmd = ExtractCmd(json); // "start" | "stop" | "replay"

            switch (cmd)
            {
                case "start":
                    cancellationTokenSource?.Cancel();
                    cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    _ = RollAsync(cancellationTokenSource.Token, replay: false);
                    break;
                case "stop":
                    cancellationTokenSource?.Cancel();
                    await SendAsync(_html.Status("Stopped"));
                    break;
                case "replay":
                    cancellationTokenSource?.Cancel();
                    cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    _ = RollAsync(cancellationTokenSource.Token, replay: true);
                    break;
            }
        }

        cancellationTokenSource?.Cancel();
        if (_ws.State == WebSocketState.Open)
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    private async Task RollAsync(CancellationToken ct, bool replay)
    {
        var frames = replay && _lastFrames.Count > 0
            ? _lastFrames.ToList()
            : GenerateFrames();

        if (!replay)
        {
            _lastFrames.Clear();
            _lastFrames.AddRange(frames);
        }

        await SendAsync(_html.Status(replay ? "Replaying…" : "Rolling…"));

        try
        {
            for (var i = 0; i < frames.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var final = i == frames.Count - 1;
                await SendAsync(_html.Die(frames[i], spinning: !final));
                await Task.Delay(final ? 0 : 80, ct);
            }

            await SendAsync(_html.Status($"Result: {frames[^1]}"));
        }
        catch (OperationCanceledException)
        {
            // stop is the expected path
        }
    }

    private static List<int> GenerateFrames()
    {
        var rng = Random.Shared;
        var n = rng.Next(12, 20);
        var frames = Enumerable.Range(0, n).Select(_ => rng.Next(1, 7)).ToList();
        frames[^1] = rng.Next(1, 7); // "true" result
        return frames;
    }

    private async Task SendAsync(string html)
    {
        var bytes = Encoding.UTF8.GetBytes(html);
        await _ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
    }

    private static string ExtractCmd(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("cmd", out var c) ? c.GetString() ?? "" : "";
    }
}