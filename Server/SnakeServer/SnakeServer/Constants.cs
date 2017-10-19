using System;

namespace SnakeServer
{
    public static class Constants
    {
        //Constants
        public static UInt16 USERNAME_LENGTH_MIN { get { return 3; } }
        public static UInt16 USERNAME_LENGTH_MAX { get { return 16; } }

        //Error codes
        public static UInt16 USERNAME_BAD { get { return 0x0000; } }
        public static UInt16 USERNAME_OK { get { return 0x0001; } }


        //Server errors
        public static UInt16 USER_LOGGED_IN { get { return 0x00FF; } }
    }
}
