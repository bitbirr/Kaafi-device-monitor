using Kaafi.DeviceMonitor.Components;
using Kaafi.DeviceMonitor.Data;
using Kaafi.DeviceMonitor.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString!.Contains(".db"))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Add background service
builder.Services.AddHostedService<DevicePingService>();

// Add API services
builder.Services.AddScoped<DeviceApiService>();
builder.Services.AddScoped<EmployeeApiService>();
builder.Services.AddScoped<EnrollmentApiService>();
builder.Services.AddScoped<AttendanceApiService>();

// Add toast service
builder.Services.AddScoped<ToastService>();

// Configure HttpClient for API calls
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.WebHost.GetSetting("https_port") ?? "https://localhost:5001");
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
