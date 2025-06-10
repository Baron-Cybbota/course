using System.ComponentModel.DataAnnotations;

namespace course.Models.ViewModels
{
	public class EventViewModel
{
    public int IdEvent { get; set; }
    public int? IdBoardGame { get; set; }
    public int IdLocation { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required, DataType(DataType.Time)]
    public TimeSpan Time { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    // Дополнительно:
    public string? LocationAddress { get; set; }
    public string? BoardGameName { get; set; }
}


}