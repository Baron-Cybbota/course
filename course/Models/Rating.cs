using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? PostId { get; set; }

        public int? CommentId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Значение оценки должно быть от 1 до 5")]
        public int Value { get; set; }
    }
}