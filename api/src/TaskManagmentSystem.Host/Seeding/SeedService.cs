using Bogus;

namespace TaskManagementSystem.Host.Seeding;

public class SeedService(
    ILogger<SeedService> logger)
{

    public void SeedData()
    {
	    Randomizer.Seed = new Random(4503);

        logger.LogInformation("Starting data seeding...");

        logger.LogInformation("Data seeding completed");
    }
}
