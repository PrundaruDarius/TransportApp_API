using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Services;
using TransportApp_API.DTOs.Admin.Prices;

namespace TransportApp_API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/prices")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminPricesController : ControllerBase
    {
        private readonly JsonFileService _jsonService;

        public AdminPricesController(JsonFileService jsonService)
        {
            _jsonService = jsonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPrices()
        {
            var pricesData = await _jsonService.ReadJsonAsync<PriceDto>("prices.json");
            return Ok(pricesData);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrice(string id, UpdatePriceRequest request)
        {
            var prices = await _jsonService.ReadJsonAsync<PriceDto>("prices.json");
            var priceItem = prices.FirstOrDefault(p => p.Id == id);
            if (priceItem == null) return NotFound();

            priceItem.Name = request.Name;
            priceItem.Price = request.Price;

            await _jsonService.WriteJsonAsync("prices.json", prices);
            return Ok(priceItem);
        }
    }
}