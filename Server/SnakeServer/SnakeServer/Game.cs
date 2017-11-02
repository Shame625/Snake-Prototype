using System;
using System.Collections.Generic;
using System.Linq;


//Game Logic should go here
namespace SnakeServer
{
    enum MapObjects
    {
        AIR,
        WALL,
        PLAYER,
        BUG
    }
    public class Game
    {
        public Map _selectedMap;
        public UInt16 _difficulty;
        public UInt16 _mapId;

        UInt16 _currentLocationP1;
        UInt16 _currentLocationP2;

        public List<UInt16> _P1blocks = new List<UInt16>();
        public List<UInt16> _P2blocks = new List<UInt16>();

        public byte P1Direction;
        public byte P2Direction;

        UInt16[] _currentMapP1;
        UInt16[] _currentMapP2;

        public UInt16 bugLocationP1;
        public UInt16 bugLocationP2;

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

            _currentMapP1 = (UInt16[])MapManager._Maps[_mapId]._indexedGrid.Clone();
            _currentMapP2 = (UInt16[])MapManager._Maps[_mapId]._indexedGrid.Clone();

            SpawnNewBug(false);
            SpawnNewBug(true);
        }

        public byte Mover()
        {
            //Get new directions
            UInt16 nextP1 = ReturnNewIndex(P1Direction, _currentLocationP1);
            UInt16 nextP2 = ReturnNewIndex(P2Direction, _currentLocationP2);

            //check conditions for /winning || loosing
            if((_currentMapP1[nextP1] == (int)MapObjects.WALL || _currentMapP1[nextP1] == (int)MapObjects.PLAYER) && (_currentMapP2[nextP2] == (int)MapObjects.WALL || _currentMapP2[nextP2] == (int)MapObjects.PLAYER))
            {
                return Constants.GAME_DRAW;
            }
            if (_currentMapP1[nextP1] == (int)MapObjects.WALL || _currentMapP1[nextP1] == (int)MapObjects.PLAYER)
            {
                return Constants.GAME_LOST_P1;
            }
            else if(_currentMapP2[nextP2] == (int)MapObjects.WALL || _currentMapP2[nextP2] == (int)MapObjects.PLAYER)
            {
                return Constants.GAME_WON_P1;
            }

            bool didExpand = false;
            if(nextP1 == bugLocationP1)
            {
                didExpand = true;
                Expand(false, _currentLocationP1);
            }

            if (_P1blocks.Count == 1)
            {
                _P1blocks[0] = nextP1;
                _currentMapP1[_currentLocationP1] = (byte)MapObjects.AIR;
                _currentMapP1[nextP1] = (byte)MapObjects.PLAYER;
            }
            else
            {
                _currentMapP1[_P1blocks[_P1blocks.Count - 1]] = (byte)MapObjects.AIR;

                for (int i = _P1blocks.Count - 1; i >= 1; i--)
                {
                    _P1blocks[i] = _P1blocks[i - 1];
                    _currentMapP1[_P1blocks[i - 1]] = (byte)MapObjects.PLAYER;
                }
                _P1blocks[0] = nextP1;

                _currentMapP1[nextP1] = (byte)MapObjects.PLAYER;

                if (didExpand)
                    SpawnNewBug(false);
            }

            didExpand = false;
            if (nextP2 == bugLocationP2)
            {
                didExpand = true;
                Expand(true, _currentLocationP2);
            }
            if (_P2blocks.Count == 1)
            {
                _P2blocks[0] = nextP2;
                _currentMapP2[_currentLocationP2] = (byte)MapObjects.AIR;
                _currentMapP2[nextP2] = (byte)MapObjects.PLAYER;
            }
            else
            {
                _currentMapP2[_P2blocks[_P2blocks.Count - 1]] = (byte)MapObjects.AIR;

                for (int i = _P2blocks.Count - 1; i >= 1; i--)
                {
                    _P2blocks[i] = _P2blocks[i - 1];
                    _currentMapP2[_P2blocks[i - 1]] = (byte)MapObjects.PLAYER;
                }
                _P2blocks[0] = nextP2;

                _currentMapP2[nextP2] = (byte)MapObjects.PLAYER;

                if(didExpand)
                    SpawnNewBug(true);
            }

            //set new locations
            _currentLocationP1 = nextP1;
            _currentLocationP2 = nextP2;

            return 0xFF;
        }

