namespace MyHome.Core.Extensions;

public static class DateTimeExtensions
{
    public static bool IsWeekend(this DateTime now)
    {
        return
            now.DayOfWeek == DayOfWeek.Saturday ||
            now.DayOfWeek == DayOfWeek.Sunday;
    }

    public static bool IsWeekday(this DateTime now)
    {
        return
            now.DayOfWeek == DayOfWeek.Monday ||
            now.DayOfWeek == DayOfWeek.Tuesday ||
            now.DayOfWeek == DayOfWeek.Wednesday ||
            now.DayOfWeek == DayOfWeek.Thursday ||
            now.DayOfWeek == DayOfWeek.Friday;
    }
}
