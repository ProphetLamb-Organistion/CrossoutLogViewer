using System;

namespace CrossoutLogView.Statistics
{
    public class Stripe : IStatisticData, ICloneable
    {
        public string Name;
        public int Ammount;

        public Stripe()
        {
            Name = null;
            Ammount = 0;
        }

        public Stripe(string name, int ammount)
        {
            Name = name;
            Ammount = ammount;
        }

        public static Stripe Merge(Stripe stripe, Stripe other)
        {
            return new Stripe(stripe.Name, stripe.Ammount + other.Ammount);
        }

        public Stripe Clone() => new Stripe(Name, Ammount);
        object ICloneable.Clone() => Clone();

        public override string ToString() => String.Concat(nameof(Stripe), " ", Name, " ", Ammount);
    }
}
