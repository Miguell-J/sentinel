using System.Text.Json;
using Sentinel.Api.Security;
using Sentinel.Domain.Incidents;
using Sentinel.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSentinelInfrastructure(builder.Configuration);

var app = builder.Build();
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", async (IIncidentStore store, CancellationToken ct) =>
    await store.PingAsync(ct)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.MapPost("/api/v1/incidents", CreateIncident);
app.MapGet("/api/v1/incidents/{id:guid}", GetIncident);
app.MapGet("/webhooks/whatsapp", VerifyWhatsappSubscription);
app.MapPost("/webhooks/whatsapp", ReceiveWhatsappWebhook);
app.Run();

static async Task<IResult> CreateIncident(
    HttpContext http, IIncidentStore store, IConfiguration config, CancellationToken ct)
{
    if (!WebhookSecurity.IsAuthorizedBearer(http.Request.Headers.Authorization.ToString(), config["Sentinel:IngestToken"]))
        return Results.Unauthorized();

    if (http.Request.ContentLength is > 16384)
        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

    NewIncident? input;
    try
    {
        input = await http.Request.ReadFromJsonAsync<NewIncident>(cancellationToken: ct);
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { error = "Invalid JSON." });
    }

    if (input is null || string.IsNullOrWhiteSpace(input.Source) ||
        string.IsNullOrWhiteSpace(input.Repository) ||
        string.IsNullOrWhiteSpace(input.Fingerprint) ||
        string.IsNullOrWhiteSpace(input.Title) ||
        input.Source.Length > 80 || input.Repository.Length > 250 ||
        input.Fingerprint.Length > 250 || input.Title.Length > 500)
        return Results.BadRequest(new { error = "Invalid incident fields." });

    var incident = await store.AddOrGetAsync(input, ct);
    return Results.Created($"/api/v1/incidents/{incident.Id}", incident);
}

static async Task<IResult> GetIncident(
    Guid id, HttpContext http, IIncidentStore store, IConfiguration config, CancellationToken ct)
{
    if (!WebhookSecurity.IsAuthorizedBearer(http.Request.Headers.Authorization.ToString(), config["Sentinel:IngestToken"]))
        return Results.Unauthorized();

    var incident = await store.FindAsync(id, ct);
    return incident is null ? Results.NotFound() : Results.Ok(incident);
}

static IResult VerifyWhatsappSubscription(HttpContext http, IConfiguration config)
{
    var query = http.Request.Query;
    if (query["hub.mode"] != "subscribe" ||
        !WebhookSecurity.FixedTimeEquals(query["hub.verify_token"].ToString(), config["WhatsApp:VerifyToken"]))
        return Results.StatusCode(StatusCodes.Status403Forbidden);

    return Results.Text(query["hub.challenge"].ToString(), "text/plain");
}

static async Task<IResult> ReceiveWhatsappWebhook(
    HttpContext http, IConfiguration config, ILogger<Program> logger, CancellationToken ct)
{
    const int maxBytes = 1024 * 1024;
    if (string.IsNullOrWhiteSpace(config["WhatsApp:AppSecret"]))
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    if (http.Request.ContentLength is > maxBytes)
        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

    using var buffer = new MemoryStream();
    var chunk = new byte[8192];
    int count;
    while ((count = await http.Request.Body.ReadAsync(chunk, ct)) > 0)
    {
        if (buffer.Length + count > maxBytes)
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
        buffer.Write(chunk, 0, count);
    }

    if (!WebhookSecurity.VerifyMetaSignature(
        buffer.ToArray(), http.Request.Headers["X-Hub-Signature-256"].ToString(), config["WhatsApp:AppSecret"]))
        return Results.StatusCode(StatusCodes.Status403Forbidden);

    // Authenticated does not mean authorized. No message parsing or command execution in v0.1.
    logger.LogInformation("Validated WhatsApp webhook received; processing disabled in bootstrap.");
    return Results.Accepted();
}
