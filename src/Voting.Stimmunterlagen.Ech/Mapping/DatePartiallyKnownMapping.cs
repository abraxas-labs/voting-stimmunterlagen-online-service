// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Globalization;
using Ech0044_4_1;

namespace Voting.Stimmunterlagen.Ech.Mapping;

// there are 3 types of date partially known:
// year (ex: "1995")
// year month (ex: "1995-09")
// year month day (ex: "1995-09-21")
public static class DatePartiallyKnownMapping
{
    public const string YearMonthDayFormat = "yyyy-MM-dd";
    public const string YearMonthFormat = "yyyy-MM";
    public const string UnspecifiedDateString = "0";

    public static string ToDateString(this DatePartiallyKnownType datePartiallyKnown)
    {
        if (!string.IsNullOrEmpty(datePartiallyKnown.Year))
        {
            return datePartiallyKnown.Year;
        }

        if (!string.IsNullOrEmpty(datePartiallyKnown.YearMonth))
        {
            return datePartiallyKnown.YearMonth;
        }

        if (datePartiallyKnown.YearMonthDayValueSpecified)
        {
            return datePartiallyKnown.YearMonthDay!.Value.ToString(YearMonthDayFormat);
        }

        throw new InvalidOperationException($"No data found in ${nameof(DatePartiallyKnownType)}");
    }

    public static DateTime ToDateTime(this string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            throw new InvalidOperationException($"Cannot create ${nameof(DateTime)} with an empty string");
        }

        if (short.TryParse(dateString, out var year))
        {
            return new DateTime(year);
        }

        if (DateTime.TryParseExact(dateString, YearMonthDayFormat, null, DateTimeStyles.None, out var yearMonthDay))
        {
            return yearMonthDay;
        }

        // year month variant
        if (DateTime.TryParseExact(dateString, YearMonthFormat, null, DateTimeStyles.None, out var yearMonth))
        {
            return yearMonth;
        }

        throw new InvalidOperationException($"Cannot create ${nameof(DateTime)} with string:'{dateString}'");
    }

    public static DatePartiallyKnownType ToEchDatePartiallyKnown(this string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            throw new InvalidOperationException($"Cannot create ${nameof(DatePartiallyKnownType)} with an empty string");
        }

        if (short.TryParse(dateString, out var year))
        {
            return new DatePartiallyKnownType { Year = year.ToString() };
        }

        if (DateTime.TryParseExact(dateString, YearMonthDayFormat, null, DateTimeStyles.None, out var yearMonthDay))
        {
            return new DatePartiallyKnownType { YearMonthDay = yearMonthDay };
        }

        // year month variant
        return new DatePartiallyKnownType { YearMonth = dateString };
    }
}
