using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class Post
    {
        public int Id { get; set; }

        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Заголовок поста обязателен")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Заголовок должен быть от 5 до 200 символов")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Содержание поста обязательно")]
        [DataType(DataType.Html)]
        public string Content { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsEdited { get; set; } = false;
        public DateTime? LastEditedDate { get; set; }

        public bool IsHidden { get; set; } = false;
    }
}