using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SnakeServer
{
    public class Position
    {
        public byte _x { get; set; }
        public byte _y { get; set; }

        public Position(){}

        public Position(byte x, byte y)
        {
            _x = x;
            _y = y;
        }
    };

    public class Map
    {
        public UInt16 _xSize;
        public UInt16 _ySize;
        public byte[,] _grid;

        public int[] _indexedGrid;

        public Position _spawnPoint = new Position();

        public Map(UInt16 x, UInt16 y)
        {
            _xSize = x;
            _ySize = y;
            _grid = new byte[x, y];
        }

        public void SetSpawnPoint(byte x, byte y)
        {
            _spawnPoint._x = x;
            _spawnPoint._y = y;
        }

        public UInt16 GetSpawnPointIndex()
        {
            return MapManager.getIndex(_spawnPoint._x, _spawnPoint._y, _xSize);
        }

        public void PrintMap()
        {
            
            for (UInt16 y = 0; y < _ySize; y++)
            {
                string temp = "";
                for (UInt16 x = 0; x < _xSize; x++)
                {
                    temp += _grid[x,y];
                }
            }
        }
    }
}
