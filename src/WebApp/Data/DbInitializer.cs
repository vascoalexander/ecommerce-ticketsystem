namespace WebApp.Data;

public static class DbInitializer
{
    public static void SeedDb(AppDbContext context)
    {
        context.SaveChanges();
    }
}