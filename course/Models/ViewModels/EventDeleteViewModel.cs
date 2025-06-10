using System;
using System.ComponentModel.DataAnnotations;

namespace course.Models.ViewModels // Or wherever you keep your ViewModels
{
    public class EventDeleteViewModel
    {
        // Primary key of the Event being deleted (for the form submission)
        public int IdEvent { get; set; }

        // Properties directly from the Event model
        [Display(Name = "Название мероприятия")] // Added Name property
        public string Name { get; set; } = null!;

        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Время")]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        // Properties for related data (BoardGame Title and Location Address)
        [Display(Name = "Настольная игра")]
        public string? BoardGameTitle { get; set; }

        [Display(Name = "Место проведения")]
        public string? LocationAddress { get; set; } // To display the actual address
    }
}