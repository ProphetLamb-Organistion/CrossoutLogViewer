using System;

namespace CrossoutLogView.Common
{
    public struct Int32Range : IEquatable<Int32Range>
    {
        public readonly int Start;
        public readonly int End;
        public readonly int Count;

        public Int32Range(int start, int end) : this()
        {
            Start = start;
            End = end;
            Count = end - start;
        }

        public Int32Range LeftShift(int count)
        {
            return new Int32Range(Start - count, End - count);
        }

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
