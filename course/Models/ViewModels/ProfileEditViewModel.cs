using System.ComponentModel.DataAnnotations;

namespace course.Models.ViewModels
{
    public class ProfileEditViewModel
    {
        public int Id { get; set; } // ID пользователя, который будет редактироваться

        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Логин должен быть от 1 до 50 символов")]
        [Display(Name = "Логин")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Статус блокировки")]
        public bool BlockStatus { get; set; } // Пользователь может видеть, но не редактировать в личном кабинете

        [Display(Name = "Рейтинг")]
        public int Rating { get; set; } // Пользователь может видеть, но не редактировать

        [Display(Name = "Дата регистрации")]
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; } // Пользователь может видеть, но не редактировать
    }
}
