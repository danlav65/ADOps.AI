using System.Text.RegularExpressions;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;

namespace ADOps.Infrastructure.Collectors.Replication;

public sealed class ReplicationOutputParser
    : IReplicationOutputParser
{
    public IReadOnlyCollection<ReplicationRecord> Parse(
        string sourceDomainController,
        string commandOutput,
        CollectorContext context)
    {
        var records = new List<ReplicationRecord>();

        if (string.IsNullOrWhiteSpace(commandOutput))
        {
            return records;
        }

        var partnerMatches =
            Regex.Matches(
                commandOutput,
                @"From server:\s+(?<partner>\S+)",
                RegexOptions.IgnoreCase);

        for (var index = 0; index < partnerMatches.Count; index++)
        {
            var match =
                partnerMatches[index];

            var partner =
                match.Groups["partner"].Value;

            var sectionStart =
                match.Index;

            var sectionEnd =
                index + 1 < partnerMatches.Count
                    ? partnerMatches[index + 1].Index
                    : commandOutput.Length;

            var section =
                commandOutput[
                    sectionStart..
                    sectionEnd];

            var errorCode =
                ExtractErrorCode(section);

            var errorMessage =
                ExtractErrorMessage(section);

            var success =
                IsSuccessful(section);

            records.Add(
                new ReplicationRecord
                {
                    SourceDomainController =
                        sourceDomainController,

                    PartnerDomainController =
                        partner,

                    Success =
                        success,

                    ErrorCode =
                        errorCode,

                    ErrorMessage =
                        success
                            ? null
                            : errorMessage,

                    SourceSite =
                        context.Site,

                    CollectedUtc =
                        DateTimeOffset.UtcNow
                });
        }

        return records;
    }

    private static bool IsSuccessful(
        string output)
    {
        return Regex.IsMatch(
            output,
            @"was\s+successful",
            RegexOptions.IgnoreCase);
    }

    private static int? ExtractErrorCode(
        string output)
    {
        var match =
            Regex.Match(
                output,
                @"(?<code>\d+)\s+\(0x[0-9a-f]+\)",
                RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return null;
        }

        return int.Parse(
            match.Groups["code"].Value);
    }

    private static string? ExtractErrorMessage(
        string output)
    {
        var match =
            Regex.Match(
                output,
                @"The .*",
                RegexOptions.IgnoreCase);

        return match.Success
            ? match.Value.Trim()
            : null;
    }
}