using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AegisLogs.Api.Services;
using AegisLogs.Api.Models;

namespace AegisLogs.Api.Services;

public class IntegrityScanner
{
    private readonly AegisDbContext _context;

    public IntegrityScanner(AegisDbContext context)
    {
        _context = context;
    }

    public async Task<bool> VerifyChainAsync()
    {
        var logs = await _context.AuditLogs
            .OrderBy(l => l.TimestampNano)
            .ToListAsync();

        if (!logs.Any()) return true;

        var genesisBlock = logs.First();
        string expectedGenesisZeros = new string('0', 64);
        if (genesisBlock.PrevHash != expectedGenesisZeros)
        {
            return false; 
        }

        for (int i = 1; i < logs.Count; i++)
        {
            var previousLog = logs[i - 1];
            var currentLog = logs[i];

            string serializedPrev = CryptographyEngine.ToCanonicalJson(previousLog);
            string calculatedPrevHash = CryptographyEngine.ComputeHash(serializedPrev);

            if (currentLog.PrevHash != calculatedPrevHash)
            {
                return false;
            }
        }

        return true;
    }
}