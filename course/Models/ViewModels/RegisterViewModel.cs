using System.ComponentModel.DataAnnotations;

namespace course.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Поле 'Логин' обязательно.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов.")]
        [Display(Name = "Логин")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "Поле 'Email' обязательно.")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email.")]
        [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Поле 'Пароль' обязательно.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
