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

    public static bool IsDayTime(this DateTime date)
    {
        var time = TimeOnly.FromDateTime(date);
        var dayStart = new TimeOnly(7, 0);
        var dayEnd = new TimeOnly(19, 0);

        return time.IsBetween(dayStart, dayEnd);
    }

    public static bool IsNightTime(this DateTime date)
    {
        var time = TimeOnly.FromDateTime(date);
        var start = new TimeOnly(0, 0);
        var end = new TimeOnly(4, 0);

        return time.IsBetween(start, end);
    }

    public static bool IsMidDay(this DateTime date)
    {
        var time = TimeOnly.FromDateTime(date);
        var start = new TimeOnly(10, 0);
        var end = new TimeOnly(15, 0);

        return time.IsBetween(start, end);
    }

    public static bool IsWeekdayDayTime(this DateTime date) => date.IsWeekday() && date.IsDayTime();
    public static bool IsWeekdayMidDay(this DateTime date) => date.IsWeekday() && date.IsMidDay();
}