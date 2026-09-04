using Microsoft.AspNetCore.Mvc;
using Serilog.Core;

namespace range;

/// <summary>
/// This is here as a backup, in case my use of WS's in Razor Pages doesn't work out.
/// </summary>
[ApiController]
public class DiceWsController : ControllerBase
{
    private readonly DiceHtmlRenderer _html;
    private readonly Logger logger;

    public DiceWsController(DiceHtmlRenderer html, Logger logger)
    {
        _html = html;
        this.logger = logger;
    }

    [Route("/ws/dice")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var session = new DiceSession(socket, _html, this.logger);
        await session.RunAsync(HttpContext.RequestAborted);
    }
}