using System;

namespace SnakeServer
{
    public static class PacketSizes
    {
        public static UInt16 SET_NAME_PACKET_MIN { get { return Constants.MESSAGE_BASE; } }
        public static UInt16 SET_NAME_PACKET_MAX { get { return Convert.ToUInt16(Constants.MESSAGE_BASE + (Constants.USERNAME_LENGTH_MAX - 1)); } }

        public static UInt16 LOGOUT_PACKET_MIN { get { return Convert.ToUInt16(Constants.MESSAGE_BASE + sizeof(UInt16)); } }
        public static UInt16 LOGOUT_PACKET_MAX { get { return Convert.ToUInt16(Constants.MESSAGE_BASE + sizeof(UInt16)); } }

        public static UInt16 USER_ID_MIN { get { return Constants.MESSAGE_BASE; } }
        public static UInt16 USER_ID_MAX { get { return Constants.MESSAGE_BASE; } }

        public static UInt16 ROOM_CREATE_MIN { get { return Convert.ToUInt16(Constants.MESSAGE_BASE + sizeof(UInt16)); } }
        public static UInt16 ROOM_CREATE_MAX { get { return Convert.ToUInt16(Constants.MESSAGE_BASE + sizeof(UInt16) + (Constants.ROOM_NAME_LENGTH_MAX - 1) + (Constants.ROOM_PASSWORD_LENGTH_MAX - 1)); } }
    }
}
