using AdPlatformLocator.Models;
using AdPlatformLocator.Service.Interface;

namespace AdPlatformLocator.Service
{
    public class AdvertisingPlatformService : IAdvertisingPlatformService
    {
        private List<AdvertisingPlatform> _platforms = new();

        public List<string> GetPlatformsForLocation(string location)
        {
            return _platforms
            .Where(p => p.Locations.Any(l => location.StartsWith(l))) 
            .OrderBy(p => p.Locations.Min(l => l.Count(c => c == '/'))) 
            .Select(p => p.Name) 
            .ToList();
        }

   

        public async Task LoadPlatformsFromStreamAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var platforms = new List<AdvertisingPlatform>();
            var names = new HashSet<string>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;
                var name = parts[0].Trim();
                if (string.IsNullOrEmpty(name) || names.Contains(name)) continue;
                var locations = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => l.StartsWith("/"))
                    .ToList();
                if (locations.Count == 0) continue;
                platforms.Add(new AdvertisingPlatform { Name = name, Locations = locations });
                names.Add(name);
            }
            _platforms = platforms;
        }
    }
}
