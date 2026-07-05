using GETIS.Api.Models;

namespace GETIS.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (!db.Packages.Any())
        {
            db.Packages.AddRange(
                new Package
                {
                    Title = "The Adventure Seeker",
                    Description = "Includes flights, 4-star lodging, guided hiking, and city tours. Perfect for active travelers.",
                    DurationDays = "7 Days / 6 Nights",
                    Price = 1999m
                },
                new Package
                {
                    Title = "Family / Group Package",
                    Description = "Comfortable hotels, guided group activities, family-friendly tours, and some meals included.",
                    DurationDays = "6 Days / 5 Nights",
                    Price = 899m
                },
                new Package
                {
                    Title = "The Luxury Escape",
                    Description = "Private transfer, 5-star resort, personal chef, and spa treatments included.",
                    DurationDays = "5 Days / 4 Nights",
                    Price = 4500m
                }
            );
            db.SaveChanges();
        }
    }
}
