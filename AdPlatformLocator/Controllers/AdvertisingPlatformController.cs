using AdPlatformLocator.Service.Interface;
using AdPlatformLocator.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatformLocator.Controllers
{
    [ApiController]
    [Route("api/advertising-platforms")]
    public class AdvertisingPlatformController : ControllerBase
    {
        private readonly IAdvertisingPlatformService _service;

        public AdvertisingPlatformController(IAdvertisingPlatformService service)
        {
            _service = service;
        }

        /// <summary>
        /// Загрузить рекламные площадки из текстового файла
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// 
        /// POST /api/advertising-platforms/load
        /// Content-Type: multipart/form-data
        /// 
        /// Файл: [выберите файл]
        /// </remarks>
        [HttpPost("load")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerFileUpload]
        public async Task<IActionResult> LoadPlatforms(
     [FromForm] IFormFile file) // Важно оставить IFormFile
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не предоставлен или пустой.");
            try
            {
                using var stream = file.OpenReadStream();
                await _service.LoadPlatformsFromStreamAsync(stream);
                return Ok("Данные успешно загружены.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить список рекламных площадок для заданной локации.
        /// </summary>
        /// <param name="location">Локация в формате /ru, /ru/msk и т.д.</param>
        /// <returns>Список названий площадок</returns>
        [HttpGet]
        public IActionResult GetPlatforms([FromQuery] string location)
        {
            if (string.IsNullOrEmpty(location))
                return BadRequest("Локация не указана.");

            try
            {
                var platforms = _service.GetPlatformsForLocation(location);
                return Ok(platforms);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }
        }
    }
}
