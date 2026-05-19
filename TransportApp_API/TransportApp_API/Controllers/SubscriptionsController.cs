using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Data;
using TransportApp_API.DTOs.Subscriptions;
using TransportApp_API.Models;
using TransportApp_API.Services;

namespace TransportApp_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly RevenueService _revenueService;

        public SubscriptionsController(
            AppDbContext context,
            IWebHostEnvironment environment,
            RevenueService revenueService)
        {
            _context = context;
            _environment = environment;
            _revenueService = revenueService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        private SubscriptionStatus GetRealStatus(Subscription subscription)
        {
            if (subscription.Status == SubscriptionStatus.Active &&
                DateTime.UtcNow > subscription.ExpiresAt)
            {
                return SubscriptionStatus.Expired;
            }

            return subscription.Status;
        }

        private string ToUtcIso(DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc).ToString("O");
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateSubscription(CreateSubscriptionRequest request)
        {
            if (request.PaymentMethod != "MockCard")
                return BadRequest(new { message = "Invalid payment method." });

            var userId = GetUserId();

            var alreadyHasActiveSubscription = _context.Subscriptions
                .Any(s =>
                    s.UserId == userId &&
                    s.Status == SubscriptionStatus.Active &&
                    DateTime.UtcNow <= s.ExpiresAt);

            if (alreadyHasActiveSubscription)
                return BadRequest(new { message = "User already has an active subscription." });

            var subscriptionInfo = GetSubscriptionFromPricesJson(request.SubscriptionTypeId);

            var now = DateTime.UtcNow;

            var subscription = new Subscription
            {
                UserId = userId,
                SubscriptionTypeId = subscriptionInfo.Id,
                Name = subscriptionInfo.Name,
                PurchasedAt = now,
                ExpiresAt = now.AddDays(subscriptionInfo.ValidityDays),
                Price = subscriptionInfo.Price,
                Status = SubscriptionStatus.Active,
                UniqueCode = Guid.NewGuid().ToString()
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            await _revenueService.AddSubscriptionRevenueAsync(subscription.Price, subscription.PurchasedAt);

            return Ok(new SubscriptionResponse
            {
                Id = subscription.Id,
                SubscriptionTypeId = subscription.SubscriptionTypeId,
                Name = subscription.Name,
                PurchasedAt = ToUtcIso(subscription.PurchasedAt),
                ExpiresAt = ToUtcIso(subscription.ExpiresAt),
                Price = subscription.Price,
                Status = subscription.Status,
                UniqueCode = subscription.UniqueCode
            });
        }

        [Authorize(Roles = "User")]
        [HttpGet("my")]
        public IActionResult GetMySubscriptions()
        {
            var userId = GetUserId();

            var subscriptions = _context.Subscriptions
                .Where(s => s.UserId == userId)
                .ToList()
                .Select(s => new SubscriptionResponse
                {
                    Id = s.Id,
                    SubscriptionTypeId = s.SubscriptionTypeId,
                    Name = s.Name,
                    PurchasedAt = ToUtcIso(s.PurchasedAt),
                    ExpiresAt = ToUtcIso(s.ExpiresAt),
                    Price = s.Price,
                    Status = GetRealStatus(s),
                    UniqueCode = s.UniqueCode
                });

            return Ok(subscriptions);
        }

        [Authorize(Roles = "User")]
        [HttpGet("{id}")]
        public IActionResult GetSubscription(int id)
        {
            var userId = GetUserId();

            var subscription = _context.Subscriptions
                .FirstOrDefault(s => s.Id == id && s.UserId == userId);

            if (subscription == null)
                return NotFound();

            return Ok(new SubscriptionResponse
            {
                Id = subscription.Id,
                SubscriptionTypeId = subscription.SubscriptionTypeId,
                Name = subscription.Name,
                PurchasedAt = ToUtcIso(subscription.PurchasedAt),
                ExpiresAt = ToUtcIso(subscription.ExpiresAt),
                Price = subscription.Price,
                Status = GetRealStatus(subscription),
                UniqueCode = subscription.UniqueCode
            });
        }

        [Authorize(Roles = "User")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelSubscription(int id)
        {
            var userId = GetUserId();

            var subscription = _context.Subscriptions
                .FirstOrDefault(s => s.Id == id && s.UserId == userId);

            if (subscription == null)
                return NotFound();

            if (subscription.Status == SubscriptionStatus.Cancelled)
                return BadRequest("Already cancelled");

            if (subscription.Status == SubscriptionStatus.Expired ||
                DateTime.UtcNow > subscription.ExpiresAt)
            {
                subscription.Status = SubscriptionStatus.Expired;
                await _context.SaveChangesAsync();

                return BadRequest("Subscription expired");
            }

            subscription.Status = SubscriptionStatus.Cancelled;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Controller")]
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateSubscription(ValidateSubscriptionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UniqueCode))
            {
                return BadRequest(new ValidateSubscriptionResponse
                {
                    Valid = false,
                    Message = "Cod QR lipsă."
                });
            }

            var subscription = _context.Subscriptions
                .FirstOrDefault(s => s.UniqueCode == request.UniqueCode);

            if (subscription == null)
            {
                return NotFound(new ValidateSubscriptionResponse
                {
                    Valid = false,
                    Message = "Abonament invalid. Codul nu există."
                });
            }

            if (subscription.Status == SubscriptionStatus.Cancelled)
            {
                return BadRequest(new ValidateSubscriptionResponse
                {
                    Valid = false,
                    Message = "Abonamentul a fost anulat.",
                    SubscriptionId = subscription.Id,
                    Status = subscription.Status.ToString(),
                    ExpiresAt = ToUtcIso(subscription.ExpiresAt)
                });
            }

            if (subscription.Status == SubscriptionStatus.Expired)
            {
                return BadRequest(new ValidateSubscriptionResponse
                {
                    Valid = false,
                    Message = "Abonamentul este expirat.",
                    SubscriptionId = subscription.Id,
                    Status = subscription.Status.ToString(),
                    ExpiresAt = ToUtcIso(subscription.ExpiresAt)
                });
            }

            if (subscription.Status == SubscriptionStatus.Active &&
                DateTime.UtcNow > subscription.ExpiresAt)
            {
                subscription.Status = SubscriptionStatus.Expired;
                await _context.SaveChangesAsync();

                return BadRequest(new ValidateSubscriptionResponse
                {
                    Valid = false,
                    Message = "Abonamentul este expirat.",
                    SubscriptionId = subscription.Id,
                    Status = SubscriptionStatus.Expired.ToString(),
                    ExpiresAt = ToUtcIso(subscription.ExpiresAt)
                });
            }

            return Ok(new ValidateSubscriptionResponse
            {
                Valid = true,
                Message = "Abonament valid.",
                SubscriptionId = subscription.Id,
                Status = subscription.Status.ToString(),
                ExpiresAt = ToUtcIso(subscription.ExpiresAt)
            });
        }

        private SubscriptionPriceInfo GetSubscriptionFromPricesJson(string subscriptionTypeId)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Data", "prices.json");

            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("prices.json not found");

            var json = System.IO.File.ReadAllText(filePath);

            using var document = JsonDocument.Parse(json);

            var subscriptions = document.RootElement.GetProperty("subscriptions");

            foreach (var item in subscriptions.EnumerateArray())
            {
                var id = item.GetProperty("id").GetString();

                if (id != subscriptionTypeId)
                    continue;

                var name = item.GetProperty("name").GetString();
                var priceText = item.GetProperty("price").GetString();

                if (string.IsNullOrWhiteSpace(id))
                    throw new Exception("Subscription id not found");

                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("Subscription name not found");

                if (string.IsNullOrWhiteSpace(priceText))
                    throw new Exception("Subscription price not found");

                var price = ParsePrice(priceText);
                var validityDays = ParseValidityDays(name);

                if (validityDays <= 0)
                    throw new Exception("Invalid subscription validity format");

                return new SubscriptionPriceInfo
                {
                    Id = id,
                    Name = name,
                    Price = price,
                    ValidityDays = validityDays
                };
            }

            throw new Exception("Subscription not found in prices.json");
        }

        private decimal ParsePrice(string priceText)
        {
            var numericText = priceText
                .Replace("Lei", "", StringComparison.OrdinalIgnoreCase)
                .Replace("lei", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (!decimal.TryParse(numericText, out var price))
                throw new Exception("Invalid subscription price format");

            return price;
        }

        private int ParseValidityDays(string name)
        {
            var match = Regex.Match(name, @"(\d+)\s*zile?", RegexOptions.IgnoreCase);

            if (!match.Success)
                return 0;

            return int.Parse(match.Groups[1].Value);
        }

        private class SubscriptionPriceInfo
        {
            public string Id { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public decimal Price { get; set; }

            public int ValidityDays { get; set; }
        }
    }
}