using System.ComponentModel.DataAnnotations;

namespace course.Models.ViewModels
{

	// ViewModel для управления статусом администратора конкретного пользователя
	public class UserAdminStatusViewModel
	{
		[Required]
		public int UserId { get; set; }

		[Display(Name = "Имя пользователя")]
		public string UserName { get; set; } = null!; // Соответствует Login в вашей модели User

		[Display(Name = "Email")]
		public string Email { get; set; } = null!;

		[Display(Name = "Является администратором")]
		public bool IsAdministrator { get; set; }

		[Display(Name = "Дата назначения")]
		[DataType(DataType.DateTime)]
		public DateTime? AssignmentDate { get; set; } // Nullable, если пользователь не администратор
	}
}