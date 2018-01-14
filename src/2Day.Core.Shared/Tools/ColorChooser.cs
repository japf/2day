using System;
using System.Collections.Generic;
using System.Linq;

namespace Chartreuse.Today.Core.Shared.Tools
{
    public static class ColorChooser
    {
        private static readonly List<string> colors;
        private static readonly Dictionary<string, string> mapping;
        private static readonly Dictionary<string, string> namedColors;

        public static string Default = "#4A8ADC";

        public static string Blue = "#4A8ADC";
        public static string Orange = "#FC6E51";
        public static string Purple = "#8F41A0";
        public static string Yellow = "#F6BB42";
        public static string Green = "#8CC152";
        public static string Gray = "#656d78";
        public static string LightGray = "#ccd1d9";

        public static List<string> Colors
        {
            get { return colors; }
        }

        public static Dictionary<string, string> NamedColors
        {
            get { return namedColors; }
        }

        static ColorChooser()
        {
            colors = new List<string>
            {
                "#da4452",
                "#fc6e51",
                "#ce4f19",
                "#f6bb42",
                "#e7c413",
                "#8cc152",
                "#288e2f",
                "#37bc9b",
                "#1995a1",
                "#4a8adc",
                "#967adc",
                "#8f41a0",
                "#d770ac",
                "#f7a5b7",
                "#ccd1d9",
                "#656d78"
            };

            mapping = new Dictionary<string, string>
            {
                { "#FF0000", "#da4452" },
                { "#FF4500", "#da4452" },
                { "#DC143C", "#da4452" },
                { "#8B0000", "#ce4f19" },
                { "#A52A2A", "#da4452" },
                { "#B22222", "#da4452" },
                { "#A0522D", "#ce4f19" },
                { "#FF6347", "#ce4f19" },
                { "#CD5C5C", "#fc6e51" },
                { "#FFC0CB", "#fc6e51" },
                { "#F08080", "#fc6e51" },
                { "#FFFF00", "#e7c413" },
                { "#FFD700", "#e7c413" },
                { "#FFA500", "#f6bb42" },
                { "#DAA520", "#f6bb42" },
                { "#B8860B", "#f6bb42" },
                { "#FF8C00", "#f6bb42" },
                { "#FFDAB9", "#f6bb42" },
                { "#FFE4C4", "#f6bb42" },
                { "#DEB887", "#f6bb42" },
                { "#D2691E", "#fc6e51" },
                { "#FF7F50", "#fc6e51" },
                { "#008000", "#288e2f" },
                { "#00FF00", "#8cc152" },
                { "#7FFF00", "#8cc152" },
                { "#008080", "#1995a1" },
                { "#228B22", "#288e2f" },
                { "#006400", "#288e2f" },
                { "#556B2F", "#288e2f" },
                { "#000000", "#656d78" },
                { "#ADFF2F", "#8cc152" },
                { "#7CFC00", "#8cc152" },
                { "#90EE90", "#8cc152" },
                { "#32CD32", "#8cc152" },
                { "#808000", "#288e2f" },
                { "#6B8E23", "#288e2f" },
                { "#9ACD32", "#8cc152" },
                { "#00FF7F", "#8cc152" },
                { "#2E8B57", "#288e2f" },
                { "#98FB98", "#8cc152" },
                { "#66CDAA", "#37bc9b" },
                { "#3CB371", "#288e2f" },
                { "#00FA9A", "#37bc9b" },
                { "#20B2AA", "#37bc9b" },
                { "#7FFFD4", "#37bc9b" },
                { "#00FFFF", "#37bc9b" },
                { "#0000FF", "#4a8adc" },
                { "#00008B", "#4a8adc" },
                { "#191970", "#4a8adc" },
                { "#5F9EA0", "#37bc9b" },
                { "#6495ED", "#4a8adc" },
                { "#008B8B", "#1995a1" },
                { "#00CED1", "#37bc9b" },
                { "#00BFFF", "#4a8adc" },
                { "#ADD8E6", "#ccd1d9" },
                { "#0000CD", "#4a8adc" },
                { "#000080", "#4a8adc" },
                { "#40E0D0", "#37bc9b" },
                { "#4682B4", "#4a8adc" },
                { "#87CEEB", "#ccd1d9" },
                { "#6A5ACD", "#967adc" },
                { "#4169E1", "#4a8adc" },
                { "#B0E0E6", "#ccd1d9" },
                { "#AFEEEE", "#ccd1d9" },
                { "#87CEFA", "#4a8adc" },
                { "#48D1CC", "#37bc9b" },
                { "#FF00FF", "#d770ac" },
                { "#483D8B", "#967adc" },
                { "#8B008B", "#8f41a0" },
                { "#8A2BE2", "#8f41a0" },
                { "#800080", "#8f41a0" },
                { "#9932CC", "#8f41a0" },
                { "#9400D3", "#8f41a0" },
                { "#FF69B4", "#d770ac" },
                { "#FF1493", "#d770ac" },
                { "#4B0082", "#8f41a0" },
                { "#EE82EE", "#f7a5b7" },
                { "#D8BFD8", "#f7a5b7" },
                { "#DDA0DD", "#f7a5b7" },
                { "#DA70D6", "#8f41a0" },
                { "#DB7093", "#d770ac" },
                { "#C71585", "#8f41a0" },
                { "#9370DB", "#967adc" },
                { "#7B68EE", "#967adc" },
                { "#BA55D3", "#8f41a0" },
                { "#C0C0C0", "#ccd1d9" },
                { "#708090", "#656d78" },
                { "#A9A9A9", "#ccd1d9" },
                { "#696969", "#656d78" },
                { "#808080", "#656d78" },
                { "#E0FFFF", "#ccd1d9" },
                { "#E6E6FA", "#ccd1d9" },
                { "#D3D3D3", "#ccd1d9" },
                { "#778899", "#656d78" },
                { "#B0C4DE", "#ccd1d9" },
                { "#BDB76B", "#f6bb42" },
                { "#E9967A", "#fc6e51" },
                { "#F0E68C", "#f6bb42" },
                { "#FFB6C1", "#d770ac" },
                { "#FFA07A", "#fc6e51" },
                { "#CD853F", "#ce4f19" },
                { "#BC8F8F", "#d770ac" },
                { "#8B4513", "#ce4f19" },
                { "#FA8072", "#fc6e51" },
                { "#F4A460", "#f6bb42" },
                { "#800000", "#ce4f19" },
                { "#D2B48C", "#f6bb42" },
                { "#F5DEB3", "#f6bb42" },
                { "#BABABA", "#ccd1d9" }
            };

            namedColors = new Dictionary<string, string>();
            NamedColors.Add("aliceblue", "#f0f8ff");
            NamedColors.Add("antiquewhite", "#faebd7");
            NamedColors.Add("aqua", "#00ffff");
            NamedColors.Add("aquamarine", "#7fffd4");
            NamedColors.Add("azure", "#f0ffff");
            NamedColors.Add("beige", "#f5f5dc");
            NamedColors.Add("bisque", "#ffe4c4");
            NamedColors.Add("black", "#000000");
            NamedColors.Add("blanchedalmond", "#ffebcd");
            NamedColors.Add("blue", "#0000ff");
            NamedColors.Add("blueviolet", "#8a2be2");
            NamedColors.Add("nokiablue", "#1080DD");
            NamedColors.Add("brown", "#a52a2a");
            NamedColors.Add("burlywood", "#deb887");
            NamedColors.Add("cadetblue", "#5f9ea0");
            NamedColors.Add("chartreuse", "#7fff00");
            NamedColors.Add("chocolate", "#d2691e");
            NamedColors.Add("coral", "#ff7f50");
            NamedColors.Add("cornflowerblue", "#6495ed");
            NamedColors.Add("cornsilk", "#fff8dc");
            NamedColors.Add("crimson", "#dc143c");
            NamedColors.Add("cyan", "#00ffff");
            NamedColors.Add("darkblue", "#00008b");
            NamedColors.Add("darkcyan", "#008b8b");
            NamedColors.Add("darkgoldenrod", "#b8860b");
            NamedColors.Add("darkgray", "#a9a9a9");
            NamedColors.Add("darkgreen", "#006400");
            NamedColors.Add("darkkhaki", "#bdb76b");
            NamedColors.Add("darkmagenta", "#8b008b");
            NamedColors.Add("darkolivegreen", "#556b2f");
            NamedColors.Add("darkorange", "#ff8c00");
            NamedColors.Add("darkorchid", "#9932cc");
            NamedColors.Add("darkred", "#8b0000");
            NamedColors.Add("darksalmon", "#e9967a");
            NamedColors.Add("darkseagreen", "#8fbc8f");
            NamedColors.Add("darkslateblue", "#483d8b");
            NamedColors.Add("darkslategray", "#2f4f4f");
            NamedColors.Add("darkturquoise", "#00ced1");
            NamedColors.Add("darkviolet", "#9400d3");
            NamedColors.Add("deeppink", "#ff1493");
            NamedColors.Add("deepskyblue", "#00bfff");
            NamedColors.Add("dimgray", "#696969");
            NamedColors.Add("dodgerblue", "#1e90ff");
            NamedColors.Add("firebrick", "#b22222");
            NamedColors.Add("floralwhite", "#fffaf0");
            NamedColors.Add("forestgreen", "#228b22");
            NamedColors.Add("fuchsia", "#ff00ff");
            NamedColors.Add("gainsboro", "#dcdcdc");
            NamedColors.Add("ghostwhite", "#f8f8ff");
            NamedColors.Add("gold", "#ffd700");
            NamedColors.Add("goldenrod", "#daa520");
            NamedColors.Add("gray", "#808080");
            NamedColors.Add("green", "#008000");
            NamedColors.Add("greenyellow", "#adff2f");
            NamedColors.Add("honeydew", "#f0fff0");
            NamedColors.Add("hotpink", "#ff69b4");
            NamedColors.Add("indianred", "#cd5c5c");
            NamedColors.Add("indigo", "#4b0082");
            NamedColors.Add("ivory", "#fffff0");
            NamedColors.Add("khaki", "#f0e68c");
            NamedColors.Add("lavender", "#e6e6fa");
            NamedColors.Add("lavenderblush", "#fff0f5");
            NamedColors.Add("lawngreen", "#7cfc00");
            NamedColors.Add("lemonchiffon", "#fffacd");
            NamedColors.Add("lightblue", "#add8e6");
            NamedColors.Add("lightcoral", "#f08080");
            NamedColors.Add("lightcyan", "#e0ffff");
            NamedColors.Add("lightgoldenrodyellow", "#fafad2");
            NamedColors.Add("lightgreen", "#90ee90");
            NamedColors.Add("lightgrey", "#d3d3d3");
            NamedColors.Add("lightpink", "#ffb6c1");
            NamedColors.Add("lightsalmon", "#ffa07a");
            NamedColors.Add("lightseagreen", "#20b2aa");
            NamedColors.Add("lightskyblue", "#87cefa");
            NamedColors.Add("lightslategray", "#778899");
            NamedColors.Add("lightsteelblue", "#b0c4de");
            NamedColors.Add("lightyellow", "#ffffe0");
            NamedColors.Add("lime", "#00ff00");
            NamedColors.Add("limegreen", "#32cd32");
            NamedColors.Add("linen", "#faf0e6");
            NamedColors.Add("magenta", "#ff00ff");
            NamedColors.Add("maroon", "#800000");
            NamedColors.Add("mediumaquamarine", "#66cdaa");
            NamedColors.Add("mediumblue", "#0000cd");
            NamedColors.Add("mediumorchid", "#ba55d3");
            NamedColors.Add("mediumpurple", "#9370db");
            NamedColors.Add("mediumseagreen", "#3cb371");
            NamedColors.Add("mediumslateblue", "#7b68ee");
            NamedColors.Add("mediumspringgreen", "#00fa9a");
            NamedColors.Add("mediumturquoise", "#48d1cc");
            NamedColors.Add("mediumvioletred", "#c71585");
            NamedColors.Add("midnightblue", "#191970");
            NamedColors.Add("mintcream", "#f5fffa");
            NamedColors.Add("mistyrose", "#ffe4e1");
            NamedColors.Add("moccasin", "#ffe4b5");
            NamedColors.Add("navajowhite", "#ffdead");
            NamedColors.Add("navy", "#000080");
            NamedColors.Add("oldlace", "#fdf5e6");
            NamedColors.Add("olive", "#808000");
            NamedColors.Add("olivedrab", "#6b8e23");
            NamedColors.Add("orange", "#ffa500");
            NamedColors.Add("orangered", "#ff4500");
            NamedColors.Add("orchid", "#da70d6");
            NamedColors.Add("palegoldenrod", "#eee8aa");
            NamedColors.Add("palegreen", "#98fb98");
            NamedColors.Add("paleturquoise", "#afeeee");
            NamedColors.Add("palevioletred", "#db7093");
            NamedColors.Add("papayawhip", "#ffefd5");
            NamedColors.Add("peachpuff", "#ffdab9");
            NamedColors.Add("peru", "#cd853f");
            NamedColors.Add("pink", "#ffc0cb");
            NamedColors.Add("plum", "#dda0dd");
            NamedColors.Add("powderblue", "#b0e0e6");
            NamedColors.Add("purple", "#800080");
            NamedColors.Add("red", "#ff0000");
            NamedColors.Add("rosybrown", "#bc8f8f");
            NamedColors.Add("royalblue", "#4169e1");
            NamedColors.Add("saddlebrown", "#8b4513");
            NamedColors.Add("salmon", "#fa8072");
            NamedColors.Add("sandybrown", "#f4a460");
            NamedColors.Add("seagreen", "#2e8b57");
            NamedColors.Add("seashell", "#fff5ee");
            NamedColors.Add("sienna", "#a0522d");
            NamedColors.Add("silver", "#c0c0c0");
            NamedColors.Add("skyblue", "#87ceeb");
            NamedColors.Add("slateblue", "#6a5acd");
            NamedColors.Add("slategray", "#708090");
            NamedColors.Add("snow", "#fffafa");
            NamedColors.Add("springgreen", "#00ff7f");
            NamedColors.Add("steelblue", "#4682b4");
            NamedColors.Add("tan", "#d2b48c");
            NamedColors.Add("teal", "#008080");
            NamedColors.Add("thistle", "#d8bfd8");
            NamedColors.Add("tomato", "#ff6347");
            NamedColors.Add("turquoise", "#40e0d0");
            NamedColors.Add("violet", "#ee82ee");
            NamedColors.Add("wheat", "#f5deb3");
            NamedColors.Add("white", "#ffffff");
            NamedColors.Add("whitesmoke", "#f5f5f5");
            NamedColors.Add("yellow", "#ffff00");
            NamedColors.Add("yellowgreen", "#9acd32");
        }

        public static int GetColorIndex(string colorCode)
        {
            string entry = colors.FirstOrDefault(e => e.Equals(colorCode, StringComparison.OrdinalIgnoreCase));

            return colors.IndexOf(entry);
        }

        public static string GetNewColor(string oldColor)
        {
            if (mapping.ContainsValue(oldColor) || mapping.ContainsValue(oldColor.ToUpperInvariant()))
                return oldColor;

            string color = oldColor.ToLowerInvariant();
            if (NamedColors.ContainsKey(color))
                color = NamedColors[color];

            color = color.ToUpperInvariant();

            // remove 100% alpha if needed
            if (color.Length == 9 && color.StartsWith("#FF")) // #FFxxxxxx => #xxxxxx
                color = "#" + color.Substring(3);

            if (mapping.ContainsKey(color))
                return mapping[color];
            else if (!NamedColors.ContainsValue(color))
                return Blue;
            else
                return oldColor;
        }
    }
}
