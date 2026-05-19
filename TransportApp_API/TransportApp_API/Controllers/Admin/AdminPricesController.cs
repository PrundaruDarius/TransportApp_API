using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using TransportApp_API.DTOs.Admin.Prices;
using TransportApp_API.Services;

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
            var prices = await _jsonService.ReadJsonNodeAsync("prices.json");
            if (prices == null) return NotFound();

            return Ok(prices);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrice(string id, UpdatePriceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name is required.");

            if (string.IsNullOrWhiteSpace(request.Price))
                return BadRequest("Price is required.");

            var prices = await _jsonService.ReadJsonNodeAsync("prices.json");
            if (prices == null) return NotFound();

            var found = false;

            if (prices is JsonObject root)
            {
                foreach (var property in root)
                {
                    if (property.Value is JsonArray array)
                    {
                        foreach (var item in array)
                        {
                            if (item is JsonObject obj &&
                                obj["id"]?.GetValue<string>() == id)
                            {
                                obj["name"] = request.Name;
                                obj["price"] = request.Price;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found) break;
                }
            }

            if (!found) return NotFound();

            await _jsonService.WriteJsonNodeAsync("prices.json", prices);

            return Ok(prices);
        }
    }
}