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

    public static bool IsWeekdayMidDay(this DateTime date)
    {
        var time = TimeOnly.FromDateTime(date);
        var weekDayMidDayStart = new TimeOnly(10, 0);
        var weekDayMidDayEnd = new TimeOnly(15, 0);

        return
            date.IsWeekday() &&
            time.IsBetween(weekDayMidDayStart, weekDayMidDayEnd);
    }

    public static bool IsNightTime(this DateTime date)
    {
        var time = TimeOnly.FromDateTime(date);
        var nightStart = new TimeOnly(1, 0);
        var nightEnd = new TimeOnly(4, 0);

        return time.IsBetween(nightStart, nightEnd);
    }

    public static bool IsWithinWorkingHours(this DateTime date)
    {
        var now = DateTime.Now;

        if (now.IsWeekend())
        {
            return false;
        }

        var workingHoursStart = TimeSpan.FromHours(7);
        var workingHoursEnd = TimeSpan.FromHours(19);

        return 
            now.TimeOfDay >= workingHoursStart && 
            now.TimeOfDay < workingHoursEnd;
    }
}