using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace course.Models
{
    
    public class User
    {
        [Key]
        public int IdUser { get; set; } 

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

    
    public class Administrator
    {
        [Key]
        
        public int IdAdministrator { get; set; } 

        [Required]
         
        public int IdUser { get; set; }

        [Required(ErrorMessage = "Дата назначения обязательна")]
        [DataType(DataType.DateTime)]
        
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow; 
    }

    
    public class BlacklistEntry
    {
        [Key]
        
        public int IdBlacklist { get; set; } 

        [Required]
         
        public int IdAdministrator { get; set; }

        [Required]
         
        public int IdUser { get; set; }

        [Required(ErrorMessage = "Причина блокировки обязательна")]
        [StringLength(500)]
        
        public string Reason { get; set; } = null!; 

        [DataType(DataType.DateTime)]
        
        public DateTime BlockDate { get; set; } = DateTime.UtcNow; 

        
        
        
        
        
        public TimeSpan? BlockDuration { get; set; } 
    }

    
    public class Message
    {
        [Key]
        
        public int IdMessage { get; set; } 

        
        
        [Required]
         
        public int IdUser { get; set; }

         
        public int? IdEvent { get; set; }

        [Required(ErrorMessage = "Содержание сообщения обязательно")]
        [StringLength(4000)]
        
        public string Content { get; set; } = null!; 

        [DataType(DataType.DateTime)]
        
        public DateTime SendDate { get; set; } = DateTime.UtcNow; 

        [Required]
        
        public bool IsRead { get; set; } = false; 
    }

    
    public class Post
    {
        [Key]
        
        public int IdPost { get; set; } 

        [Required]
         
        public int IdUser { get; set; }

        
         
        public int? IdEvent { get; set; }

        [Required(ErrorMessage = "Заголовок поста обязателен")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Заголовок должен быть от 2 до 200 символов")]
        
        public string Title { get; set; } = null!; 

        [Required(ErrorMessage = "Содержание поста обязательно")]
        [DataType(DataType.Html)]
        
        public string Content { get; set; } = null!; 

        [DataType(DataType.DateTime)]
        
        public DateTime CreationDate { get; set; } = DateTime.UtcNow; 

        [DataType(DataType.DateTime)]
        
        public DateTime? EditDate { get; set; } 

        
        public bool IsHidden { get; set; } = false; 

        
        public int Rating { get; set; } = 0; 
    }

    
    public class Comment
    {
        [Key]
        
        public int IdComment { get; set; } 

        [Required]
         
        public int IdPost { get; set; }

        [Required]
         
        public int IdUser { get; set; }

        [Required(ErrorMessage = "Содержание комментария обязательно")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Комментарий должен быть от 1 до 2000 символов")]
        
        public string Content { get; set; } = null!; 

        [DataType(DataType.DateTime)]
        
        public DateTime CreationDate { get; set; } = DateTime.UtcNow; 

        [DataType(DataType.DateTime)]
        
        public DateTime? EditDate { get; set; } 

        
        public int Rating { get; set; } = 0; 
    }

    
    public class Rating
    {
        [Key]
        
        public int IdRating { get; set; } 

        [Required]
         
        public int IdUser { get; set; }

        
         
        public int? IdPost { get; set; }

         
        public int? IdComment { get; set; }

        
        
        [Required]
        
        public bool Value { get; set; } 
    }

    
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
        
        public int IdComplaint { get; set; } 

        [Required]
         
        public int IdUser { get; set; }

         
        public int? IdAdministrator { get; set; }

         
        public int? IdPost { get; set; }

         
        public int? IdComment { get; set; }

         
        public int? IdEvent { get; set; }
        
         
        public int? IdMessage { get; set; }

        [Required(ErrorMessage = "Причина жалобы обязательна")]
        [StringLength(1000)]
        
        public string Reason { get; set; } = null!; 

        [Required]
        
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending; 

        [DataType(DataType.DateTime)]
        
        public DateTime CreationDate { get; set; } = DateTime.UtcNow; 

        [DataType(DataType.DateTime)]
        
        public DateTime? ResolutionDate { get; set; } 

        [StringLength(1000)]
         
        public string? ModeratorNotes { get; set; } 
    }

    
    public class BoardGame
    {
        [Key]
        
        public int IdBoardGame { get; set; } 

        [Required(ErrorMessage = "Название игры обязательно")]
        [StringLength(200)]
        
        public string Name { get; set; } = null!; 

        [StringLength(2000)]
        
        public string? Description { get; set; } 

        
        public int? ReleaseYear { get; set; } 

        
        public int? MinPlayers { get; set; } 

        
        public int? MaxPlayers { get; set; } 

        
        public TimeSpan? EstimatedPlayTime { get; set; } 

        [StringLength(100)]
        
        public string? Genre { get; set; } 

        
        public int? Difficulty { get; set; } 

        
        
        
        
    }

    
    public class Event
    {
        [Key]
        
        public int IdEvent { get; set; } 

         
        public int? IdBoardGame { get; set; }

        [Required]
         
        public int IdLocation { get; set; }

        [Required(ErrorMessage = "Название мероприятия обязательно")]
        [StringLength(200)]
        
        public string Name { get; set; } = null!; 

        [Required(ErrorMessage = "Дата мероприятия обязательна")]
        [DataType(DataType.Date)]
        
        public DateTime Date { get; set; } 

        [Required(ErrorMessage = "Время мероприятия обязательно")]
        [DataType(DataType.Time)]
        
        public TimeSpan Time { get; set; } 

        [StringLength(2000)]
        
        public string? Description { get; set; } 
    }

    
    public class EventParticipant
    {
        public int IdUser { get; set; }

         
        public int IdEvent { get; set; }

        [Required(ErrorMessage = "Статус участия обязателен")]
        [StringLength(50)]
        
        public string ParticipationStatus { get; set; } = null!; 
    }

    
    public class Image
    {
        [Key]
        
        public int IdImage { get; set; } 

        
        
         
        public int? IdPost { get; set; }

         
        public int? IdEvent { get; set; }

         
        public int? IdBoardGame { get; set; }

        [Required(ErrorMessage = "URL изображения обязателен")]
        [StringLength(500)]
        [DataType(DataType.Url)]
        
        public string ImageUrl { get; set; } = string.Empty; 
    }
    public class Location
    {
        [Key]
        public int IdLocation { get; set; } 
        [Required(ErrorMessage = "Адрес локации обязателен")]
        [StringLength(1000)]
        public string Address { get; set; } = null!; 
        [Required]
        public double Latitude { get; set; } 
        [Required]
        public double Longitude { get; set; } 
    }

    
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}