using System;
using System.Globalization;

namespace CrossoutLogView.Log
{
    public class Tokenizer
    {
        public Tokenizer()
        {
            CurrentString = String.Empty;
        }

        public string CurrentString { get; private set; }
        public byte CurrentByte { get => Byte.Parse(CurrentString, CultureInfo.InvariantCulture.NumberFormat); }
        public short CurrentInt16 { get => Int16.Parse(CurrentString, CultureInfo.InvariantCulture.NumberFormat); }
        public int CurrentInt32 { get => Int32.Parse(CurrentString, CultureInfo.InvariantCulture.NumberFormat); }
        public long CurrentInt64 { get => Int64.Parse(CurrentString, CultureInfo.InvariantCulture.NumberFormat); }
        public double CurrentSingle { get => Single.Parse(CurrentString, CultureInfo.InvariantCulture.NumberFormat); }
        public int CurrentHex { get => Int32.Parse(CurrentString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat); }

        public int Position { get; private set; }

        public bool First(ReadOnlySpan<char> source, ReadOnlySpan<char> terminationPattern)
        {
            int index = source.IndexOf(terminationPattern, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1) return false;
            Position = index + terminationPattern.Length;
            CurrentString = String.Empty;
            return true;
        }

        public void End(ReadOnlySpan<char> source)
        {
            CurrentString = source.Slice(Position).Trim().ToString();
            Position = source.Length;
        }

        public bool MoveNext(ReadOnlySpan<char> source, ReadOnlySpan<char> terminationPattern)
        {
            var target = source.Slice(Position);
            int index = target.IndexOf(terminationPattern, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1) return false;
            CurrentString = target.Slice(0, index).Trim().ToString();
            Position += index + terminationPattern.Length;
            return true;
        }

        public void Reset()
        {
            CurrentString = String.Empty;
            Position = 0;
        }
    }
}
