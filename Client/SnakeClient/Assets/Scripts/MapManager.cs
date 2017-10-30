using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public static class MapManager
    {
    public class Position
    {
        public byte x;
        public byte y;

        public Position(byte v1, byte v2)
        {
            x = v1;
            y = v2;
        }
    };

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

                UInt16 x = Convert.ToUInt16(rows[0].Length - 1);
                UInt16 y = Convert.ToUInt16(rows.Length - 1);

                //Creating new map object
                Map newMap = new Map(x,y);
               
                int Y = 0;
                UInt16 i = 0;
                newMap._indexedGrid = new UInt16[x * y];

                foreach (string row in rows)
                {
                int X = 0;

                    foreach(char c in row)
                    {
                        if (char.IsNumber(c))
                        {
                            newMap._grid[X, Y] = ReturnMapObject(c - '0');
                            newMap._indexedGrid[i] = ReturnMapObject(c - '0');


                        if (ReturnMapObject(c - '0') == (int)MapObjects.SPAWN_POINT)
                        {
                            newMap.SetSpawnPoint((byte)X, (byte)Y);
                            newMap.startIndex = i;
                        }

                         X++;
                         i++;
                        }
                    }
                    Y++;

                }

                _Maps.Add(newMap);
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

    static public UInt16 getIndex(byte x, byte y, UInt16 length)
    {
        return Convert.ToUInt16(y * length + x);
    }

    static public Position getCoordinates(UInt16 index)
    {
        return new Position(Convert.ToByte(index % _Maps[currentMapIndex]._xSize), Convert.ToByte(index / _Maps[currentMapIndex]._xSize));
    }
}
