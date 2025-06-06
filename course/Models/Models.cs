using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace course.Models
{
    // Пользователь/4
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Логин должен быть от 1 до 50 символов")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Пароль обязателен")]
        public string PasswordHash { get; set; } = null!;

        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public bool BlockStatus { get; set; } = false;

        public int Rating { get; set; } = 0;
    }

    // Администратор/7
    public class Administrator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // FK to User

        [Required(ErrorMessage = "Дата назначения обязательна")]
        [DataType(DataType.DateTime)]
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Уровень доступа обязателен")]
        public int AccessLevel { get; set; }
    }

    // Черный_список/9
    public class BlacklistEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AdministratorId { get; set; } // FK to Administrator

        [Required]
        public int UserId { get; set; } // FK to User

        [Required(ErrorMessage = "Причина блокировки обязательна")]
        [StringLength(500)]
        public string Reason { get; set; } = null!;

        [DataType(DataType.DateTime)]
        public DateTime BlockDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? BlockDuration { get; set; }
    }

    // Чат/1
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CreatorId { get; set; } // FK to User

        [Required(ErrorMessage = "Название чата обязательно")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }

    // Сообщение/2
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChatId { get; set; } // FK to Chat

        [Required]
        public int SenderId { get; set; } // FK to User

        [Required(ErrorMessage = "Содержание сообщения обязательно")]
        [StringLength(4000)]
        public string Content { get; set; } = null!;

        [DataType(DataType.DateTime)]
        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsRead { get; set; } = false;
    }

    // Пост/3
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AuthorId { get; set; } // FK to User

        [Required(ErrorMessage = "Заголовок поста обязателен")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Заголовок должен быть от 5 до 200 символов")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Содержание поста обязательно")]
        [DataType(DataType.Html)]
        public string Content { get; set; } = null!;

        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        // Добавлено обратно: Дата последнего редактирования
        [DataType(DataType.DateTime)]
        public DateTime? LastEditDate { get; set; }

        public bool IsHidden { get; set; } = false;

        public int Rating { get; set; } = 0;
    }

    // Комментарий/5
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; } // FK to Post

        [Required]
        public int AuthorId { get; set; } // FK to User

        [Required(ErrorMessage = "Содержание комментария обязательно")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Комментарий должен быть от 1 до 2000 символов")]
        public string Content { get; set; } = null!;

        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        // Добавлено обратно: Дата последнего редактирования
        [DataType(DataType.DateTime)]
        public DateTime? LastEditDate { get; set; }

        public int Rating { get; set; } = 0;
    }

    // Оценка/6 (renamed to Rating)
    public class Rating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // FK to User

        public int? PostId { get; set; } // FK to Post

        public int? CommentId { get; set; } // FK to Comment

        [Required]
        public bool Value { get; set; }
    }

    // Жалоба/8 (renamed to Complaint)
    public enum ComplaintStatus
    {
        [Display(Name = "Подана")]
        Pending,
        [Display(Name = "Рассматривается")]
        InProgress,
        [Display(Name = "Решена")]
        Resolved,
        [Display(Name = "Отклонена")]
        Rejected
    }

public class Complaint
    {
        [Key]
        public int Id { get; set; } // ID_Жалоба

        [Required]
        public int AuthorId { get; set; } // ID_Пользователь (FK)

        public int? AdministratorId { get; set; } // ID_Администратор (FK) - the admin who handles the complaint

        public int? PostId { get; set; } // ID_Пост (FK)

        public int? CommentId { get; set; } // ID_Комментарий (FK)

        [Required(ErrorMessage = "Причина жалобы обязательна")]
        [StringLength(1000)]
        public string Reason { get; set; } = null!;// Причина

        [Required]
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending; // Статус

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Дата_создания - ДОБАВЛЕНО

        public DateTime? ResolvedDate { get; set; } // Дата_решения (если есть)

        [StringLength(1000)]
        public string? ModeratorNotes { get; set; } // Примечания_модератора
    }

    // Настольная_игра/12
    public class BoardGame
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название игры обязательно")]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        public int? ReleaseYear { get; set; }

        public int? MinPlayers { get; set; }

        public int? MaxPlayers { get; set; }

        public TimeSpan? AveragePlayTime { get; set; }

        [StringLength(100)]
        public string? Genre { get; set; }

        public int? Difficulty { get; set; }

        [StringLength(500)]
        [DataType(DataType.Url)]
        public string? ImageUrl { get; set; }
    }

    // Мероприятие/10 (renamed to Event)
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BoardGameId { get; set; } // FK to BoardGame

        [Required(ErrorMessage = "Дата мероприятия обязательна")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Время мероприятия обязательно")]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }

        [Required(ErrorMessage = "Место проведения обязательно")]
        [StringLength(500)]
        public string Location { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }
    }

    // Участник_Мероприятия/11 (renamed to EventParticipant)
    public class EventParticipant
    {
        [Required]
        public int UserId { get; set; } // FK to User

        [Required]
        public int EventId { get; set; } // FK to Event

        [Required(ErrorMessage = "Статус участия обязателен")]
        [StringLength(50)]
        public string ParticipationStatus { get; set; } = null!;

        // Composite primary key:
        // You'll need to configure this in your DbContext using Fluent API:
        // modelBuilder.Entity<EventParticipant>().HasKey(ep => new { ep.UserId, ep.EventId });
    }
	
	    // Участник_Чата - Явная таблица для управления участниками чатов
    public class ChatParticipant
    {
        // Для составных первичных ключей Id обычно не нужен, но если он есть в БД,
        // то EF Core может его использовать как суррогатный ключ.
        // Если вы хотите, чтобы UserId и ChatId были составным первичным ключом,
        // то Id можно удалить, а настройку сделать в DbContext.
        // Я оставлю Id для простоты, предполагая, что вы настроите составной ключ в DbContext.
        // Если вы *не* хотите суррогатный Id, удалите эту строку и добавьте HasKey в DbContext.
        // [Key] // Удалите, если UserId и ChatId - составной ПК
        // public int Id { get; set; }

        [Required]
        // [ForeignKey("Chat")] // Не нужно, так как нет навигационного свойства
        public int ChatId { get; set; } // Внешний ключ к Chat

        [Required]
        // [ForeignKey("User")] // Не нужно, так как нет навигационного свойства
        public int UserId { get; set; } // Внешний ключ к User

        [DataType(DataType.DateTime)]
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    }
    
    // ViewModel для управления статусом администратора конкретного пользователя
    public class UserAdminStatusViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; } = null!; // Соответствует Login в вашей модели User

        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Является администратором")]
        public bool IsAdministrator { get; set; }

        [Display(Name = "Уровень доступа")]
        [Range(1, int.MaxValue, ErrorMessage = "Уровень доступа должен быть больше 0")]
        public int? AccessLevel { get; set; } // Nullable, если пользователь не администратор

        [Display(Name = "Дата назначения")]
        [DataType(DataType.DateTime)]
        public DateTime? AssignmentDate { get; set; } // Nullable, если пользователь не администратор
    }

    // Standard ASP.NET Core Error ViewModel
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}