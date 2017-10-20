using System;
using System.Linq;

namespace SnakeServer
{
    public class RoomHelper
    {
        public static UInt16 CreateRoom(UInt16 type, string name, string password)
        {
            if (CheckRoomType(ref type) != Constants.ROOM_TYPE_OK)
                return Constants.ROOM_TYPE_BAD;

            if (type == Constants.ROOM_TYPE_PRIVATE)
            {
                if (CheckRoomName(ref name) != Constants.ROOM_NAME_OK)
                {
                    return Constants.ROOM_NAME_BAD;
                }

                if(!string.IsNullOrEmpty(password))
                {
                    if(CheckRoomPassword(ref password) != Constants.ROOM_PASSWORD_OK)
                    {
                        return Constants.ROOM_PASSWORD_BAD;
                    }
                }
            }

            return Constants.ROOM_CREATE_SUCCESS;
        }


        //Checks
        static UInt16 CheckRoomType(ref UInt16 type)
        {
            if (type > Constants.ROOM_TYPE_PUBLIC)
                return Constants.ROOM_TYPE_BAD;

            return Constants.ROOM_TYPE_OK;
        }

        static UInt16 CheckRoomName(ref string s)
        {
            if (s.Length <= Constants.ROOM_NAME_LENGTH_MIN || s.Length >= Constants.ROOM_NAME_LENGTH_MAX || s.Any(ch => !Char.IsLetterOrDigit(ch)))
            {
                return Constants.ROOM_NAME_BAD;
            }
            return Constants.ROOM_NAME_OK;
        }

        static UInt16 CheckRoomPassword(ref string p)
        {
            if (p.Length <= Constants.ROOM_PASSWORD_LENGTH_MIN || p.Length >= Constants.ROOM_PASSWORD_LENGTH_MAX)
            {
                return Constants.ROOM_PASSWORD_OK;
            }
            return Constants.ROOM_PASSWORD_BAD;
        }
    }
}
