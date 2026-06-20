using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using AegisLogs.Api;
using AegisLogs.Api.Models;
using AegisLogs.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AegisDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IntegrityScanner>();

// 1. Register Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AegisLogs Cryptographic API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();

// 2. Turn on Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AegisLogs API v1"));
}

// --- API ROUTES ---
app.MapPost("/api/logs", async ([FromQuery] string eventType, [FromQuery] string payload, AegisDbContext db) =>
{
    var lastLog = await db.AuditLogs.OrderByDescending(l => l.TimestampNano).FirstOrDefaultAsync();
    string prevHash = lastLog == null ? new string('0', 64) : CryptographyEngine.ComputeHash(CryptographyEngine.ToCanonicalJson(lastLog));
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
})
.WithName("IngestLog")
.WithOpenApi(op => { op.Summary = "Inject an immutable ledger event entry"; return op; });

app.MapGet("/api/logs/verify", async (IntegrityScanner scanner) =>
{
    bool isChainValid = await scanner.VerifyChainAsync();
    if (isChainValid) return Results.Ok(new { status = "SECURE", message = "No data tampering detected." });
    return Results.BadRequest(new { status = "COMPROMISED", message = "Warning: Cryptographic history has been modified!" });
})
.WithName("VerifyChain")
.WithOpenApi(op => { op.Summary = "Execute full cryptographic validation scan"; return op; });

app.MapGet("/api/logs", async (AegisDbContext db) =>
{
    var logs = await db.AuditLogs.OrderBy(l => l.TimestampNano).ToListAsync();
    return Results.Ok(logs);
})
.WithName("GetAllLogs")
.WithOpenApi(op => { op.Summary = "Fetch all chronological ledger logs"; return op; });

app.Run();