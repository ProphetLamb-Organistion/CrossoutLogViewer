using System.Collections.Generic;

namespace CrossoutLogView.Statistics
{
    public class GameMap
    {
        public GameMap(Map map, IEnumerable<Game> games)
        {
            Map = map;
            Games = games;
        }

        public Map Map { get; }
        public IEnumerable<Game> Games { get; }
    }
}
