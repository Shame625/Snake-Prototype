﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SnakeServer
{
    static class MapManager
    {
        enum MapObjects
        {
            AIR,
            WALL,
            SPAWN_POINT,
        }

        public static int _NumberOfMaps;
        public static List<Map> _Maps = new List<Map>();

        public static void LoadMaps()
        {
            //path of Maps.txt is in Debug/Files/Maps.txt

            string mapString = File.ReadAllText("Files/Maps.txt");

            string[] maps = mapString.Split('-');

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
                Map newMap = new Map(Convert.ToUInt16(x), Convert.ToUInt16(y));

                int Y = 0;
                int i = 0;
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
                                newMap.SetSpawnPoint(Convert.ToByte(X), Convert.ToByte(Y));
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

        static public UInt16 getIndex(byte x, byte y, UInt16 length)
        {
            return Convert.ToUInt16(y * length + x);
        }

        static public Position getCoordinates(UInt16 index, UInt16 length)
        {
            return new Position(Convert.ToByte(index % length), Convert.ToByte(index / length));
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
    }
}
