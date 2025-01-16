namespace SQLExtends.EFCore;

public static class DateTimeExtends
{
    public static TimeZoneInfo TimeZoneDefault { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

    public static DateTime ToTimeZone(this DateTime dateTime)
    {
        return TimeZoneInfo.ConvertTime(dateTime, TimeZoneDefault);
    }
}