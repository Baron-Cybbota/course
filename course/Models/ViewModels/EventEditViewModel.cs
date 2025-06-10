using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList

namespace course.Models.ViewModels // Or course.ViewModels
{
    public class EventEditViewModel
    {
        // This is the primary key of the Event being edited. Crucial for updates.
        public int IdEvent { get; set; } // Match your database model's PK name

        [Required(ErrorMessage = "Название мероприятия обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов.")]
        [Display(Name = "Название мероприятия")]
        public string Name { get; set; } = null!; // Add this required field

        [Display(Name = "Настольная игра")]
        public int? BoardGameId { get; set; } // Match the dropdown binding

        [Required(ErrorMessage = "Дата мероприятия обязательна")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Время мероприятия обязательно")]
        [DataType(DataType.Time)]
        [Display(Name = "Время")]
        public TimeSpan Time { get; set; }

        // This property will hold the address string for the user input
        [Required(ErrorMessage = "Место проведения обязательно")] // Assuming it's required in the form
        [StringLength(1000, ErrorMessage = "Адрес не должен превышать 1000 символов.")]
        [Display(Name = "Место проведения (адрес)")]
        public string LocationAddress { get; set; } = null!;

        // This property will hold the current IdLocation, passed from DB to controller to ViewModel
        // It's used in the controller to resolve the LocationAddress to an actual Location entity.
        public int CurrentIdLocation { get; set; } // To retain the original location ID for updates

        [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов.")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        // For the dropdown list of board games
        public SelectList? BoardGameSelectList { get; set; }
    }
}