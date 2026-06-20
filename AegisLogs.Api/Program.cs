using Microsoft.EntityFrameworkCore;
using AegisLogs.Api;
using AegisLogs.Api.Models;
using AegisLogs.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AegisDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IntegrityScanner>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors(); 


app.MapPost("/api/logs", async (string eventType, string payload, AegisDbContext db) =>
{
    var lastLog = await db.AuditLogs.OrderByDescending(l => l.TimestampNano).FirstOrDefaultAsync();

    string prevHash = lastLog == null
        ? new string('0', 64)
        : CryptographyEngine.ComputeHash(CryptographyEngine.ToCanonicalJson(lastLog));

    long nanoseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000;

    var newEvent = new AuditEvent(
        EventId: Guid.CreateVersion7().ToString(),
        PrevHash: prevHash,
        TimestampIso: DateTime.UtcNow.ToString("o"),
        TimestampNano: nanoseconds,
        EventType: eventType,
        Payload: payload
    );

    db.AuditLogs.Add(newEvent);
    await db.SaveChangesAsync();

    return Results.Ok(newEvent);
});

app.MapGet("/api/logs/verify", async (IntegrityScanner scanner) =>
{
    bool isChainValid = await scanner.VerifyChainAsync();

    if (isChainValid)
    {
        return Results.Ok(new { status = "SECURE", message = "No data tampering detected." });
    }

    return Results.BadRequest(new { status = "COMPROMISED", message = "Warning: Cryptographic history has been modified!" });
});

app.MapGet("/api/logs", async (AegisDbContext db) =>
{
    var logs = await db.AuditLogs.OrderBy(l => l.TimestampNano).ToListAsync();
    return Results.Ok(logs);
});

app.Run();