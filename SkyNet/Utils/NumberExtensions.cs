using System;

namespace SkyNet
{
    public static class NumberExtensions
    {
        public static string FormatBytes<T>(this T input, bool shortFormat) where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            ulong num1 = (ulong)Convert.ChangeType(input, typeof(ulong));
            string str;
            double num2;
            if (num1 >= 1152921504606846976UL)
            {
                str = "eb";
                num2 = (num1 >> 50);
            }
            else if (num1 >= 1125899906842624UL)
            {
                str = "pb";
                num2 = (num1 >> 40);
            }
            else if (num1 >= 1099511627776UL)
            {
                str = "tb";
                num2 = (num1 >> 30);
            }
            else if (num1 >= 1073741824UL)
            {
                str = "gb";
                num2 = (num1 >> 20);
            }
            else if (num1 >= 1048576UL)
            {
                str = "mb";
                num2 = (num1 >> 10);
            }
            else
            {
                if (num1 < 1024UL)
                    return num1.ToString("0b");
                str = "kb";
                num2 = num1;
            }
            return (num2 / 1024.0).ToString(!shortFormat ? "0.00" : "0") + str;
        }

        public static T Clamp<T>(this T input, T min, T max) where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            if (input.CompareTo(min) < 0)
                return min;
            if (input.CompareTo(max) > 0)
                return max;
            return input;
        }

        public static string FormatSeconds(this ulong i)
        {
            return ((long)i).FormatSeconds();
        }

        public static string FormatSeconds(this long s)
        {
            double num1 = Math.Floor(s / 60.0);
            double num2 = Math.Floor(num1 / 60.0);
            double num3 = Math.Floor(num2 / 24.0);
            double num4 = Math.Floor(num3 / 7.0);
            if (s < 60L)
                return string.Format("{0}s", s);
            if (num1 < 60.0)
                return string.Format("{1}m{0}s", (s % 60L), num1, num2, num3, num4);
            if (num2 < 48.0)
                return string.Format("{2}h{1}m{0}s", (s % 60L), (num1 % 60.0), num2, num3, num4);
            if (num3 < 7.0)
                return string.Format("{3}d{2}h{1}m{0}s", (s % 60L), (num1 % 60.0), (num2 % 24.0), (num3 % 7.0), num4);
            return string.Format("{4}w{3}d{2}h{1}m{0}s", (s % 60L), (num1 % 60.0), (num2 % 24.0), (num3 % 7.0), num4);
        }

        public static string FormatSecondsLong(this ulong i)
        {
            return ((long)i).FormatSecondsLong();
        }

        public static string FormatSecondsLong(this long s)
        {
            double num1 = Math.Floor(s / 60.0);
            double num2 = Math.Floor(num1 / 60.0);
            double num3 = Math.Floor(num2 / 24.0);
            double num4 = Math.Floor(num3 / 7.0);
            if (s < 60L)
                return string.Format("{0} seconds", s);
            if (num1 < 60.0)
                return string.Format("{1} minutes, {0} seconds", (s % 60L), num1, num2, num3, num4);
            if (num2 < 48.0)
                return string.Format("{2} hours and {1} minutes", (s % 60L), (num1 % 60.0), num2, num3, num4);
            if (num3 < 7.0)
                return string.Format("{3} days, {2} hours and {1} minutes", (s % 60L), (num1 % 60.0), (num2 % 24.0), (num3 % 7.0), num4);
            return string.Format("{3} days, {2} hours and {1} minutes", (s % 60L), (num1 % 60.0), (num2 % 24.0), num3, num4);
        }

        public static string FormatNumberShort(this ulong i)
        {
            return ((long)i).FormatNumberShort();
        }

        public static string FormatNumberShort(this long num)
        {
            if (num >= 100000L)
                return (num / 1000L).FormatNumberShort() + "K";
            if (num >= 10000L)
                return (num / 1000.0).ToString("0.#") + "K";
            return num.ToString("#,0");
        }
    }
}
