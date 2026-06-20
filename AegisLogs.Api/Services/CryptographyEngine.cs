using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AegisLogs.Api.Models;


namespace AegisLogs.Api.Services
{
    public static class CryptographyEngine
    {

        public static string ComputeHash(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA256.HashData(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public static string ToCanonicalJson(AuditEvent ev)
        {
            var sortedMap = new SortedDictionary<string, string>
            {
                { "event_id", ev.EventId },
            { "prev_hash", ev.PrevHash },
            { "timestamp_iso", ev.TimestampIso },
            { "timestamp_nano", ev.TimestampNano.ToString() },
            { "event_type", ev.EventType },
            { "payload", ev.Payload }
            };

            return JsonSerializer.Serialize(sortedMap);
        }

        public static string BuildMerkleRoot(List<string> hashes)
        {
            if (hashes == null || hashes.Count == 0) return string.Empty;

            List<string> currentLayer = new List<string>(hashes);

            while (currentLayer.Count > 1)
            {
                if (currentLayer.Count % 2 != 0)
                {
                    currentLayer.Add(currentLayer[^1]);
                }

                List<string> nextLayer = new List<string>();

                for (int i = 0; i < currentLayer.Count; i += 2)
                {
                    string combinedHash = ComputeHash(currentLayer[i] + currentLayer[i + 1]);
                    nextLayer.Add(combinedHash);
                }

                currentLayer = nextLayer;
            }

            return currentLayer[0];
        }
    }
}
