[Fact]
public async Task SFO_Incident_Should_Be_Critical_Risk()
{
    // Arrange
    var scenario = new SFOReplicationIncidentScenario();

    var investigation = scenario.Create();

    // Act
    var result =
        await _investigationEngine.RunAsync(
            investigation);

    // Assert
    Assert.Equal(
        RiskLevel.Critical,
        result.Risk.Level);
}