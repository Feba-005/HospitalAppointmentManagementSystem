namespace HospitalAppointmentManagementSystem.Models;

// Model class representing a medical doctor entity
public class Doctor
{
    // Unique identifier for the doctor
    public int DoctorId { get; set; }

    // Full name of the doctor
    public string DoctorName { get; set; } = string.Empty;

    // Field of clinical specialization
    public string Specialization { get; set; } = string.Empty;

    // Total years of professional medical experience
    public int Experience { get; set; }

    // Standard consultation charge in INR
    public double ConsultationFee { get; set; }
}