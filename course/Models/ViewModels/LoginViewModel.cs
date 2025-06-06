using System.ComponentModel.DataAnnotations;

namespace course.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле 'Логин или Email' обязательно.")]
        [Display(Name = "Логин или Email")]
        public string LoginOrEmail { get; set; } = null!;

        [Required(ErrorMessage = "Поле 'Пароль' обязательно.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
