namespace GETIS.Api.Models;

public class Booking
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public DateTime TravelDate { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal TotalCost { get; set; }

    public int TravelerId { get; set; }
    public Traveler Traveler { get; set; } = null!;

    public int PackageId { get; set; }
    public Package Package { get; set; } = null!;
}
