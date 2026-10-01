namespace HospitalAppointmentManagementSystem.Models;

// Static container class for custom Doctor extension methods
public static class DoctorExtensions
{
    // Extension method to compute total charges with an added flat ₹100 service fee
    public static double CalculateTotalCharge(this Doctor doc)
    {
        // Add flat ₹100 service fee to the base consultation fee
        return doc.ConsultationFee + 100.0;
    }
}  