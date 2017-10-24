public class Map
    {
        public int _x;
        public int _y;
        public byte[,] _grid;

        public Map(int x, int y)
        {
            _x = x;
            _y = y;
            _grid = new byte[x, y];
        }

        public string PrintMap()
        {
        string map = "";
            for (int y = 0; y < _y; y++)
            {
                string temp = "";
                for (int x = 0; x < _x; x++)
                {
                    temp += _grid[x,y];
                }
            map += temp + '\n';
            }
        return map;
        }
    }
