using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;

namespace CrossoutLogView.GUI.Models
{
    public sealed class StripeModel : ViewModelBase
    {
        public StripeModel()
        {
            Parent = null;
            Stripe = new Stripe();
            Name = String.Empty;
        }

        public StripeModel(object parent, Stripe obj)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Stripe = obj ?? throw new ArgumentNullException(nameof(obj));
            Name = DisplayStringFactory.StripeName(Stripe.Name);
        }

        public Stripe Stripe { get; }

        public object Parent { get; } // Either playerview or userview

        public string Name { get; }

        public int Ammount => Stripe.Ammount;
    }
}
