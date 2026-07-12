namespace Domu.Api.Interface.RequestContext;

public interface IClientRequestContextAccessor
{
    ClientRequestContext Current { get; set; }
}