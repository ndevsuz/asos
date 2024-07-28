namespace Asos.Helpers;

public static class TimeConstants
{
    private const int Utc = 5;

    public static DateTime GetNow()
        => DateTime.UtcNow.AddHours(Utc);
}