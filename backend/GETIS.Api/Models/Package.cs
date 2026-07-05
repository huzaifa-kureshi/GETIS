namespace GETIS.Api.Models;

public class Package
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DurationDays { get; set; }
    public decimal Price { get; set; }

    public List<Booking> Bookings { get; set; } = new();
}
