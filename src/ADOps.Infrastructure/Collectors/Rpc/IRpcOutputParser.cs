using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;

namespace ADOps.Infrastructure.Collectors.Rpc;

public interface IRpcOutputParser
{
    RpcRecord Parse(
        string domainController,
        string content,
        CollectorContext context);
}
