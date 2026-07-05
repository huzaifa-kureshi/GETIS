using GETIS.Api.Data;
using GETIS.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GETIS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PackagesController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/packages
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PackageDto>>> GetAll()
    {
        var packages = await _db.Packages
            .Select(p => new PackageDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                DurationDays = p.DurationDays,
                Price = p.Price
            })
            .ToListAsync();

        return Ok(packages);
    }
}
