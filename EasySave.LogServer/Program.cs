// EasySave.LogServer/Program.cs
// ASP.NET Core minimal host for the centralised Docker log service

using EasySave.LogServer.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Log directory: read from environment variable LOG_DIR, fallback to /logs (Docker volume)
string logDirectory = Environment.GetEnvironmentVariable("LOG_DIR")
    ?? builder.Configuration["LogDirectory"]
    ?? "/logs";

builder.Services.AddSingleton<ILogStorageService>(_ => new JsonLogStorageService(logDirectory));

// ── App ───────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseRouting();
app.MapControllers();

// Root endpoint — health check
app.MapGet("/", () => Results.Ok(new
{
    service  = "EasySave LogServer",
    version  = "3.0.0",
    status   = "running",
    logDir   = logDirectory
}));

app.Run();
