using Microsoft.AspNetCore.Mvc;
using course.Data; // Убедитесь, что ваш DbContext находится в этом пространстве имен
using course.Models;
// Removed: using course.Models.ViewModels; // No longer needed for this approach
using System.Linq;
using System.Collections.Generic;
using System;

public class LocationController : Controller
{
    private readonly ApplicationDbContext _context;

    public LocationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Страница с формой + карта
    [HttpGet]
    public IActionResult Select()
    {
        // No longer passing a ViewModel to the View, if we don't have existing locations.
        // If you need to pre-fill the form, use ViewBag here.
        // Example:
        // ViewBag.LocationName = "Default Address";
        // ViewBag.Latitude = 53.9;
        // ViewBag.Longitude = 27.5667;

        // If you were previously displaying existing locations on the map
        // from the model, you'd now need to fetch them and pass via ViewBag
        // or a dedicated API endpoint for map markers.
        // As per previous request, existinglocations are not needed on the view.

        return View(); // Return the view without a model
    }

    [HttpPost]
    public IActionResult SaveLocation(string Address, double Latitude, double Longitude)
    {
        // Basic server-side validation can still be done here
        if (string.IsNullOrWhiteSpace(Address))
        {
            ModelState.AddModelError("Address", "Адрес обязателен.");
        }
        // Add more validation for Latitude and Longitude if needed

        if (!ModelState.IsValid)
        {
            // If validation fails, return to the view, re-populating fields via ViewBag
            ViewBag.Address = Address;
            ViewBag.Latitude = Latitude;
            ViewBag.Longitude = Longitude;
            return View("Select");
        }

        var existing = _context.Locations
            .FirstOrDefault(l => l.Address.ToLower() == Address.ToLower());

        if (existing != null)
        {
            ModelState.AddModelError("Address", "Такая локация уже существует. Выберите её на карте.");
            ViewBag.Address = Address; // Repopulate fields on error
            ViewBag.Latitude = Latitude;
            ViewBag.Longitude = Longitude;
            return View("Select");
        }

        var location = new Location
        {
            Address = Address,
            Latitude = Latitude,
            Longitude = Longitude
        };

        _context.Locations.Add(location);
        _context.SaveChanges();

        // Optionally, set a success message via ViewBag before redirecting or returning view
        TempData["Message"] = "Локация успешно сохранена!"; // Use TempData for redirect scenarios
        return RedirectToAction("Select");
    }

    [HttpPost]
    public IActionResult GetNearbyLocations(double latitude, double longitude)
    {
        const double radiusKm = 1.0;

        var locations = _context.Locations.ToList();

        var nearby = locations
            .Where(loc => GetDistance(latitude, longitude, loc.Latitude, loc.Longitude) <= radiusKm)
            .Select(loc => new {
                loc.Address,
                loc.Latitude,
                loc.Longitude
            }).ToList();

        return Json(nearby);
    }

    // Haversine formula (без изменений, так как работает с широтой и долготой)
    private double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371; // Earth radius in km
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double DegreesToRadians(double deg) => deg * Math.PI / 180;
}