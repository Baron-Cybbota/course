using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity; // Добавляем using для Identity
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    // Наследуемся от IdentityUser, указывая int как тип первичного ключа
    // IdentityUser уже содержит поля Id, UserName, Email, PhoneNumber, PasswordHash и т.д.
    // Наши существующие поля (Login, Email, PasswordHash) будут перекрыты или дополнены IdentityUser.
    // Важно: Поле Login в IdentityUser называется UserName. Мы можем использовать его.
    // Поле Email также есть. Мы должны быть осторожны, чтобы не дублировать поля.
    public class User : IdentityUser<int> // <int> указывает, что первичный ключ будет int
    {
        // Поле "Login" из вашей ERD будет соответствовать "UserName" в IdentityUser.
        // Поэтому мы можем удалить дублирующее поле "Login" здесь.

        // Поле "Email" также уже есть в IdentityUser.

        // Поле "PasswordHash" также управляется IdentityUser.

        // Сохраняем наши уникальные поля, которые не предоставляет IdentityUser напрямую:
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool IsBlocked { get; set; } = false;

        // Навигационные свойства (связи) - их можно оставить, но убедитесь,
        // что они не конфликтуют с внутренними связями Identity.
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

    // Для ролей, мы также будем использовать класс IdentityRole, но расширим его, если нужно.
    // Для простоты, мы можем использовать IdentityRole<int> напрямую.
    public class Role : IdentityRole<int> // <int> указывает, что первичный ключ будет int
    {
        // Здесь можно добавить дополнительные поля для ролей, если требуется
    }
}