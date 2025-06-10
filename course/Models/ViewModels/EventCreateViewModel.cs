using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList

namespace course.Models.ViewModels // Or course.ViewModels
{
    public class EventCreateViewModel
    {
        [Required(ErrorMessage = "Название мероприятия обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов.")]
        [Display(Name = "Название мероприятия")]
        public string Name { get; set; } = null!; // Add the missing Name field

        [Display(Name = "Настольная игра")]
        public int? BoardGameId { get; set; } // Use BoardGameId as in the view

        [Required(ErrorMessage = "Дата мероприятия обязательна")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Время мероприятия обязательно")]
        [DataType(DataType.Time)]
        [Display(Name = "Время")]
        public TimeSpan Time { get; set; }

        // This is the property for the *user-entered* location string
        [Required(ErrorMessage = "Место проведения обязательно")] // Assuming it's required in the form
        [StringLength(1000, ErrorMessage = "Адрес не должен превышать 1000 символов.")]
        [Display(Name = "Место проведения (адрес)")]
        public string LocationAddress { get; set; } = null!; // Renamed to avoid confusion with IdLocation

        [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов.")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        // This is for the dropdown in the view
        public SelectList? BoardGameSelectList { get; set; }
    }
}