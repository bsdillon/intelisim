using CodeMechanic.Razorhat;

namespace range;

/// <summary>
/// todo: add this to CodeMechanic.Razorhat and finish the spec and test it.  There's value here with handling CSRF + Importmap.
/// https://chatgpt.com/g/g-p-693dc38ba44881918f2206bbb7f1ea5f-drip-ui/c/6aa3b636-f808-83ea-b2e2-d417baf1e1aa
/// </summary>
public static class RazorHatExtensions
{
    public static WebApplicationBuilder UseRazorHat(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddAntiforgery(options => { options.HeaderName = "X-Razorhat-CSRF"; });

        builder.Services.AddSingleton<ImportMap>();

        return builder;
    }
}