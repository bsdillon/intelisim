using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog.Core;

namespace range;

public sealed class DiceSession
{
    private readonly WebSocket socket;
    private readonly DiceHtmlRenderer html;
    private CancellationTokenSource? cancellationTokenSource;
    private readonly List<int> lastFrames = new();
    private readonly Logger logger;
    private static readonly string[] Faces = ["⚀", "⚁", "⚂", "⚃", "⚄", "⚅"];

    public DiceSession(WebSocket socket, DiceHtmlRenderer html, Logger logger)
    {
        this.socket = socket;
        this.html = html;
        this.logger = logger;
    }


    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8 * 1024];
        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await socket.ReceiveAsync(buffer, cancellationToken);
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
                    await SendAsync(html.Status("Stopped"));
                    break;
                case "replay":
                    cancellationTokenSource?.Cancel();
                    cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    _ = RollAsync(cancellationTokenSource.Token, replay: true);
                    break;
            }
        }

        cancellationTokenSource?.Cancel();
        if (socket.State == WebSocketState.Open)
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    private async Task RollAsync(CancellationToken ct, bool replay)
    {
        var frames = replay && lastFrames.Count > 0
            ? lastFrames.ToList()
            : GenerateFrames();

        if (!replay)
        {
            lastFrames.Clear();
            lastFrames.AddRange(frames);
        }

        await SendAsync(html.Status(replay ? "Replaying…" : "Rolling…"));

        try
        {
            for (var i = 0; i < frames.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var final = i == frames.Count - 1;
                await SendAsync(html.Die(frames[i], spinning: !final));
                await Task.Delay(final ? 0 : 80, ct);
            }

            await SendAsync(html.Status($"Result: {frames[^1]}"));
        }
        catch (OperationCanceledException exception)
        {
            // stop is the expected path
        }
    }

    private static List<int> GenerateFrames()
    {
        // todo: refactor the magic numbers into descriptive variables.
        var rng = Random.Shared;
        var n = rng.Next(12, 20);
        var frames = Enumerable.Range(0, n).Select(_ => rng.Next(1, 7)).ToList();
        frames[^1] = rng.Next(1, 7);
        return frames;
    }

    private async Task SendAsync(string html)
    {
        var bytes = Encoding.UTF8.GetBytes(html);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
    }

    private static string ExtractCmd(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("cmd", out var c) ? c.GetString() ?? "" : "";
    }
}