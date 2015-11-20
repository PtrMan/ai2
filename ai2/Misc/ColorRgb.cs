using System;
using System.Collections.Generic;

namespace Misc
{
    class ColorRgb
    {
        public float r;
        public float g;
        public float b;

        public ColorRgb(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public static ColorRgb operator +(ColorRgb a, ColorRgb b)
        {
            return new ColorRgb(a.r + b.r, a.g + b.g, a.b + b.b);
        }

        public static ColorRgb operator -(ColorRgb a, ColorRgb b)
        {
            return new ColorRgb(a.r - b.r, a.g - b.g, a.b - b.b);
        }

        public static ColorRgb operator *(ColorRgb a, ColorRgb b)
        {
            return new ColorRgb(a.r * b.r, a.g * b.g, a.b * b.b);
        }

        public static ColorRgb operator *(ColorRgb a, float scale)
        {
            return new ColorRgb(a.r * scale, a.g * scale, a.b * scale);
        }

        public float getMagnitude()
        {
            return (r + g + b) * 0.33333333333333f;
        }
    }
}
