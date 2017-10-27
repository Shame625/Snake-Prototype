using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeServer
{
    public class RoomHelper
    {

        public Room CreatePublicRoom(Client client)
        {
            Room newRoom = new Room(client, Constants.ROOM_TYPE_PUBLIC, client._clientId);
            return newRoom;
        }

        public Room CreatePrivateRoom(Client client, ref RoomStruct room)
        {
            room.roomName = room.roomName.Trim('\0');
            room.roomPassword = room.roomPassword.Trim('\0');

            Room newRoom = new Room(client, room.roomType, client._clientId, room.roomName, room.roomPassword);

            return newRoom;
        }

        public UInt16 CheckRoomForErrors(ref RoomStruct room, Client c)
        {
            if (c._isInRoom)
                return Constants.USER_ALREADY_IN_ROOM;

            if (CheckRoomType(room.roomType) != Constants.ROOM_TYPE_OK)
                return Constants.ROOM_TYPE_BAD;

            if (room.roomType == Constants.ROOM_TYPE_PUBLIC)
                return Constants.ROOM_CREATE_SUCCESS;


            UInt16 errorCode = CheckRoomName(room.roomName);
            if (errorCode == Constants.ROOM_NAME_IN_USE)
                return Constants.ROOM_NAME_IN_USE;

            else if (errorCode == Constants.ROOM_NAME_BAD)
                return Constants.ROOM_NAME_BAD;

            else if (CheckRoomPassword(room.roomPassword) != Constants.ROOM_PASSWORD_OK)
                return Constants.ROOM_PASSWORD_BAD;

            return Constants.ROOM_CREATE_SUCCESS;
        }

        //Checks
        static UInt16 CheckRoomType(UInt16 type)
        {
            if (type > Constants.ROOM_TYPE_PRIVATE)
                return Constants.ROOM_TYPE_BAD;

            return Constants.ROOM_TYPE_OK;
        }

        static UInt16 CheckRoomName(string s)
        {
            s = s.Trim('\0');

            if (CheckIfRoomNameInUse(ref s, ref Program._privateRooms))
                return Constants.ROOM_NAME_IN_USE;

            if (s.Length <= Constants.ROOM_NAME_LENGTH_MIN || s.Length >= Constants.ROOM_NAME_LENGTH_MAX || s.Any(ch => !Char.IsLetterOrDigit(ch)))
            {
                return Constants.ROOM_NAME_BAD;
            }
            
            return Constants.ROOM_NAME_OK;
        }

        static UInt16 CheckRoomPassword(string p)
        {
            p = p.Trim('\0');

            if (string.IsNullOrEmpty(p))
                return Constants.ROOM_PASSWORD_OK;

            if (p.Length <= Constants.ROOM_PASSWORD_LENGTH_MIN || p.Length >= Constants.ROOM_PASSWORD_LENGTH_MAX || p.Any(ch => !Char.IsLetterOrDigit(ch)))
            {
                return Constants.ROOM_PASSWORD_BAD;
            }
            return Constants.ROOM_PASSWORD_OK;
        }

        public UInt16 AbandonRoom(Client c, ref int recievedId)
        {
            if(c._isInRoom)
            {
                if (c._currentRoom._roomAdmin == c && recievedId == c._clientId)
                {
                    DestroyRoom(c._currentRoom);
                    c.LeftRoom();
                    
                    return Constants.ROOM_ABANDONED_SUCCESS;
                }
            }
            return Constants.ROOM_ABANDONED_FAILURE;
        }

        public void DestroyRoom(Room r)
        {
            r.DisposeOfTimer();
            if(r._type == Constants.ROOM_TYPE_PRIVATE)
            {
                Program._privateRooms.Remove(r._roomName);
            }
            else
            {
                Program._publicRooms.Remove(r._roomId);
            }
        }

        public void PublicGamesTick(Room r)
        {
            if (Program._clientQueue.Count != 0)
            {
                try
                {
                    if (Program._connectedClients[Program._clientQueue[0]]._findingRoom)
                    {
                        Console.WriteLine("Clinet ID:" + Program._connectedClients[Program._clientQueue[0]]._clientId + " joined the room ID: " + r._roomId);

                        Program.SendDataJoinedRoom(r, Program._connectedClients[Program._clientQueue[0]]);

                        r.StartGame();
                        Program._clientQueue.RemoveAt(0);

                        Console.WriteLine("People in queue: " + Program._clientQueue.Count);
                    }
                }
                catch
                {
                    Console.WriteLine("remove DICT KEYS!");
                }
            }
        }

        private static bool CheckIfRoomNameInUse(ref string name, ref Dictionary<string, Room> dict)
        {
            if (dict.ContainsKey(name))
                return true;
            return false;
        }
    }
}
