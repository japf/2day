using System;
using System.Globalization;
using System.Linq;
using Windows.UI;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.Core.Universal.Tools.Extensions
{
    public static class ColorExtension
    {
        public static Color ToColor(this string value)
        {
            if (string.IsNullOrEmpty(value) || value.Equals("transparent", StringComparison.OrdinalIgnoreCase))
                return Colors.Transparent;

            // Named Colors
            string valueLower = value.ToLower();
            if (ColorChooser.NamedColors.ContainsKey(valueLower))
                return ColorChooser.NamedColors[valueLower].ToColor();

            // #ARGB and #RGB Hex Colors
            if (value[0] == '#')
                value = value.Remove(0, 1);

            int length = value.Length;
            if ((length == 6 || length == 8) && IsHexColor(value))
            {
                if (length == 8)
                    return Color.FromArgb(
                    byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber),
                    byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber),
                    byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber),
                    byte.Parse(value.Substring(6, 2), NumberStyles.HexNumber));

                if (length == 6)
                    return Color.FromArgb(0xff,
                    byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber),
                    byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber),
                    byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber));
            }

            // A,R,G,B and R,G,B Colors
            string[] argb = value.Split(
            new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (argb != null)
            {
                if (argb.Length == 4)
                    return Color.FromArgb(
                    byte.Parse(argb[0]), byte.Parse(argb[1]), byte.Parse(argb[2]),
                    byte.Parse(argb[3]));

                if (argb.Length == 3)
                    return Color.FromArgb(0xff,
                    byte.Parse(argb[0]), byte.Parse(argb[1]), byte.Parse(argb[2]));
            }

            return Colors.Red;
        }

        private static bool IsHexColor(string value)
        {
            if (value == null)
                return false;

            return value.All(IsHexDigit);
        }

        private static bool IsHexDigit(char character)
        {
            if (character >= 48 && character <= 57 || character >= 65 && character <= 70)
                return true;
            if (character >= 97)
                return character <= 102;
            else
                return false;
        }
    }
}
