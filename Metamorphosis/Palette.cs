using System;
using UnityEngine;

namespace Metamorphosis
{
    public static class Palette
    {
        public static Color EnabledColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public static Color DisabledColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);

        public static Color Red = new Color(197.0f/255.0f, 17.0f/255.0f, 17.0f/255.0f, 1.0f);
        public static Color Blue = new Color(19.0f/255.0f, 46.0f/255.0f, 209.0f/255.0f, 1.0f);
        public static Color Green = new Color(17.0f/255.0f, 127.0f/255.0f, 45.0f/255.0f, 1.0f);
        public static Color Pink = new Color(237.0f/255.0f, 84.0f/255.0f, 186.0f/255.0f, 1.0f);
        public static Color Orange = new Color(239.0f/255.0f, 125.0f/255.0f, 14.0f/255.0f, 1.0f);
        public static Color Yellow = new Color(246.0f/255.0f, 246.0f/255.0f, 88.0f/255.0f, 1.0f);
        public static Color Black = new Color(63.0f/255.0f, 71.0f/255.0f, 78.0f/255.0f, 1.0f);
        public static Color White = new Color(214.0f/255.0f, 224.0f/255.0f, 240.0f/255.0f, 1.0f);
        public static Color Purple = new Color(107.0f/255.0f, 49.0f/255.0f, 188.0f/255.0f, 1.0f);
        public static Color Brown = new Color(113.0f/255.0f, 73.0f/255.0f, 30.0f/255.0f, 1.0f);
        public static Color Cyan = new Color(56.0f/255.0f, 254.0f/255.0f, 219.0f/255.0f, 1.0f);
        public static Color Lime = new Color(80.0f/255.0f, 239.0f/255.0f, 57.0f/255.0f, 1.0f);

        public static Color[] ColorById =
        {
            Red,
            Blue,
            Green,
            Pink,
            Orange,
            Yellow,
            Black,
            White,
            Purple,
            Brown,
            Cyan,
            Lime
        };
    }
}