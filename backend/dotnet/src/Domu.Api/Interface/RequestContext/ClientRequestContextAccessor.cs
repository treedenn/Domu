namespace Domu.Api.Interface.RequestContext;

public sealed class ClientRequestContextAccessor : IClientRequestContextAccessor
{
    public ClientRequestContext Current { get; set; } = new()
    {
        RequestId = Guid.NewGuid().ToString()
    };
}
