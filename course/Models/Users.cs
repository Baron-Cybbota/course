using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool IsBlocked { get; set; } = false;

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Chat> CreatedChats { get; set; } = new List<Chat>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual ICollection<Complaint> AuthoredComplaints { get; set; } = new List<Complaint>();
        public virtual BlacklistEntry? CurrentBlacklistEntry { get; set; }
        public virtual Moderator? ModeratorProfile { get; set; }
        public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
    }
}