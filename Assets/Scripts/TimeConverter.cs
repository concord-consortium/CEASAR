using System;

// Adapted from https://www.codeproject.com/Articles/459441/Sidreal-Time-Calculator
public static class TimeConverter
{
    private static DateTime GregorianReformDate = new DateTime(1582, 10, 15, 0, 0, 0);

    static double ToFractionalDay(this TimeSpan sourceTime)
    {
        return sourceTime.TotalHours / 24d;
    }

    static double ToFractionalDay(this DateTime sourceDate)
    {
        return sourceDate.TimeOfDay.ToFractionalDay();
    }

    static TimeSpan ToTimeOfDay(this double fractionalDay)
    {
        fractionalDay -= Math.Floor(fractionalDay);
        var totalHours = fractionalDay * 24;
        int hours = (int)Math.Floor(totalHours);
        var totalMinutes = (totalHours - hours) * 60; ;
        int minutes = (int)Math.Floor(totalMinutes);
        var totalSeonds = (totalMinutes - minutes) * 60;
        int seconds = (int)Math.Floor(totalSeonds);
        TimeSpan retVal = new TimeSpan(hours, minutes, seconds);
        return retVal;
    }

    static TimeSpan ToHMS(this double degrees)
    {
        int hours = (int)Math.Floor(degrees);
        double totalMinutes = 60 * (degrees - hours);
        int minutes = (int)Math.Floor(totalMinutes);
        double totalSeconds = 60 * (totalMinutes - minutes);
        TimeSpan retVal = new TimeSpan(hours, minutes, (int)totalSeconds);
        return retVal;

    }

    //http://www.j2i.net/blogEngine/post/2011/10/21/Modified-Julian-Date.aspx
    public static double ToModifiedJulianDate(this DateTime sourceTime)
    {
        int calcMonth, calcYear, calcDay;

        calcDay = sourceTime.Day;
        if (sourceTime.Month < 2)
        {
            calcMonth = sourceTime.Month + 12;
            calcYear = sourceTime.Year - 1;
        }
        else
        {
            calcMonth = sourceTime.Month;
            calcYear = sourceTime.Year;
        }
        var leapDays = (calcYear / 400) - (calcYear / 100) + (calcYear / 4);
        var mjd = 365L * calcYear - 679004L + leapDays + (int)(30.6001 * (calcMonth + 1)) + calcDay;
        return mjd + ToFractionalDay(sourceTime.TimeOfDay);
    }

    public static double ToJulianDate(this DateTime sourceDate)
    {
        return getJulianDate(sourceDate);
    }

    private static double getJulianDate(DateTime sourceDate)
    {
        double y, m, c;
        if (sourceDate.Month <= 2)
        {
            y = sourceDate.Year - 1;
            m = sourceDate.Month + 2;
        }
        else
        {
            y = sourceDate.Year;
            m = sourceDate.Month;
        }

        double leapDayCount = (sourceDate > GregorianReformDate) ? (2 - Math.Floor(y / 100) + Math.Floor(y / 400)) : 0;
        if (sourceDate.Year < 0)
        {
            c = (int)(365.25 * (double)sourceDate.Year - 0.75);
        }
        else
        {
            c = (int)(365.25 * (double)sourceDate.Year);
        }
        double d = Math.Floor(30.6001 * (m + 1));
        var retVal = leapDayCount + c + d + sourceDate.Day + 1720994.5;
        return retVal + sourceDate.ToFractionalDay();
    }

    public static DateTime JulianToCalendarDate(this double sourceJulianDate)
    {
        sourceJulianDate += 0.5;
        double integerPart = Math.Floor(sourceJulianDate);
        double fractionalPart = sourceJulianDate - integerPart;
        double b;
        if (sourceJulianDate > 2299160)
        {
            var a = (integerPart - 1867216.25) / 36524.25;
            b = integerPart + a - Math.Floor(a / 4) + 1;
        }
        else
            b = integerPart;
        double c = (b + 1524);
        double d = Math.Floor((c - 122.1) / 365.25);
        double e = Math.Floor(365.25 * d);
        double g = Math.Floor((c - e) / 30.6001);
        double day = c - e + fractionalPart - Math.Floor(30.6001 * g);
        int month, year;
        if (g < 13.5)
            month = (int)(g - 1);
        else
            month = (int)(g - 13);
        if (month > 2.5)
            year = (int)(d - 4716);
        else
            year = (int)(d - 4715);

        DateTime retVal = new DateTime(year, month, (int)day).Add(sourceJulianDate.ToTimeOfDay());
        return retVal;

    }

    public static DateTime ModifiedJulianToCalendarDate(this double source)
    {
        DateTime modifiedDate = JulianToCalendarDate(source + 2400000.5);
        return modifiedDate;
    }

    public static double ToSiderealTime(this DateTime date)
    {
        DateTime d = date.Date;
        double julianDate = getJulianDate(d);
        double s = julianDate - 2451545;
        var t = s / 36525;
        double t0 = 6.697374558 + (2400.051336 * t) + (0.000025862 * t * t);
        while (t0 > 24)
        {
            t0 -= 24;
        }
        while (t0 < 0)
        {
            t0 += 24;
        }
        double time = 24 * date.ToFractionalDay();
        time *= 1.002737909;
        t0 += time;
        while (t0 > 24)
        {
            t0 -= 24;
        }
        return t0;
    }
}