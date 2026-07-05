using System.Text;
using GETIS.Api.Data;
using GETIS.Api.Dtos;
using GETIS.Api.Filters;
using GETIS.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GETIS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingsController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/bookings  (admin only)
    [AdminAuth]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetAll()
    {
        var bookings = await _db.Bookings
            .Include(b => b.Traveler)
            .Include(b => b.Package)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return Ok(bookings.Select(ToDto));
    }

    // GET api/bookings/5  (admin only)
    [AdminAuth]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponseDto>> GetById(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.Traveler)
            .Include(b => b.Package)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();
        return Ok(ToDto(booking));
    }

    // POST api/bookings  (public - anyone can submit a booking)
    [HttpPost]
    public async Task<ActionResult<BookingResponseDto>> Create(BookingCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Destination) ||
            dto.TravelDate == default)
        {
            return BadRequest(new { error = "Please complete all required fields." });
        }

        var package = await _db.Packages.FindAsync(dto.PackageId);
        if (package == null)
        {
            return BadRequest(new { error = "Selected package does not exist." });
        }

        var traveler = await _db.Travelers.FirstOrDefaultAsync(t => t.Email == dto.Email);
        if (traveler == null)
        {
            traveler = new Traveler
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone
            };
            _db.Travelers.Add(traveler);
        }
        else
        {
            traveler.FullName = dto.FullName;
            traveler.Phone = dto.Phone ?? traveler.Phone;
        }

        var booking = new Booking
        {
            BookingReference = $"BK-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            TravelDate = dto.TravelDate,
            Destination = dto.Destination,
            Status = "Pending",
            TotalCost = package.Price,
            Traveler = traveler,
            Package = package
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, ToDto(booking));
    }

    // DELETE api/bookings/5  (admin only)
    [AdminAuth]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET api/bookings/export/csv  (admin only)
    [AdminAuth]
    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv()
    {
        var bookings = await _db.Bookings
            .Include(b => b.Traveler)
            .Include(b => b.Package)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Reference,Name,Email,TravelDate,Destination,Package,TotalCost,Status,BookingDate");
        foreach (var b in bookings)
        {
            sb.AppendLine(string.Join(",",
                b.BookingReference,
                b.Traveler.FullName,
                b.Traveler.Email,
                b.TravelDate.ToString("yyyy-MM-dd"),
                b.Destination,
                b.Package.Title,
                b.TotalCost,
                b.Status,
                b.BookingDate.ToString("yyyy-MM-dd")));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "bookings.csv");
    }

    // GET api/bookings/report/by-month  (admin only)
    [AdminAuth]
    [HttpGet("report/by-month")]
    public async Task<ActionResult<object>> ReportByMonth()
    {
        var grouped = await _db.Bookings
            .GroupBy(b => new { b.TravelDate.Year, b.TravelDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        return Ok(grouped);
    }

    private static BookingResponseDto ToDto(Booking b) => new()
    {
        Id = b.Id,
        BookingReference = b.BookingReference,
        BookingDate = b.BookingDate,
        TravelDate = b.TravelDate,
        Destination = b.Destination,
        Status = b.Status,
        TotalCost = b.TotalCost,
        TravelerName = b.Traveler.FullName,
        TravelerEmail = b.Traveler.Email,
        PackageTitle = b.Package.Title
    };
}
