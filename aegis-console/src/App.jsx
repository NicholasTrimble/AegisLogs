import { useState, useEffect } from 'react';

function App() {
    const [logs, setLogs] = useState([]);
    const [securityStatus, setSecurityStatus] = useState({ status: "LOADING", message: "" });
    const [eventType, setEventType] = useState("");
    const [payload, setPayload] = useState("");

    const API_URL = "http://localhost:5000/api/logs";

    const fetchLogs = async () => {
        try {
            const response = await fetch(API_URL);
            const data = await response.json();
            setLogs(data);
        } catch (err) {
            console.error("Error fetching logs:", err);
        }
    };

    const verifyIntegrity = async () => {
        try {
            const response = await fetch(`${API_URL}/verify`);
            const data = await response.json();
            setSecurityStatus({ status: data.status, message: data.message });
        } catch (err) {
            setSecurityStatus({ status: "COMPROMISED", message: "Warning: Cannot confirm ledger security!" });
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!eventType || !payload) return;

        try {
            await fetch(`${API_URL}?eventType=${encodeURIComponent(eventType)}&payload=${encodeURIComponent(payload)}`, {
                method: 'POST'
            });
            setEventType("");
            setPayload("");
            fetchLogs();
            verifyIntegrity();
        } catch (err) {
            console.error("Error saving log:", err);
        }
    };

    useEffect(() => {
        fetchLogs();
        verifyIntegrity();
    }, []);

    // --- NEW STYLING SYSTEM BASED ON THE PROOFCHAIN DESIGN ---
    const styles = {
        container: {
            fontFamily: '"Times New Roman", Times, Baskerville, Georgia, serif', // Editorial Typography
            padding: '60px 40px',
            backgroundColor: '#fbfbfa', // Muted off-white ivory canvas
            minHeight: '100vh',
            color: '#1a1a1a',
            maxWidth: '1200px',
            margin: '0 auto',
            lineHeight: '1.6'
        },
        metaLabel: {
            textTransform: 'uppercase',
            fontSize: '11px',
            letterSpacing: '0.15em',
            color: securityStatus.status === "SECURE" ? "#2e7d32" : "#b71c1c",
            fontWeight: '700',
            marginBottom: '12px',
            display: 'block'
        },
        title: {
            fontSize: '42px',
            fontWeight: '400',
            letterSpacing: '-0.02em',
            margin: '0 0 24px 0',
            color: '#111'
        },
        subtitle: {
            fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
            fontSize: '16px',
            color: '#555',
            maxWidth: '650px',
            margin: '0 0 40px 0'
        },
        divider: {
            border: 'none',
            borderTop: '1px solid #e5e4e0',
            margin: '40px 0'
        },
        sectionHeading: {
            fontSize: '24px',
            fontWeight: '400',
            marginBottom: '20px',
            letterSpacing: '-0.01em'
        },
        formGroup: {
            display: 'flex',
            gap: '12px',
            marginBottom: '40px'
        },
        inputField: {
            fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
            padding: '12px 16px',
            border: '1px solid #dddcd8',
            backgroundColor: '#fff',
            fontSize: '14px',
            borderRadius: '0px', // Crisp, square edges
            outline: 'none',
            flex: 1
        },
        submitButton: {
            fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
            padding: '12px 24px',
            backgroundColor: '#111', // Striking monochrome dark button
            color: '#fff',
            border: 'none',
            fontSize: '14px',
            fontWeight: '500',
            cursor: 'pointer',
            transition: 'background 0.2s'
        },
        table: {
            width: '100%',
            borderCollapse: 'collapse',
            fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
            fontSize: '14px',
            textAlign: 'left'
        },
        th: {
            padding: '12px 8px',
            borderBottom: '2px solid #111',
            textTransform: 'uppercase',
            fontSize: '11px',
            letterSpacing: '0.1em',
            color: '#666',
            fontWeight: '600'
        },
        td: {
            padding: '16px 8px',
            borderBottom: '1px solid #e5e4e0',
            color: '#333'
        },
        monoText: {
            fontFamily: 'monospace',
            fontSize: '12px',
            color: '#666'
        },
        badge: {
            backgroundColor: '#f0effa',
            color: '#4f46e5',
            padding: '4px 8px',
            fontSize: '11px',
            fontWeight: '600',
            letterSpacing: '0.05em'
        }
    };

    return (
        <div style={styles.container}>
            {/* Dynamic Security State Indicator */}
            <span style={styles.metaLabel}>
                ⚡ SYSTEM STATUS // {securityStatus.status === "SECURE" ? "Verified Unbroken Chain" : "Tamper Alarm Active"}
            </span>

            <h1 style={styles.title}>Cryptographic integrity as a verifiable ledger.</h1>
            <p style={styles.subtitle}>
                AegisLogs intercepts incoming runtime transactions, creates deterministic sequential hashes,
                and pins them to an immutable PostgreSQL schema. This interface exposes direct independent chain validation.
            </p>

            {/* Security Info Panel */}
            <div style={{
                padding: '20px',
                backgroundColor: securityStatus.status === "SECURE" ? '#f4fbf4' : '#fff5f5',
                borderLeft: securityStatus.status === "SECURE" ? '4px solid #2e7d32' : '4px solid #c62828',
                fontSize: '14px',
                fontFamily: '-apple-system, BlinkMacSystemFont, sans-serif',
                marginBottom: '40px'
            }}>
                <strong>Ledger Diagnostic:</strong> {securityStatus.message}
            </div>

            <hr style={styles.divider} />

            {/* Ingestion Console Block */}
            <section>
                <h3 style={styles.sectionHeading}>Send live requests from this console.</h3>
                <form onSubmit={handleSubmit} style={styles.formGroup}>
                    <input
                        type="text"
                        placeholder="Method / Event Type (e.g., AUTH_LOGIN)"
                        value={eventType}
                        onChange={(e) => setEventType(e.target.value)}
                        style={styles.inputField}
                    />
                    <input
                        type="text"
                        placeholder="JSON Payload Body"
                        value={payload}
                        onChange={(e) => setPayload(e.target.value)}
                        style={{ ...styles.inputField, flex: 2 }}
                    />
                    <button type="submit" style={styles.submitButton}>
                        Send request
                    </button>
                </form>
            </section>

            <hr style={styles.divider} />

            {/* Datatable Block */}
            <section>
                <h3 style={styles.sectionHeading}>Every row is structured and inspectable.</h3>
                <table style={styles.table}>
                    <thead>
                        <tr>
                            <th style={styles.th}>Timestamp (Nano)</th>
                            <th style={styles.th}>Event Type</th>
                            <th style={styles.th}>Payload</th>
                            <th style={styles.th}>Previous Hash</th>
                        </tr>
                    </thead>
                    <tbody>
                        {logs.length === 0 ? (
                            <tr>
                                <td colSpan="4" style={{ ...styles.td, color: '#999', textAlign: 'center', padding: '40px' }}>
                                    No records stored in local cryptographic chain. Send a live request above to seed history.
                                </td>
                            </tr>
                        ) : (
                            logs.map((log) => (
                                <tr key={log.eventId}>
                                    <td style={{ ...styles.td, ...styles.monoText }}>{log.timestampNano}</td>
                                    <td style={styles.td}>
                                        <span style={styles.badge}>{log.eventType}</span>
                                    </td>
                                    <td style={styles.td}>{log.payload}</td>
                                    <td style={{ ...styles.td, ...styles.monoText }}>
                                        {log.prevHash.substring(0, 8)}...{log.prevHash.substring(56)}
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </section>
        </div>
    );
}

export default App;