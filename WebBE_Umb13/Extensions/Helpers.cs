using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace YeahWeb.Extensions;

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
        return decNum.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string ToStringFormat(this double douNum, string format = "#.###")
    {
        return douNum.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }

    //NOTE: use the Umbraco.Forms.Web.Helpers.GetRequestIpAddressToRecord???
    public static string? GetRemoteIPString(this HttpContext httpContext)
    {
        //https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-8.0&viewFallbackFrom=aspnetcore-2.2
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static Random _random => new();
    public static IEnumerable<T> RandomizeFirstItem<T>(this IEnumerable<T> srcItems)
    {
        var srcList = srcItems?.ToList();
        if (srcList == null || srcList.Count <= 1)
            return srcList;

        // random integer >= minValue and < maxValue
        int randomIndex = _random.Next(1, srcList.Count);
        T selectedItem = srcList.ElementAt(randomIndex);

        // Shift elements one position to the right
        for (int i = randomIndex; i > 0; i--)
        {
            srcList[i] = srcList[i - 1];
        }

        // place selected item at the first
        srcList[0] = selectedItem;

        return srcList;
    }
}
