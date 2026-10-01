using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using HospitalAppointmentManagementSystem.Models;

namespace HospitalAppointmentManagementSystem.Controllers;

public class HomeController : Controller
{
    // Injected HTTP client factory for outbound API requests
    private readonly IHttpClientFactory _clientFactory;

    // Injected in-memory cache instance
    private readonly IMemoryCache _cache;

    // Unique key used to identify the cached doctor list
    private const string DoctorCacheKey = "Hospital_Doctor_Cache";

    // Constructor injecting HTTP client and caching dependencies
    public HomeController(IHttpClientFactory clientFactory, IMemoryCache cache)
    {
        // Store injected HTTP client factory
        _clientFactory = clientFactory;
        // Store injected in-memory cache service
        _cache = cache;
    }

    // Helper method to retrieve or cache doctor data with a 3-minute expiration
    private List<Doctor> GetCachedDoctors()
    {
        // Check if doctor collection exists in memory cache
        if (!_cache.TryGetValue(DoctorCacheKey, out List<Doctor>? doctors))
        {
            // If not found in cache, construct the doctor data set
            doctors = new List<Doctor>
            {
                new Doctor { DoctorId = 1, DoctorName = "Dr. Ajit Sharma", Specialization = "Pulmonology", Experience = 12, ConsultationFee = 800.0 },
                new Doctor { DoctorId = 2, DoctorName = "Dr. Meera Nair", Specialization = "Neurology", Experience = 8, ConsultationFee = 950.0 },
                new Doctor { DoctorId = 3, DoctorName = "Dr. Amrita Joshi", Specialization = "Pediatrics", Experience = 14, ConsultationFee = 500.0 }
            };

            // Define absolute expiration policy of 3 minutes
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

            // Write doctor collection into the in-memory cache
            _cache.Set(DoctorCacheKey, doctors, cacheOptions);
        }

        // Return the doctor list from cache or newly generated list
        return doctors!;
    }

    // GET: / (Doctor Directory)
    public IActionResult Index()
    {
        // Fetch doctor records via memory cache helper
        var doctors = GetCachedDoctors();

        // Send doctor list to Index view
        return View(doctors);
    }

    // GET: /Home/Details?doctorId=1 (Query String and Response Caching)
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public IActionResult Details(int? doctorId)
    {
        // Retrieve doctors from cache
        var doctors = GetCachedDoctors();

        // Locate doctor matching query string doctorId or default to the first
        var selectedDoctor = doctors.FirstOrDefault(d => d.DoctorId == doctorId) ?? doctors.First();

        // Persist doctor's specialization into session state
        HttpContext.Session.SetString("SelectedSpecialization", selectedDoctor.Specialization);

        // Read stored patient name from browser cookie if available
        ViewBag.PatientName = Request.Cookies["PatientName"] ?? string.Empty;

        // Retrieve the specialization string from session state
        ViewBag.Specialization = HttpContext.Session.GetString("SelectedSpecialization");

        // Return details view bound to the selected doctor
        return View(selectedDoctor);
    }

    // POST: /Home/BookAppointment (Form submit with Cookie & Hidden Field)
    [HttpPost]
    public IActionResult BookAppointment(int hiddenDoctorId, string patientName, string appointmentDate)
    {
        // Store patient name inside a client browser cookie with a 7-day lifespan
        Response.Cookies.Append("PatientName", patientName, new CookieOptions
        {
            // Set cookie expiration timestamp
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        // Get doctors to find the booked doctor details
        var doctors = GetCachedDoctors();

        // Retrieve booked doctor using the hidden form field value
        var bookedDoctor = doctors.FirstOrDefault(d => d.DoctorId == hiddenDoctorId);

        // Read current specialization from server session
        string specialization = HttpContext.Session.GetString("SelectedSpecialization") ?? "General";

        // Create flash confirmation message detailing the captured state
        TempData["Message"] = $"Appointment confirmed for {patientName} on {appointmentDate} with {bookedDoctor?.DoctorName} ({specialization})! Preserved DoctorId via Hidden Field: {hiddenDoctorId}.";

        // Redirect back to Index page
        return RedirectToAction("Index");
    }

    // GET: /Home/Posts (Asynchronous Web Request)
    public async Task<IActionResult> Posts()
    {
        // Instantiate HTTP client using factory
        var client = _clientFactory.CreateClient();

        // Remote endpoint URL to retrieve JSON post data
        string targetUrl = "https://jsonplaceholder.typicode.com/posts";

        // Post collection container
        List<ApiPost> posts = new List<ApiPost>();

        try
        {
            // Asynchronously fetch JSON string without blocking the web thread
            string jsonResponse = await client.GetStringAsync(targetUrl);

            // Configure JSON deserializer options
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Deserialize raw JSON string into strongly-typed ApiPost list
            var postResults = JsonSerializer.Deserialize<List<ApiPost>>(jsonResponse, jsonOptions) ?? new List<ApiPost>();

            // Select first 10 posts for presentation
            posts = postResults.Take(10).ToList();
        }
        catch (Exception ex)
        {
            // Capture any error message on network failure
            ViewBag.Error = ex.Message;
        }

        // Return view with deserialized post list
        return View(posts);
    }
}