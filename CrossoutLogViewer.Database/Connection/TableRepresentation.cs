using System;

namespace CrossoutLogView.Database.Connection
{
    [Flags]
    public enum TableRepresentation
    {
        Store = 1 << 0,
        Reference = 1 << 1,
        StoreArray = 1 << 2,
        ReferenceArray = 1 << 3,
        Varialbe = 1 << 0 | 1 << 1,
        Array = 1 << 2 | 1 << 3,
        All = ~0
    }
}
