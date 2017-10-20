using System;

namespace SnakeServer
{
    public static class Constants
    {
        //Server constants
        public static UInt16 LOGOUT_CODE { get { return 0x0001; } }

        //Server errors
        public static UInt16 USER_LOGGED_IN { get { return 0x00FF; } }

        //User name constants
        public static UInt16 USERNAME_LENGTH_MIN { get { return 3; } }
        public static UInt16 USERNAME_LENGTH_MAX { get { return 16; } }
        
        //User name Error codes
        public static UInt16 USERNAME_BAD { get { return 0x0000; } }
        public static UInt16 USERNAME_OK { get { return 0x0001; } }
        public static UInt16 USERNAME_IN_USE { get { return 0x0002; } }

        //Room constants
        public static UInt16 ROOM_NAME_LENGTH_MIN { get { return 3; } }
        public static UInt16 ROOM_NAME_LENGTH_MAX { get { return 16; } }
        public static UInt16 ROOM_PASSWORD_LENGTH_MIN { get { return 3; } }
        public static UInt16 ROOM_PASSWORD_LENGTH_MAX { get { return 16; } }
        public static UInt16 ROOM_TYPE_PRIVATE { get { return 0x0000; } }
        public static UInt16 ROOM_TYPE_PUBLIC { get { return 0x0001; } }

        //Room error codes
        public static UInt16 ROOM_NAME_BAD { get { return 0x0000; } }
        public static UInt16 ROOM_PASSWORD_BAD { get { return 0x0001; } }
        public static UInt16 ROOM_TYPE_BAD { get { return 0x0003; } }
        public static UInt16 ROOM_NAME_OK { get { return 0x0004; } }
        public static UInt16 ROOM_PASSWORD_OK { get { return 0x0005; } }
        public static UInt16 ROOM_TYPE_OK { get { return 0x0006; } }

        public static UInt16 ROOM_CREATE_FAILURE { get { return 0x00FE; } }
        public static UInt16 ROOM_CREATE_SUCCESS { get { return 0x00FF; } }

    }
}
