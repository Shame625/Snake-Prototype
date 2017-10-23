using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    public class Map
    {
        int _x;
        int _y;
        public byte[,] _grid;

        public Map(int x, int y)
        {
            _x = x;
            _y = y;
            _grid = new byte[x, y];
        }

        public void PrintMap()
        {
            
            for (int y = 0; y < _y; y++)
            {
                string temp = "";
                for (int x = 0; x < _x; x++)
                {
                    temp += _grid[x,y];
                }
            }
        }
    }
}
