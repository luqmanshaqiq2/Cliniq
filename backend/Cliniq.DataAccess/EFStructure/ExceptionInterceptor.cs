
using Microsoft.EntityFrameworkCore.Diagnostics;

public sealed class ExceptionInterceptor : SaveChangesInterceptor
{
    public override void SaveChangesFailed(
        DbContextErrorEventData eventData)
    {
        Console.WriteLine(
            $"Failed to save changes: {eventData.Exception.Message}");

        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Failed to save changes: {eventData.Exception.Message}");

        return base.SaveChangesFailedAsync(
            eventData, cancellationToken);
    }
}
