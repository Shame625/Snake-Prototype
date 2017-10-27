public class Map
    {
        public int _xSize;
        public int _ySize;

        public MapManager.Position spawnPoint;

        public byte[,] _grid;

        public Map(int x, int y)
        {
            _xSize = x;
            _ySize = y;
            _grid = new byte[x, y];
        }

        public void SetSpawnPoint(byte x, byte y)
        {
            spawnPoint = new MapManager.Position(x, y);
        }

        public string PrintMap()
        {
        string map = "";
            for (int y = 0; y < _ySize; y++)
            {
                string temp = "";
                for (int x = 0; x < _xSize; x++)
                {
                    temp += _grid[x,y];
                }
            map += temp + '\n';
            }
        return map;
        }
    }
