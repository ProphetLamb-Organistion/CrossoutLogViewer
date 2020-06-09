using CrossoutLogView.Common;
using CrossoutLogView.Database;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Log;

using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CrossoutLogView.Test
{
    public static class Program
    {
        public static void Main()
        {
            var col = new ObservableCollection<int>() 
            {
                1,
                5,
                6,
                3,
                0,
                2,
                4
            };
            col.Sort((x,y) => y.CompareTo(x));
            Console.WriteLine("Sort");
            foreach (var v in col) Console.WriteLine(v);
            Console.WriteLine("Find");
            Console.WriteLine(col.Find(x => x == 4));
            Console.WriteLine("FindIndex");
            Console.WriteLine(col.FindIndex(x => x == 4));
            Console.ReadLine();
        }
    }
}
