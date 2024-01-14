namespace BlazorEcommerce.Client.Services.AddressService
{
    public interface IAddressService
    {
        Task<Address> AddOrUpdateAddress(Address address);
        Task<Address> GetAddress();
    }
}
