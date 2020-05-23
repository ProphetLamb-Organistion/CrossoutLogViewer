using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.Models
{
    public sealed class StripeModel : ViewModelBase
    {
        public StripeModel()
        {
            Parent = null;
            Object = new Stripe();
            Name = String.Empty;
        }

        public StripeModel(object parent, Stripe obj)
        {
            Parent = parent;
            Object = obj;
            Name = DisplayStringFactory.StripeName(Object.Name);
        }

        public override void UpdateCollections() { }

        public Stripe Object { get; }

        public object Parent { get; } //either playerview or userview

        public string ListItemString => String.Concat(Object.Ammount, CenterDotSeparator, Object.Name);

        public string Name { get; }

        public int Ammount => Object.Ammount;
    }
}
