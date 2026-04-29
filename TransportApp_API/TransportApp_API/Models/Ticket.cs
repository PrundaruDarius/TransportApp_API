using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApp_API.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public DateTime PurchasedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public TicketStatus Status { get; set; }

        [Required]
        public string UniqueCode { get; set; } = null!;
    }
}