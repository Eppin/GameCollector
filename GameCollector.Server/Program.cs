using GameCollector.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ProjectService>();
builder.Services.AddScoped<DataService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

// Load data needed for the app
await using (var scope = app.Services.CreateAsyncScope())
{
    var provider = scope.ServiceProvider;
    
    await provider.GetRequiredService<ProjectService>().Load();
    await provider.GetRequiredService<DataService>().Load();
}

app.Run();
