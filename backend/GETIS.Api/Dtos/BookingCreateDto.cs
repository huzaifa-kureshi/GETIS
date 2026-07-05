namespace GETIS.Api.Dtos;

public class BookingCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime TravelDate { get; set; }
    public string Destination { get; set; } = string.Empty;
    public int PackageId { get; set; }
}
