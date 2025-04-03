namespace SQLExtends.EFCore;

public static class DateTimeExtends
{
    public static TimeZoneInfo TimeZoneDefault { get; set; } = OperatingSystem.IsWindows() ? TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time") : TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTime ToTimeZone(this DateTime dateTime)
    {
        return TimeZoneInfo.ConvertTime(dateTime, TimeZoneDefault);
    }
}