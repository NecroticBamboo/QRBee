using QRBee.Core.Client;
using System.Collections.Concurrent;

namespace QRBee.Load.Generator;

public class ClientPool
{
    private readonly ConcurrentDictionary<int, ClientSettings> _clientPool = new();
    private readonly ConcurrentDictionary<int, ClientSettings> _merchantPool = new();
    private readonly SecurityServiceFactory _securityServiceFactory;
    private readonly Client _client;

    public ClientPool(SecurityServiceFactory securityServiceFactory, QRBee.Core.Client.Client client)
    {
        _securityServiceFactory = securityServiceFactory;
        this._client = client;
    }

    public async Task<ClientSettings> GetMerchant(int no)
    {
        var merchant = _merchantPool.GetOrAdd(no+100_000_000, x => new ClientSettings(_securityServiceFactory, no, true));
        await merchant.InitialSetup(_client);
        return merchant;
    }

    public async Task<ClientSettings> GetClient(int no)
    {
        var customer = _clientPool.GetOrAdd(no, x => new ClientSettings(_securityServiceFactory, no, false));
        await customer.InitialSetup(_client);
        return customer;
    }
}
