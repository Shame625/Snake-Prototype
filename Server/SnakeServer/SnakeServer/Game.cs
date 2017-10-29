using System;
using System.Collections.Generic;


//Game Logic should go here
namespace SnakeServer
{
    enum MapObjects
    {
        AIR,
        WALL,
        PLAYER
    }
    public class Game
    {
        public Map _selectedMap;
        public UInt16 _difficulty;
        public UInt16 _mapId;

        int _currentLocationP1;
        int _currentLocationP2;

        public List<int> _P1blocks = new List<int>();
        public List<int> _P2blocks = new List<int>();

        byte P1Direction;
        byte P2Direction;

        int[] _currentMapP1;
        int[] _currentMapP2;

        public bool _gameInProgress = false;

        public Game()
        {
            //Setting Default Map
            SetMap(0);
            _difficulty = 0;
        }

        //Will be called with Client packet
        public void SetMap(UInt16 mapId)
        {
            _selectedMap = MapManager._Maps[mapId];
            _mapId = mapId;

            Initialization();
        }

        public void Reset()
        {
            Initialization();
        }

        public void SetDifficulty(UInt16 difficulty)
        {
            _difficulty = difficulty;
        }

        void Initialization()
        {
            _gameInProgress = false;
            _P1blocks.Clear();
            _P2blocks.Clear();

            _currentLocationP1 = _currentLocationP2 = MapManager.getIndex(_selectedMap._spawnPoint._x, _selectedMap._spawnPoint._y, _selectedMap._xSize);

            _P1blocks.Add(_currentLocationP1);
            _P2blocks.Add(_currentLocationP2);

            P1Direction = P2Direction = Constants.GAME_DIRECTION_RIGHT;

            _currentMapP1 = (int[])MapManager._Maps[_mapId]._indexedGrid.Clone();
            _currentMapP2 = (int[])MapManager._Maps[_mapId]._indexedGrid.Clone();
        }

        public byte Mover()
        {   
            //Get new directions
            int nextP1 = ReturnNewIndex(P1Direction, _currentLocationP1);
            int nextP2 = ReturnNewIndex(P2Direction, _currentLocationP2);

            //check conditions for /winning || loosing
            if((_currentMapP1[nextP1] == (int)MapObjects.WALL || _currentMapP1[nextP1] == (int)MapObjects.PLAYER) && (_currentMapP2[nextP2] == (int)MapObjects.WALL || _currentMapP2[nextP2] == (int)MapObjects.PLAYER))
            {
                return Constants.GAME_DRAW;
            }
            if (_currentMapP1[nextP1] == (int)MapObjects.WALL || _currentMapP1[nextP1] == (int)MapObjects.PLAYER)
            {
                return Constants.GAME_WON_P2;
            }
            else if(_currentMapP2[nextP2] == (int)MapObjects.WALL || _currentMapP2[nextP2] == (int)MapObjects.PLAYER)
            {
                return Constants.GAME_WON_P1;
            }

            if (_P1blocks.Count == 1)
            {
                _P1blocks[0] = nextP1;
                _currentMapP1[_currentLocationP1] = (byte)MapObjects.AIR;
                _currentMapP1[nextP1] = (byte)MapObjects.PLAYER;
            }
            else
            {
                _currentMapP1[_P1blocks.Count - 1] = (byte)MapObjects.AIR;

                for (int i = _P1blocks.Count - 1; i >= 1; i++)
                {
                    _P1blocks[i] = _P1blocks[i - 1];
                    _currentMapP1[_P1blocks[i - 1]] = (byte)MapObjects.PLAYER;
                }
                _P1blocks[0] = nextP1;

                _currentMapP1[nextP1] = (byte)MapObjects.PLAYER;
            }

            if (_P2blocks.Count == 1)
            {
                _P2blocks[0] = nextP2;
                _currentMapP2[_currentLocationP2] = (byte)MapObjects.AIR;
                _currentMapP2[nextP2] = (byte)MapObjects.PLAYER;
            }
            else
            {
                _currentMapP2[_P2blocks.Count - 1] = (byte)MapObjects.AIR;

                for (int i = _P2blocks.Count - 1; i >= 1; i++)
                {
                    _P2blocks[i] = _P2blocks[i - 1];
                    _currentMapP2[_P2blocks[i - 1]] = (byte)MapObjects.PLAYER;
                }
                _P2blocks[0] = nextP2;

                _currentMapP2[nextP2] = (byte)MapObjects.PLAYER;
            }

            //set new locations
            _currentLocationP1 = nextP1;
            _currentLocationP2 = nextP2;

            return 0xFF;
        }

        int ReturnNewIndex(byte dir, int index)
        {
            if (dir == Constants.GAME_DIRECTION_UP)
            {
                if ((index - _selectedMap._xSize) < 0)
                {
                    return _selectedMap._xSize * _selectedMap._ySize - index - 1;
                }
                return index - _selectedMap._xSize;
            }
            else if (index == Constants.GAME_DIRECTION_DOWN)
            {
                if ((dir + _selectedMap._xSize) > _selectedMap._xSize * _selectedMap._ySize - 1)
                {
                    return index % _selectedMap._xSize;
                }
                return index + _selectedMap._xSize;
            }
            else if (dir == Constants.GAME_DIRECTION_RIGHT)
            {
                if (index % _selectedMap._xSize + 1 >= _selectedMap._xSize)
                {
                    return index - (_selectedMap._xSize - 1);
                }
                return index + 1;
            }
            else if(dir == Constants.GAME_DIRECTION_LEFT)
            {
                if(index % _selectedMap._xSize == 0)
                {
                    return index + (_selectedMap._xSize - 1);
                }
            }
            return index - 1;
        }

    }
}
