using System;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class BlacklistEntry
    {
        public int Id { get; set; }

        public int ModeratorId { get; set; }
        public virtual Moderator Moderator { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required(ErrorMessage = "Причина блокировки обязательна")]
        [StringLength(500)]
        public string Reason { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime BlockDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiryDate { get; set; }
    }
}