        public void Expand(bool player, UInt16 location)
        {
            //P1
            if(!player)
            {
                _P1blocks.Add(location);
            }
            else
            {
                _P2blocks.Add(location);
            }
        }

        public void SpawnNewBug(bool player)
        {
            List<UInt16> freeIndexes = new List<UInt16>();
            Random newRandom = new Random();
            UInt16 randIndex = 0;

            //P1
            if (!player)
            {
                for (UInt16 i = 0; i < _currentMapP1.Length; i++)
                {
                    if (_currentMapP1[i] == (UInt16)MapObjects.AIR)
                    {
                        freeIndexes.Add(i);
                    }
                }

                randIndex = (UInt16)newRandom.Next(0, freeIndexes.Count);
                freeIndexes[randIndex] = (UInt16)MapObjects.BUG;
                bugLocationP1 = randIndex;
            }
            else
            {
                freeIndexes = new List<UInt16>();
                for (UInt16 i = 0; i < _currentMapP2.Length; i++)
                {
                    if (_currentMapP2[i] == (UInt16)MapObjects.AIR)
                    {
                        freeIndexes.Add(i);
                    }
                }

                randIndex = (UInt16)newRandom.Next(0, freeIndexes.Count);
                freeIndexes[randIndex] = (UInt16)MapObjects.BUG;
                bugLocationP2 = randIndex;
            }

        }

        UInt16 ReturnNewIndex(byte dir, UInt16 index)
        {
            if (dir == Constants.GAME_DIRECTION_UP)
            {
                if ((index - _selectedMap._xSize) < 0)
                {
                    return Convert.ToUInt16((_selectedMap._xSize * (_selectedMap._ySize - 1)) + index);
                }
                return Convert.ToUInt16(index - _selectedMap._xSize);
            }
            else if (dir == Constants.GAME_DIRECTION_DOWN)
            {
                if ((index + _selectedMap._xSize) > _selectedMap._xSize * _selectedMap._ySize - 1)
                {
                    return Convert.ToUInt16(index % _selectedMap._xSize);
                }
                return Convert.ToUInt16(index + _selectedMap._xSize);
            }
            else if (dir == Constants.GAME_DIRECTION_RIGHT)
            {
                if (index % _selectedMap._xSize + 1 >= _selectedMap._xSize)
                {
                    return Convert.ToUInt16(index - (_selectedMap._xSize - 1));
                }
                return Convert.ToUInt16(index + 1);
            }
            else if (dir == Constants.GAME_DIRECTION_LEFT)
            {
                if (index % _selectedMap._xSize == 0)
                {
                    return Convert.ToUInt16(index + (_selectedMap._xSize - 1));
                }
            }
            return Convert.ToUInt16(index - 1);
        }

        public static bool CheckValidDirection(byte currentDir, byte requestedDir)
        {
            if (currentDir == Constants.GAME_DIRECTION_UP && requestedDir == Constants.GAME_DIRECTION_DOWN)
                return false;
            else if(currentDir == Constants.GAME_DIRECTION_DOWN &&  requestedDir == Constants.GAME_DIRECTION_UP)
                return false;
            else if (currentDir == Constants.GAME_DIRECTION_LEFT && requestedDir == Constants.GAME_DIRECTION_RIGHT)
                return false;
            else if (currentDir == Constants.GAME_DIRECTION_RIGHT && requestedDir == Constants.GAME_DIRECTION_LEFT)
                return false;

            return true;
        }
    }
}
