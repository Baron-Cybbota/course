using System;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class ChatParticipant
    {
        public int Id { get; set; }

        public int ChatId { get; set; }

        public int UserId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    }
}