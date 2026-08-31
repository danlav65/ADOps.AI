using System.Text.Json;
using ADOps.Core.Entities;

namespace ADOps.Infrastructure.Collectors.Rpc;

public sealed class RpcOutputParser : IRpcOutputParser
{
    public RpcRecord Parse(
        string domainController,
        string content,
        CollectorContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            domainController);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            content);

        ArgumentNullException.ThrowIfNull(context);

        using var document =
            JsonDocument.Parse(content);

        var root =
            document.RootElement;

        var target =
            GetRequiredString(
                root,
                "ComputerName");

        var success =
            GetRequiredBool(
                root,
                "TcpTestSucceeded");

        var sourceAddress =
            GetOptionalAddress(
                root,
                "SourceAddress");

        var remoteAddress =
            GetOptionalAddress(
                root,
                "RemoteAddress");

        var remotePort =
            GetOptionalInt32(
                root,
                "RemotePort");

        var interfaceAlias =
            GetOptionalString(
                root,
                "InterfaceAlias");

        return new RpcRecord
        {
            DomainController =
                domainController,

            Target =
                target,

            SourceAddress =
                sourceAddress,

            RemoteAddress =
                remoteAddress,

            RemotePort =
                remotePort,

            InterfaceAlias =
                interfaceAlias,

            Success =
                success,

            ErrorCode =
                null,

            ErrorMessage =
                success
                    ? null
                    : "TCP port 135 connectivity test failed.",

            CollectedUtc =
                DateTimeOffset.UtcNow,

            SourceCommand =
                $"Test-NetConnection -ComputerName '{target}' -Port 135"
        };
    }

    private static string GetRequiredString(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            throw new ArgumentException(
                $"RPC report is missing required property '{propertyName}'.");
        }

        if (property.ValueKind !=
            JsonValueKind.String)
        {
            throw new ArgumentException(
                $"RPC report contains an invalid string property '{propertyName}'.");
        }

        var value =
            property.GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"RPC report contains an empty property '{propertyName}'.");
        }

        return value;
    }

    private static bool GetRequiredBool(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            throw new ArgumentException(
                $"RPC report is missing required property '{propertyName}'.");
        }

        if (property.ValueKind ==
            JsonValueKind.True)
        {
            return true;
        }

        if (property.ValueKind ==
            JsonValueKind.False)
        {
            return false;
        }

        throw new ArgumentException(
            $"RPC report contains an invalid boolean property '{propertyName}'.");
    }

    private static string? GetOptionalString(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        if (property.ValueKind ==
            JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind !=
            JsonValueKind.String)
        {
            return property.ToString();
        }

        return property.GetString();
    }

    private static string? GetOptionalAddress(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        if (property.ValueKind ==
            JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind ==
            JsonValueKind.String)
        {
            return property.GetString();
        }

        if (property.ValueKind ==
            JsonValueKind.Object)
        {
            if (property.TryGetProperty(
                "IPAddress",
                out var ipAddress))
        {
            return ipAddress.ValueKind ==
                   JsonValueKind.String
                ? ipAddress.GetString()
                : ipAddress.ToString();
        }

        if (property.TryGetProperty(
                "IPAddressToString",
                out var ipAddressToString))
        {
            return ipAddressToString.ValueKind ==
                    JsonValueKind.String
                ? ipAddressToString.GetString()
                : ipAddressToString.ToString();
        }
    }

        return property.ToString();
    }

    private static int? GetOptionalInt32(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        if (property.ValueKind ==
            JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind ==
            JsonValueKind.Number &&
            property.TryGetInt32(
                out var value))
        {
            return value;
        }

        throw new ArgumentException(
            $"RPC report contains an invalid integer property '{propertyName}'.");
    }
}