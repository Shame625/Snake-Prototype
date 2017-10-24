using System;

//Game Logic should go here
    public class Game
    {
        Map _selectedMap;
        UInt16 _selectedDifficulty;    

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