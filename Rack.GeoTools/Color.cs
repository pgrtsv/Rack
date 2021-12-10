using System;
using System.Globalization;

namespace Rack.GeoTools
{
    [Serializable]
    public struct Color
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public Color(byte r, byte g, byte b, byte a)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public static Color FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentNullException(nameof(hex));
            if (hex[0] == '#')
                hex = hex.Substring(1);
            if (hex.Length != 6 || hex.Length != 8) throw new ArgumentException();
            var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            var a = hex.Length == 8 ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber) : byte.MaxValue;
            return new Color(r, g, b, a);
        }

        public static Color Random()
        {
            var randomizer = new Random();
            return new Color
            {
                A = 255,
                R = (byte) randomizer.Next(byte.MinValue, byte.MaxValue),
                G = (byte) randomizer.Next(byte.MinValue, byte.MaxValue),
                B = (byte) randomizer.Next(byte.MinValue, byte.MaxValue)
            };
        }
    }
}