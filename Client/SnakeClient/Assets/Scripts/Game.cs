using System;
using System.Collections.Generic;
using UnityEngine;

//Game Logic should go here
public class Game
    {
    enum MapObjects
    {
        AIR,
        WALL,
        PLAYER,
        BUG
    }
    private UInt16[] _currentMapP1;
    private UInt16[] _currentMapP2;

    public byte P1Direction;
    public byte P2Direction;

    public List<UInt16> _P1blocks = new List<UInt16>();
    public List<UInt16> _P2blocks = new List<UInt16>();

    public UInt16 _currentLocationP1;
    public UInt16 _currentLocationP2;

    public Map _selectedMap;
    public UInt16 _difficulty;

    public UInt16 bugLocationP1;
    public UInt16 bugLocationP2;

    public bool _gameInProgress = false;

    int mapID = 0;

    public Game()
    {
        //Setting Default Map
        ClearGame();
        Initialization();
    }

    //Will be called with Client packet
    public void SetMap(int mapId)
    {
        mapID = mapId;
        _selectedMap = MapManager._Maps[mapId];
        MapManager.MapChanged(mapId);
    }

    public void ClearGame()
    {
        MapManager.Reset();
        _gameInProgress = false;
        _selectedMap = MapManager._Maps[0];
        _difficulty = Constants.ROOM_DIFFICULTY_EASY;
    }

    public void SetGame()
    {
        Initialization();
    }

    void Initialization()
    {
        _gameInProgress = false;
        _P1blocks.Clear();
        _P2blocks.Clear();

        _currentLocationP1 = _currentLocationP2 = _selectedMap.startIndex;

        Debug.Log("Current map " + mapID);
        Debug.Log("First location of P2: " + _currentLocationP2);


        _P1blocks.Add(_currentLocationP1);
        _P2blocks.Add(_currentLocationP2);

        P1Direction = P2Direction = Constants.GAME_DIRECTION_RIGHT;

        _currentMapP1 = (UInt16[])MapManager._Maps[mapID]._indexedGrid.Clone();
        _currentMapP2 = (UInt16[])MapManager._Maps[mapID]._indexedGrid.Clone();
    }

    public UInt16 ReturnNewIndex(byte dir, int index)
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
}