using System.ComponentModel.DataAnnotations;

namespace course.Models.ViewModels
{
	public class PostViewModel
	{
		public int IdPost { get; set; }
		public int IdUser { get; set; }
		public int? IdEvent { get; set; }

		[Required, StringLength(200, MinimumLength = 2)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Content { get; set; } = string.Empty;

		public DateTime CreationDate { get; set; }
		public DateTime? EditDate { get; set; }
		public bool IsHidden { get; set; }
		public int Rating { get; set; }

		// Дополнительно:
		public string? UserLogin { get; set; }
		public string? EventName { get; set; }
	}

}
