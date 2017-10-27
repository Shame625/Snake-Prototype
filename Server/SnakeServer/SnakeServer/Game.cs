using System;


//Game Logic should go here
namespace SnakeServer
{
    public class Game
    {
        public Map _selectedMap;
        public UInt16 _difficulty;
        public UInt16 _mapId;

        public bool _gameInProgress = false;

        public Game()
        {
            //Setting Default Map
            _selectedMap = MapManager._Maps[0];
            _difficulty = 0;
        }

        //Will be called with Client packet
        public void SetMap(UInt16 mapId)
        {
            _selectedMap = MapManager._Maps[mapId];
            _mapId = mapId;
        }

        public void SetDifficulty(UInt16 difficulty)
        {
            _difficulty = difficulty;
        }

    }
}
