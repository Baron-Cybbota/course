// course.Models.ViewModels.CommentDeleteViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace course.Models.ViewModels
{
    public class CommentDeleteViewModel
    {
        public int IdComment { get; set; }
        public int IdPost { get; set; } // Needed for redirection after delete

        [Display(Name = "Содержание")]
        public string Content { get; set; } = null!;

        [Display(Name = "Дата создания")]
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Пост")]
        public string? PostTitle { get; set; }

        [Display(Name = "Автор")]
        public string? AuthorName { get; set; }
    }
}