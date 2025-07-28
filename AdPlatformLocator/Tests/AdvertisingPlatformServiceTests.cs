using AdPlatformLocator.Service.Interface;
using AdPlatformLocator.Service;
using Xunit;

namespace AdPlatformLocator.Tests
{
    public class AdvertisingPlatformServiceTests
    {
        private readonly IAdvertisingPlatformService _service;

        public AdvertisingPlatformServiceTests()
        {
            _service = new AdvertisingPlatformService();
        }

        [Fact]
        public void LoadPlatformsFromFile_ValidFile_LoadsData()
        {
            // Создаём временный файл
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "Яндекс.Директ:/ru\nРевдинский рабочий:/ru/svrd/revda");

            using (var stream = File.OpenRead(filePath))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            var platforms = _service.GetPlatformsForLocation("/ru/svrd/revda");

            Assert.Contains("Яндекс.Директ", platforms);
            Assert.Contains("Ревдинский рабочий", platforms);

            File.Delete(filePath);
        }

        [Fact]
        public void LoadPlatformsFromFile_InvalidLines_IgnoresInvalid()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, @"Некорректная строка
Пусто:
: /ru/svrd
Директ:/ru
Директ:/ru/svrd
");
            using (var stream = File.OpenRead(filePath))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            var platforms = _service.GetPlatformsForLocation("/ru/svrd");
            // Должна быть только одна площадка с уникальным именем
            Assert.Contains("Директ", platforms);
            Assert.Single(platforms);
            File.Delete(filePath);
        }

        [Fact]
        public void LoadPlatformsFromFile_EmptyFile_NoPlatforms()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "");
            using (var stream = File.OpenRead(filePath))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            var platforms = _service.GetPlatformsForLocation("/ru");
            Assert.Empty(platforms);
            File.Delete(filePath);
        }

        [Fact]
        public void GetPlatformsForLocation_ReturnsCorrectOrder()
        {
            using (var stream = File.OpenRead("test-data.txt"))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            var platforms = _service.GetPlatformsForLocation("/ru/svrd/revda");

            // Проверяем порядок: сначала глобальные, потом локальные
            Assert.Equal("Яндекс.Директ", platforms[0]);
            Assert.Equal("Крутая реклама", platforms[1]);
            Assert.Equal("Ревдинский рабочий", platforms[2]);
        }

        [Fact]
        public void GetPlatformsForLocation_NonexistentLocation_ReturnsEmpty()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "Яндекс.Директ:/ru");
            using (var stream = File.OpenRead(filePath))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            var platforms = _service.GetPlatformsForLocation("/us");
            Assert.Empty(platforms);
            File.Delete(filePath);
        }

        [Fact]
        public void LoadPlatformsFromFile_DuplicateNames_OnlyUnique()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "Директ:/ru\nДирект:/ru/svrd\nДирект:/ru/msk");
            using (var stream = File.OpenRead(filePath))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            var platforms = _service.GetPlatformsForLocation("/ru/svrd");
            Assert.Contains("Директ", platforms);
            Assert.Single(platforms);
            File.Delete(filePath);
        }

        [Fact]
        public void GetPlatformsForLocation_NestedLocations_ReturnsParent()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "Глобальная:/ru\nЛокальная:/ru/svrd");
            using (var stream = File.OpenRead(filePath))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            var platforms = _service.GetPlatformsForLocation("/ru/svrd/ekb");
            Assert.Contains("Глобальная", platforms);
            Assert.Contains("Локальная", platforms);
            File.Delete(filePath);
        }

        [Fact]
        public void LoadPlatformsFromFile_OverwriteOldData()
        {
            var filePath1 = Path.GetTempFileName();
            var filePath2 = Path.GetTempFileName();
            File.WriteAllText(filePath1, "Первая:/ru");
            File.WriteAllText(filePath2, "Вторая:/ru/svrd");
            using (var stream = File.OpenRead(filePath1))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            Assert.Contains("Первая", _service.GetPlatformsForLocation("/ru"));
            using (var stream = File.OpenRead(filePath2))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            Assert.DoesNotContain("Первая", _service.GetPlatformsForLocation("/ru"));
            Assert.Contains("Вторая", _service.GetPlatformsForLocation("/ru/svrd"));
            File.Delete(filePath1);
            File.Delete(filePath2);
        }

        [Fact]
        public void LoadPlatformsFromFile_LocationWithoutSlash_Ignored()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "БезСлеша:ru, /ru");
            using (var stream = File.OpenRead(filePath))
            {
                _service.LoadPlatformsFromStreamAsync(stream).GetAwaiter().GetResult();
            }
            var platforms = _service.GetPlatformsForLocation("/ru");
            Assert.Contains("БезСлеша", platforms);
            File.Delete(filePath);
        }
    }
}
