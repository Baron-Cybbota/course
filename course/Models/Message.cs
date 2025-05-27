using System;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int ChatId { get; set; }
        public virtual Chat Chat { get; set; }

        public int SenderId { get; set; }
        public virtual User Sender { get; set; }

        [Required(ErrorMessage = "Содержание сообщения обязательно")]
        [StringLength(4000)]
        public string Content { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}