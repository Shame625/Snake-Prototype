using System;

class Constants
{
    //Error codes
    public static UInt16 USERNAME_BAD { get { return 0x0000; } }
    public static UInt16 USERNAME_OK { get { return 0x0001; } }
    public static UInt16 USERNAME_IN_USE { get { return 0x0002; } }

    public static UInt16 LOGOUT_CODE { get { return 0x0001; } }

    public static UInt16 USER_LOGGED_IN { get { return 0x00FF; } }

    public static string CONNECTION_FAILED { get { return "Failed to connect!"; } }
    public static string CONNECTION_ATTEMPT { get { return "Attempting to connect! ..."; } }
    public static string CONNECTION_SUCCESS { get { return "Connected!"; } }
    public static string CONNECTION_LOST { get { return "Disconnected from server!"; } }

    public static int LOOP_MAX = 10;

    //Constants
    public static UInt16 USERNAME_LENGTH_MIN { get { return 3; } }
    public static UInt16 USERNAME_LENGTH_MAX { get { return 16; } }
}