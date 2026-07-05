namespace GETIS.Api.Dtos;

public class BookingResponseDto
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime TravelDate { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }

    public string TravelerName { get; set; } = string.Empty;
    public string TravelerEmail { get; set; } = string.Empty;
    public string PackageTitle { get; set; } = string.Empty;
}
