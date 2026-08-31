public sealed class SFOReplicationIncidentScenario
{
    public Investigation Create()
    {
        var investigation = CreateInvestigation();

        BuildOperationalContext(investigation);
        AddEvidence(investigation);
        AddHealthIndicators(investigation);
        AddTimeline(investigation);

        return investigation;
    }

    private Investigation CreateInvestigation()
    {
        // Create the SFO investigation
    }

    private void BuildOperationalContext(
        Investigation investigation)
    {
        // Add SFO business and operational context
    }

    private void AddEvidence(
        Investigation investigation)
    {
        // Add replication, RPC, disk, and patch evidence
    }

    private void AddHealthIndicators(
        Investigation investigation)
    {
        // Add replication, disk, and patch health
    }

    private void AddTimeline(
        Investigation investigation)
    {
        // Add UTC incident events
    }
}