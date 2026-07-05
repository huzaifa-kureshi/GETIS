namespace GETIS.Api.Dtos;

public class PackageDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DurationDays { get; set; }
    public decimal Price { get; set; }
}
