using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.Common
{
    public interface IMergable<T>
    {
        public T Merge(T other);
    }
}
