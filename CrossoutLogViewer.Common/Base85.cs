using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class Base85
    {
        public const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~";

        public static string Encode(int integer)
        {
            if (integer < 0) throw new ArgumentOutOfRangeException(nameof(integer));
            if (integer == 0) return String.Empty;
            var sb = new StringBuilder();
            do
            {
                sb.Insert(0, Alphabet[integer % 85]);
                integer /= 85;
            } while (integer != 0);
            return sb.ToString();
        }
        public static string Encode(long integer)
        {
            if (integer < 0) throw new ArgumentOutOfRangeException(nameof(integer));
            if (integer == 0) return String.Empty;
            var sb = new StringBuilder();
            do
            {
                sb.Insert(0, Alphabet[(int)(integer % 85)]);
                integer /= 85;
            } while (integer != 0);
            return sb.ToString();
        }

        public static int DecodeInt32(string encodedValue)
        {
            if (String.IsNullOrEmpty(encodedValue)) throw new ArgumentNullException(nameof(encodedValue));
            if (encodedValue.Length == 0) return 0;
            int integer = 0;
            int exponent = 1;
            for (int i = encodedValue.Length - 1; i >= 0; i--)
            {
                var value = Alphabet.IndexOf(encodedValue[i]);
                if (value == -1) throw new FormatException();
                integer += value * exponent;
                exponent *= 85;
            }
            return integer;
        }

        public static long DecodeInt64(string encodedValue)
        {
            if (String.IsNullOrEmpty(encodedValue)) throw new ArgumentNullException(nameof(encodedValue));
            if (encodedValue.Length == 0) return 0;
            long integer = 0;
            long exponent = 1;
            for (int i = encodedValue.Length - 1; i >= 0; i--)
            {
                var value = Alphabet.IndexOf(encodedValue[i]);
                if (value == -1) throw new FormatException();
                integer += value * exponent;
                exponent *= 85;
            }
            return integer;
        }

    }
}
