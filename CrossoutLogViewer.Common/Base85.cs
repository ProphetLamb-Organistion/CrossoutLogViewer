using System;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class Base85
    {
        public const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~";

        /// <summary>
        /// Returns the base85 encoded representation of the input integer type value.
        /// </summary>
        /// <param name="integer">The integer type value.</param>
        /// <returns>The base85 encoded representation of the input integer type value.</returns>
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

        /// <summary>
        /// Returns the base85 encoded representation of the input integer type value.
        /// </summary>
        /// <param name="integer">The integer type value.</param>
        /// <returns>The base85 encoded representation of the input integer type value.</returns>
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

        /// <summary>
        /// Returns the 32bit ineger encoded in the input string.
        /// </summary>
        /// <param name="encodedValue">The encoded representation of a 32bit ineger in base85.</param>
        /// <returns>The 32bit ineger encoded in the input string.</returns>
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

        /// <summary>
        /// Returns the 64bit ineger encoded in the input string.
        /// </summary>
        /// <param name="encodedValue">The encoded representation of a 64bit ineger in base85.</param>
        /// <returns>The 64bit ineger encoded in the input string.</returns>
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
