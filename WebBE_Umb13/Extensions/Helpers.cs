using System.Globalization;
using Umbraco.Commerce.Core.Events.Validation;

namespace YeahWeb.Extensions
{
    public static class Helpers
    {
        public static CultureInfo CultureEnUs => CultureInfo.GetCultureInfo("en-US");
        public static string HexToRgb(string hexColor, string fallback = "255,255,255")
        {
            if (string.IsNullOrEmpty(hexColor))
                return fallback;

            // Remove the '#' if it exists
            hexColor = hexColor.TrimStart('#');

            // Check if the hex color has a valid length
            if (hexColor.Length != 6)
                return fallback;

            // Convert hex to decimal
            int red = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            int green = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            int blue = Convert.ToInt32(hexColor.Substring(4, 2), 16);

            return $"{red},{green},{blue}";
        }

        public static string ToStringFormat(this decimal decNum, string format = "#.###")
        {
            return decNum.ToString(format, CultureEnUs);
        }
    }
}
