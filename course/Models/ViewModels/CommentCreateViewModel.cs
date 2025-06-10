// course.Models.ViewModels.CommentCreateViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList

namespace course.Models.ViewModels
{
    public class CommentCreateViewModel
    {
        [Required(ErrorMessage = "Выберите публикацию")]
        [Display(Name = "Публикация")]
        public int IdPost { get; set; }

        [Required(ErrorMessage = "Выберите пользователя")]
        [Display(Name = "Автор")]
        public int IdUser { get; set; }

        [Required(ErrorMessage = "Содержание комментария обязательно")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Комментарий должен быть от 1 до 2000 символов")]
        [Display(Name = "Содержание")]
        public string Content { get; set; } = null!;

        public SelectList? PostsSelectList { get; set; }
        public SelectList? UsersSelectList { get; set; }
    }
}