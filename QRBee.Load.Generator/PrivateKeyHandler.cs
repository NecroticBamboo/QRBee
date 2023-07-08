using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QRBee.Api.Services;
using QRBee.Core.Security;

namespace QRBee.Load.Generator;

public delegate IPrivateKeyHandler PrivateKeyHandlerFactory(int keyNo);
public delegate ISecurityService SecurityServiceFactory(int keyNo);

internal class PrivateKeyHandler : ServerPrivateKeyHandler
{
    public PrivateKeyHandler(ILogger<ServerPrivateKeyHandler> logger, IConfiguration config, int keyNo) : base(logger, config)
    {
        PrivateKeyFileName = Environment.ExpandEnvironmentVariables($"%TEMP%/!QRBee/QRBee-{keyNo:8X}.key");
    }
}
