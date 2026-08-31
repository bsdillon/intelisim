var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOutputCache();
builder.Services.AddResponseCaching();
builder.Services.AddSingleton<DiceHtmlRenderer>();
builder.Services.AddControllers();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // optional; skip while testing ws://localhost:5143

app.UseStaticFiles();
app.UseResponseCaching();
app.UseOutputCache();
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages().WithStaticAssets();
app.MapStaticAssets();

app.MapGet("/profile/avatar", () => Results.Content(
    $"""
     <div class="alert alert-info">
        <p class="fs-1 fw-bold">🌴 Welcome to my page!</p>
        <p class="fs-3">You arrived on ({DateTime.Now.ToLongTimeString()})</p>
     </div>
     """));

app.Run();