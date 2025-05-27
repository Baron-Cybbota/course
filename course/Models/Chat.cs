using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class Chat
    {
        public int Id { get; set; }

        public int CreatorId { get; set; }
        public virtual User Creator { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
    }
}