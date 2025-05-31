using System;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
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
        public int Id { get; set; }

        public int AuthorId { get; set; }

        public int? PostId { get; set; }

        public int? CommentId { get; set; }

        [Required(ErrorMessage = "Причина жалобы обязательна")]
        [StringLength(1000)]
        public string Reason { get; set; }

        [Required]
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? HandledByModeratorId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ResolvedDate { get; set; }

        [StringLength(1000)]
        public string? ModeratorNotes { get; set; }
    }
}