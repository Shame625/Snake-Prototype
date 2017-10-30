using System;

public class Map
    {
        public UInt16 _xSize;
        public UInt16 _ySize;

        public MapManager.Position spawnPoint;

        public byte[,] _grid;
        public UInt16[] _indexedGrid;
        public UInt16 startIndex;

        public Map(UInt16 x, UInt16 y)
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
