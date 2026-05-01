using BiblioRate.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BiblioRate.Infrastructure.BackgroundJobs
{
   // Infrastructure/BackgroundJobs/NightlyQualityGuardJob.cs
public class NightlyQualityGuardJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public NightlyQualityGuardJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(2); // Her gece 02:00 UTC
            var delay = nextRun - now;

            Console.WriteLine($"[NightlyGuard] Sonraki çalışma: {nextRun:yyyy-MM-dd HH:mm} UTC");
            await Task.Delay(delay, ct);

            using var scope = _scopeFactory.CreateScope();
            var guard = scope.ServiceProvider.GetRequiredService<INightlyQualityGuard>();
            await guard.RunAsync(ct);
        }
    }
}
}