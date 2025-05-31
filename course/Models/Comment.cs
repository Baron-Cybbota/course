using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Содержание комментария обязательно")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Комментарий должен быть до 2000 символов")]
        public string Content { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsEdited { get; set; } = false;
        public DateTime? LastEditedDate { get; set; }
    }
}