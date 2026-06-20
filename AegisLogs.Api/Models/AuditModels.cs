namespace AegisLogs.Api.Models
{
    /// Reprsents an unchangeable, cryptographically signed log record.
    public record AuditEvent
    (
        string EventId,
        string PrevHash,
        string TimestampIso,
        long TimestampNano,
        string EventType,
        string Payload

    );
}


