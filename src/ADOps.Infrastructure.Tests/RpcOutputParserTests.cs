using System.Text.Json;
using ADOps.Core.Entities;
using ADOps.Infrastructure.Collectors.Rpc;

namespace ADOps.Infrastructure.Tests;

public sealed class RpcOutputParserTests
{
    [Fact]
    public void Parse_SuccessfulTcpTest_ReturnsSuccessfulRpcRecord()
    {
        var parser =
            new RpcOutputParser();

        var content =
            """
            {"ComputerName":"ZUSW-DC1","RemoteAddress":"10.10.10.10","RemotePort":135,"InterfaceAlias":"Ethernet","SourceAddress":"10.10.20.10","TcpTestSucceeded":true}
            """;

        var context =
            CreateContext();

        var record =
            parser.Parse(
                "SFOFLEX-DC1",
                content,
                context);

        Assert.Equal(
            "SFOFLEX-DC1",
            record.DomainController);

        Assert.Equal(
            "ZUSW-DC1",
            record.Target);

        Assert.True(
            record.Success);

        Assert.Equal(
            "10.10.20.10",
            record.SourceAddress);

        Assert.Equal(
            "10.10.10.10",
            record.RemoteAddress);

        Assert.Equal(
            135,
            record.RemotePort);

        Assert.Equal(
            "Ethernet",
            record.InterfaceAlias);

        Assert.Null(
            record.ErrorCode);

        Assert.Null(
            record.ErrorMessage);

        Assert.False(
            string.IsNullOrWhiteSpace(
                record.SourceCommand));

        Assert.Equal(
            "Test-NetConnection -ComputerName 'ZUSW-DC1' -Port 135",
            record.SourceCommand);

        Assert.NotEqual(
            default,
            record.CollectedUtc);
    }

    [Fact]
    public void Parse_RemoteTcpTestWithCimSourceAddress_ExtractsIpAddress()
    {
        var parser =
            new RpcOutputParser();

        var content =
            """
            {
                "ComputerName":"SFOFLEX-DC1",
                "RemoteAddress":{
                    "Address":1987718969,
                    "AddressFamily":2,
                    "IPAddressToString":"57.47.122.118"
            },
            "RemotePort":135,
            "InterfaceAlias":"Ethernet",
            "SourceAddress":{
                "IPAddress":"57.38.8.68",
                "InterfaceAlias":"Ethernet",
                "AddressFamily":2
            },
            "TcpTestSucceeded":true
        }
        """;

        var context =
            CreateContext();

        var record =
            parser.Parse(
                "ZUSW-DC01",
                content,
                context);

        Assert.True(
            record.Success);

        Assert.Equal(
            "57.38.8.68",
            record.SourceAddress);

        Assert.Equal(
            "57.47.122.118",
            record.RemoteAddress);

        Assert.Equal(
            135,
            record.RemotePort);

        Assert.Equal(
            "Ethernet",
            record.InterfaceAlias);
    }

    [Fact]
    public void Parse_FailedTcpTest_ReturnsFailedRpcRecord()
    {
        var parser =
            new RpcOutputParser();

        var content =
            """
            {"ComputerName":"ZUSW-DC1","RemoteAddress":null,"RemotePort":0,"InterfaceAlias":null,"SourceAddress":null,"TcpTestSucceeded":false}
            """;

        var context =
            CreateContext();

        var record =
            parser.Parse(
                "SFOFLEX-DC1",
                content,
                context);

        Assert.Equal(
            "SFOFLEX-DC1",
            record.DomainController);

        Assert.Equal(
            "ZUSW-DC1",
            record.Target);

        Assert.False(
            record.Success);

        Assert.Null(
            record.ErrorCode);

        Assert.Equal(
            "TCP port 135 connectivity test failed.",
            record.ErrorMessage);
    }

    [Fact]
    public void Parse_FailedTcpTest_DoesNotInventAdReplicationErrorCode()
    {
        var parser =
            new RpcOutputParser();

        var content =
            """
            {"ComputerName":"ZUSW-DC1","RemoteAddress":null,"RemotePort":0,"InterfaceAlias":null,"SourceAddress":null,"TcpTestSucceeded":false}
            """;

        var context =
            CreateContext();

        var record =
            parser.Parse(
                "SFOFLEX-DC1",
                content,
                context);

        Assert.False(
            record.Success);

        Assert.Null(
            record.ErrorCode);
    }

    [Fact]
    public void Parse_MissingComputerName_Throws()
    {
        var parser =
            new RpcOutputParser();

        var content =
            """
            {"TcpTestSucceeded":false}
            """;

        var context =
            CreateContext();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    parser.Parse(
                        "SFOFLEX-DC1",
                        content,
                        context));

        Assert.Contains(
            "ComputerName",
            exception.Message);
    }

    [Fact]
    public void Parse_MissingTcpTestSucceeded_Throws()
    {
        var parser =
            new RpcOutputParser();

        var content =
            """
            {"ComputerName":"ZUSW-DC1"}
            """;

        var context =
            CreateContext();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    parser.Parse(
                        "SFOFLEX-DC1",
                        content,
                        context));

        Assert.Contains(
            "TcpTestSucceeded",
            exception.Message);
    }

    [Fact]
    public void Parse_EmptyContent_Throws()
    {
        var parser =
            new RpcOutputParser();

        var context =
            CreateContext();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    parser.Parse(
                        "SFOFLEX-DC1",
                        "",
                        context));

        Assert.Contains(
            "content",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_InvalidJson_Throws()
    {
        var parser =
            new RpcOutputParser();

        var context =
            CreateContext();

        Assert.ThrowsAny<JsonException>(
            () =>
                parser.Parse(
                    "SFOFLEX-DC1",
                    "not valid json",
                    context));
    }

    private static CollectorContext CreateContext()
    {
        return new CollectorContext
        {
            InvestigationId =
                "INC-SFO-20260709",

            Site =
                "SFO",

            DomainName =
                "apcflex.aero",

            DomainControllers =
                ["SFOFLEX-DC1"]
        };
    }
}