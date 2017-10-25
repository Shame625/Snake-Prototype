using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

    static class MapManager
    {
        public enum MapObjects
        {
            AIR,
            WALL,
            SPAWN_POINT
        }

        public static int _NumberOfMaps;
        public static List<Map> _Maps = new List<Map>();
        static int previousMapIndex = 0;
        static int currentMapIndex = 0;

    public static void LoadMaps()
        {
        //path of Maps.txt is in Debug/Files/Maps.txt

        //string mapString = File.ReadAllText("Assets/Files/Maps.txt");

        TextAsset mapS = Resources.Load("Files/Maps") as TextAsset;
        Debug.Log(mapS);
            string[] maps = mapS.text.Split('-');

            //Keeping track of map number for error checking later
            _NumberOfMaps = maps.Length;

            //Breaking down to separate maps

            foreach(string map in maps)
            {
                string[] rows = map.Split('\n');

                //lol
                if(rows[0][0] == 13)
                {
                    rows =  rows.Skip(1).ToArray();
                }

                int x = rows[0].Length - 1;
                int y = rows.Length - 1;

                //Creating new map object
                Map newMap = new Map(x,y);

                int Y = 0;
                foreach (string row in rows)
                {
                    int X = 0;

                    foreach(char c in row)
                    {
                        if (char.IsNumber(c))
                        {
                            newMap._grid[X, Y] = ReturnMapObject(c - '0');
                            X++;
                        }
                    }
                    Y++;
                }

                _Maps.Add(newMap);
            Debug.Log(newMap.PrintMap());
            }
        }

        static byte ReturnMapObject(int i)
        {
            if (i == (int)MapObjects.AIR)
                return 0;
            else if (i == (int)MapObjects.WALL)
                return (int)MapObjects.WALL;
            else if (i == (int)MapObjects.SPAWN_POINT)
                return (int)MapObjects.SPAWN_POINT;

            return 0;
        }

    public static void Reset()
    {
        currentMapIndex = 0;
        previousMapIndex = 0;
    }

    public static int GetCurrentMapIndex()
    {
        return currentMapIndex;
    }

    public static int ChangeMapIndex(bool side)
    {
        if(!side)
        {
            if (currentMapIndex > 0)
            {
                previousMapIndex = currentMapIndex;
                currentMapIndex--;
            }
        }
        else
        {
            if (currentMapIndex < _NumberOfMaps - 1)
            {
                previousMapIndex = currentMapIndex;
                currentMapIndex++;
            }
        }
        
        return currentMapIndex;
    }

    public static void MapChanged(int id)
    {
        previousMapIndex = id;
        currentMapIndex = id;
    }

    public static void FailedToChangeMap()
    {
        currentMapIndex = previousMapIndex;
    }
}
