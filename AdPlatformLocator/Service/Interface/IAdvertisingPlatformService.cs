namespace AdPlatformLocator.Service.Interface
{
    public interface IAdvertisingPlatformService
    {
        Task LoadPlatformsFromStreamAsync(Stream stream);
        List<string> GetPlatformsForLocation(string location);
    }
}
