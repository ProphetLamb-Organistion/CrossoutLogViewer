using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class ArrayHashing
    {

        /// <summary>
        /// Returns the Checksum of an <see cref="IEnumerable{ulong}"/>.
        /// </summary>
        /// <param name="values">The <see cref="IEnumerable{ulong}"/> containing the elements used to generate the Checksum.</param>
        /// <returns>The Checksum of an <see cref="IEnumerable{ulong}"/>.</returns>
        public static ulong Checksum(IEnumerable<ulong> values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            ulong c = 0;
            unchecked
            {
                foreach (var v in values)
                {
                    c += v;
                    c = c << 3 | c >> 61;
                    c ^= 0xFFFFFFFFFFFFFFFF;
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the Checksum of an <see cref="IEnumerable{uint}"/>.
        /// </summary>
        /// <param name="values">The <see cref="IEnumerable{uint}"/> containing the elements used to generate the Checksum.</param>
        /// <returns>The Checksum of an <see cref="IEnumerable{uint}"/>.</returns>
        public static uint Checksum(IEnumerable<uint> values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            uint c = 0;
            unchecked
            {
                foreach (var v in values)
                {
                    c += v;
                    c = c << 3 | c >> 29;
                    c ^= 0xFFFFFFFF;
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the Checksum of an <see cref="IEnumerable{int}"/>.
        /// </summary>
        /// <param name="values">The <see cref="IEnumerable{int}"/> containing the elements used to generate the Checksum.</param>
        /// <returns>The Checksum of an <see cref="IEnumerable{int}"/>.</returns>
        public static int Checksum(IEnumerable<int> values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            int c = 0;
            unchecked
            {
                foreach (var v in values)
                {
                    c += v;
                    c = c << 3 | c >> 29;
                    c ^= 0xFFFFFF;
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the Checksum of an <see cref="IEnumerable{long}"/>.
        /// </summary>
        /// <param name="values">The <see cref="IEnumerable{long}"/> containing the elements used to generate the Checksum.</param>
        /// <returns>The Checksum of an <see cref="IEnumerable{long}"/>.</returns>
        public static long Checksum(IEnumerable<long> values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            long c = 0;
            unchecked
            {
                foreach (var v in values)
                {
                    c += v;
                    c = c << 3 | c >> 29;
                    c ^= 0xFFFFFFFFFFFFFFF;
                }
            }
            return c;
        }
    }
}