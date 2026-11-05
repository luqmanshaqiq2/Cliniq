using Cliniq.DataAccess.EFStructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cliniq.DataAccess.Initialization;

public static class SampleDataInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Patients.AnyAsync(cancellationToken)
            || await dbContext.Doctors.AnyAsync(cancellationToken))
        {
            return;
        }

        var sampleData = SampleData.Create();
        dbContext.Doctors.AddRange(sampleData.Doctors);
        dbContext.Patients.AddRange(sampleData.Patients);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
