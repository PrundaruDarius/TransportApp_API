using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Data;
using TransportApp_API.DTOs.Tickets;
using TransportApp_API.Models;

namespace TransportApp_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TicketsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        private TicketStatus GetRealStatus(Ticket ticket)
        {
            if (ticket.Status == TicketStatus.Used &&
                ticket.ExpiresAt.HasValue &&
                DateTime.UtcNow > ticket.ExpiresAt.Value)
            {
                return TicketStatus.Expired;
            }

            return ticket.Status;
        }

        private string? ToUtcIso(DateTime? date)
        {
            return date.HasValue
                ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc).ToString("O")
                : null;
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateTicket(CreateTicketRequest request)
        {
            if (request.PaymentMethod != "MockCard")
                return BadRequest(new { message = "Invalid payment method." });

            var userId = GetUserId();
            var now = DateTime.UtcNow;

            var ticket = new Ticket
            {
                UserId = userId,
                PurchasedAt = now,
                ExpiresAt = null,
                Price = GetSingleTicketPrice(),
                Status = TicketStatus.Active,
                UniqueCode = Guid.NewGuid().ToString()
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new TicketResponse
            {
                Id = ticket.Id,
                PurchasedAt = DateTime.SpecifyKind(ticket.PurchasedAt, DateTimeKind.Utc).ToString("O"),
                ExpiresAt = ToUtcIso(ticket.ExpiresAt),
                Price = ticket.Price,
                Status = ticket.Status,
                UniqueCode = ticket.UniqueCode
            });
        }

        [Authorize(Roles = "User")]
        [HttpGet("my")]
        public IActionResult GetMyTickets()
        {
            var userId = GetUserId();

            var tickets = _context.Tickets
                .Where(t => t.UserId == userId)
                .ToList()
                .Select(t => new TicketResponse
                {
                    Id = t.Id,
                    PurchasedAt = DateTime.SpecifyKind(t.PurchasedAt, DateTimeKind.Utc).ToString("O"),
                    ExpiresAt = ToUtcIso(t.ExpiresAt),
                    Price = t.Price,
                    Status = GetRealStatus(t),
                    UniqueCode = t.UniqueCode
                });

            return Ok(tickets);
        }

        [Authorize(Roles = "User")]
        [HttpGet("{id}")]
        public IActionResult GetTicket(int id)
        {
            var userId = GetUserId();

            var ticket = _context.Tickets
                .FirstOrDefault(t => t.Id == id && t.UserId == userId);

            if (ticket == null)
                return NotFound();

            return Ok(new TicketResponse
            {
                Id = ticket.Id,
                PurchasedAt = DateTime.SpecifyKind(ticket.PurchasedAt, DateTimeKind.Utc).ToString("O"),
                ExpiresAt = ToUtcIso(ticket.ExpiresAt),
                Price = ticket.Price,
                Status = GetRealStatus(ticket),
                UniqueCode = ticket.UniqueCode
            });
        }

        [Authorize(Roles = "User")]
        [HttpPut("{id}/use")]
        public async Task<IActionResult> UseTicket(int id)
        {
            var userId = GetUserId();

            var ticket = _context.Tickets
                .FirstOrDefault(t => t.Id == id && t.UserId == userId);

            if (ticket == null)
                return NotFound();

            if (ticket.Status == TicketStatus.Cancelled)
                return BadRequest("Ticket cancelled");

            if (ticket.Status == TicketStatus.Expired)
                return BadRequest("Ticket expired");

            if (ticket.Status == TicketStatus.Used &&
                ticket.ExpiresAt.HasValue &&
                DateTime.UtcNow <= ticket.ExpiresAt.Value)
            {
                return BadRequest("Ticket already activated");
            }

            if (ticket.Status == TicketStatus.Used &&
                ticket.ExpiresAt.HasValue &&
                DateTime.UtcNow > ticket.ExpiresAt.Value)
            {
                ticket.Status = TicketStatus.Expired;
                await _context.SaveChangesAsync();
                return BadRequest("Ticket expired");
            }

            ticket.Status = TicketStatus.Used;
            ticket.ExpiresAt = DateTime.UtcNow.AddMinutes(30);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "User")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelTicket(int id)
        {
            var userId = GetUserId();

            var ticket = _context.Tickets
                .FirstOrDefault(t => t.Id == id && t.UserId == userId);

            if (ticket == null)
                return NotFound();

            if (ticket.Status == TicketStatus.Used)
                return BadRequest("Already used");

            ticket.Status = TicketStatus.Cancelled;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Controller")]
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateTicket(ValidateTicketRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UniqueCode))
            {
                return BadRequest(new ValidateTicketResponse
                {
                    Valid = false,
                    Message = "Cod QR lipsă."
                });
            }

            var ticket = _context.Tickets
                .FirstOrDefault(t => t.UniqueCode == request.UniqueCode);

            if (ticket == null)
            {
                return NotFound(new ValidateTicketResponse
                {
                    Valid = false,
                    Message = "Bilet invalid. Codul nu există."
                });
            }

            if (ticket.Status == TicketStatus.Active)
            {
                return BadRequest(new ValidateTicketResponse
                {
                    Valid = false,
                    Message = "Biletul nu este activat.",
                    TicketId = ticket.Id,
                    Status = ticket.Status.ToString()
                });
            }

            if (ticket.Status == TicketStatus.Cancelled)
            {
                return BadRequest(new ValidateTicketResponse
                {
                    Valid = false,
                    Message = "Biletul a fost anulat.",
                    TicketId = ticket.Id,
                    Status = ticket.Status.ToString()
                });
            }

            if (ticket.Status == TicketStatus.Expired)
            {
                return BadRequest(new ValidateTicketResponse
                {
                    Valid = false,
                    Message = "Biletul este expirat.",
                    TicketId = ticket.Id,
                    Status = ticket.Status.ToString(),
                    ExpiresAt = ToUtcIso(ticket.ExpiresAt)
                });
            }

            if (ticket.Status == TicketStatus.Used &&
                ticket.ExpiresAt.HasValue &&
                DateTime.UtcNow > ticket.ExpiresAt.Value)
            {
                ticket.Status = TicketStatus.Expired;
                await _context.SaveChangesAsync();

                return BadRequest(new ValidateTicketResponse
                {
                    Valid = false,
                    Message = "Biletul este expirat.",
                    TicketId = ticket.Id,
                    Status = TicketStatus.Expired.ToString(),
                    ExpiresAt = ToUtcIso(ticket.ExpiresAt)
                });
            }

            return Ok(new ValidateTicketResponse
            {
                Valid = true,
                Message = "Bilet valid.",
                TicketId = ticket.Id,
                Status = ticket.Status.ToString(),
                ExpiresAt = ToUtcIso(ticket.ExpiresAt)
            });
        }

        private decimal GetSingleTicketPrice()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Data", "prices.json");

            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("prices.json not found");

            var json = System.IO.File.ReadAllText(filePath);

            using var document = System.Text.Json.JsonDocument.Parse(json);

            var priceText = document.RootElement
                .GetProperty("singleTickets")[0]
                .GetProperty("price")
                .GetString();

            if (string.IsNullOrWhiteSpace(priceText))
                throw new Exception("Ticket price not found");

            var numericText = priceText
                .Replace("Lei", "", StringComparison.OrdinalIgnoreCase)
                .Replace("lei", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (!decimal.TryParse(numericText, out var price))
                throw new Exception("Invalid ticket price format");

            return price;
        }
    }
}