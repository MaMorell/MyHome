using Cronos;

namespace MyHome.ApiService.Extensions;

public static class CronExpressionExtensions
{
    public static TimeSpan GetTimeUntilNextOccurrence(this CronExpression cronExpression)
    {
        var utcNow = DateTime.UtcNow;
        var next = cronExpression.GetNextOccurrence(utcNow)
            ?? throw new InvalidOperationException("Failed to calculate next occurrence time");

        var delay = next - utcNow;

        return delay;
    }
}