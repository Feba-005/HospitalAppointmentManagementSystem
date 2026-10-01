namespace HospitalAppointmentManagementSystem.Models;

// Model class to map JSON payload from remote web request
public class ApiPost
{
    // Unique identifier of the remote post
    public int Id { get; set; }

    // Title string returned by the JSON API
    public string Title { get; set; } = string.Empty;
}