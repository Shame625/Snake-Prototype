﻿using System;

//Game Logic should go here
    public class Game
    {
        public Map _selectedMap;
        public UInt16 _difficulty;    

        public Game()
        {
            //Setting Default Map
            _selectedMap = MapManager._Maps[0];
            _difficulty = Constants.ROOM_DIFFICULTY_EASY;
        }

        //Will be called with Client packet
        public void SetMap(int mapId)
        {
            _selectedMap = MapManager._Maps[mapId];
        }

        public void ClearGame()
        {
            _selectedMap = MapManager._Maps[0];
            _difficulty = Constants.ROOM_DIFFICULTY_EASY;
        }
    }