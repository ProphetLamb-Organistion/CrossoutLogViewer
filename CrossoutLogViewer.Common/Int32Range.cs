using System;

namespace CrossoutLogView.Common
{
    public struct Int32Range : IEquatable<Int32Range>
    {
        public readonly int Start;
        public readonly int End;
        public readonly int Count;

        /// <summary>
        /// Initializes a new instance of <see cref="Int32Range"/>.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Int32Range(int start, int end) : this()
        {
            Start = start;
            End = end;
            Count = end - start;
        }

        /// <summary>
        /// Adds a specific value to both the <see cref="Start"/> and <see cref="End"/> value of this instance.
        /// </summary>
        /// <param name="count">The value offset.</param>
        public Int32Range Offset(int count)
        {
            return new Int32Range(Start + count, End + count);
        }

        /// <summary>
        /// Returns whether a value is widthin the range. Including the lower bound and excluding the upper bound.
        /// </summary>
        /// <param name="value">The value tested.</param>
        /// <returns>True if the value is widthin the range, otherwise false</returns>
        public bool Contains(int value)
        {
            return Start <= value && value < End;
        }

        public override bool Equals(object obj) => obj is Int32Range range && Equals(range);
        public bool Equals(Int32Range other) => Start == other.Start && End == other.End;
        public override int GetHashCode() => HashCode.Combine(Start, End);

        public static bool operator ==(Int32Range left, Int32Range right) => left.Equals(right);
        public static bool operator !=(Int32Range left, Int32Range right) => !(left == right);
    }
}
