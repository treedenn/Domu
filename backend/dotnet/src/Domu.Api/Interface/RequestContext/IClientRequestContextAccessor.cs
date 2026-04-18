namespace Domu.Api.Interface.Mobile.Client;

public interface IClientRequestContextAccessor
{
    ClientRequestContext Current { get; set; }
}
