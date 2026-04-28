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
            if (ticket.Status == TicketStatus.Active && DateTime.UtcNow > ticket.ExpiresAt)
            {
                return TicketStatus.Expired;
            }

            return ticket.Status;
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateTicket(CreateTicketRequest request)
        {
            if (request.PaymentMethod != "MockCard")
            {
                return BadRequest(new { message = "Invalid payment method." });
            }

            var userId = GetUserId();

            var now = DateTime.UtcNow;

            var ticket = new Ticket
            {
                UserId = userId,
                PurchasedAt = now,
                ExpiresAt = now.AddMinutes(30),
                Price = GetSingleTicketPrice(),
                Status = TicketStatus.Active,
                UniqueCode = Guid.NewGuid().ToString()
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new TicketResponse
            {
                Id = ticket.Id,
                PurchasedAt = ticket.PurchasedAt,
                ExpiresAt = ticket.ExpiresAt,
                Price = ticket.Price,
                Status = ticket.Status,
                UniqueCode = ticket.UniqueCode
            });
        }

        
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
                    PurchasedAt = t.PurchasedAt,
                    ExpiresAt = t.ExpiresAt,
                    Price = t.Price,
                    Status = GetRealStatus(t),
                    UniqueCode = t.UniqueCode
                });

            return Ok(tickets);
        }

        
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
                PurchasedAt = ticket.PurchasedAt,
                ExpiresAt = ticket.ExpiresAt,
                Price = ticket.Price,
                Status = GetRealStatus(ticket),
                UniqueCode = ticket.UniqueCode
            });
        }

        
        [HttpPut("{id}/use")]
        public async Task<IActionResult> UseTicket(int id)
        {
            var userId = GetUserId();

            var ticket = _context.Tickets
                .FirstOrDefault(t => t.Id == id && t.UserId == userId);

            if (ticket == null)
                return NotFound();

            if (GetRealStatus(ticket) == TicketStatus.Expired)
                return BadRequest("Ticket expired");

            ticket.Status = TicketStatus.Used;

            await _context.SaveChangesAsync();

            return Ok();
        }

        
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