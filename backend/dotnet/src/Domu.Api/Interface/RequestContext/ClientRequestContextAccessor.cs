namespace Domu.Api.Interface.Mobile.Client;

public sealed class ClientRequestContextAccessor : IClientRequestContextAccessor
{
    public ClientRequestContext Current { get; set; } = new()
    {
        RequestId = Guid.NewGuid().ToString()
    };
}
