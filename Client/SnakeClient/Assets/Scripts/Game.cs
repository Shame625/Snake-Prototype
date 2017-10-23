using System;


//Game Logic should go here
namespace SnakeServer
{
    public class Game
    {
        Map _selectedMap;

        public Game()
        {
            //Setting Default Map
            _selectedMap = MapManager._Maps[0];
        }

        //Will be called with Client packet
        public void SetMap(UInt16 mapId)
        {

        }
    }
}
