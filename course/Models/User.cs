// Models/User.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class User : IdentityUser<int> // <int> указывает, что первичный ключ будет int
    {
        // Поле "Login" из вашей ERD будет соответствовать "UserName" в IdentityUser.
        // Поле "Email" также уже есть в IdentityUser.
        // Поле "PasswordHash" также управляется IdentityUser.

        // Сохраняем наши уникальные поля, которые не предоставляет IdentityUser напрямую:
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool IsBlocked { get; set; } = false;

        // --- УДАЛИТЕ ВСЕ СЛЕДУЮЩИЕ НАВИГАЦИОННЫЕ СВОЙСТВА ---
        // public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        // public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        // public virtual ICollection<Chat> CreatedChats { get; set; } = new List<Chat>();
        // public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        // public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        // public virtual ICollection<Complaint> AuthoredComplaints { get; set; } = new List<Complaint>();
        // public virtual BlacklistEntry? CurrentBlacklistEntry { get; set; }
        // public virtual Moderator? ModeratorProfile { get; set; }
        // public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
        // --- КОНЕЦ УДАЛЯЕМЫХ НАВИГАЦИОННЫХ СВОЙСТВ ---
    }

    public class Role : IdentityRole<int> // <int> указывает, что первичный ключ будет int
    {
        // Здесь можно добавить дополнительные поля для ролей, если требуется
    }
}