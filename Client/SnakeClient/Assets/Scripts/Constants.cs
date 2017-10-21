using System;

class Constants
{
    //Error codes
    public static UInt16 USERNAME_BAD { get { return 0x0000; } }
    public static UInt16 USERNAME_OK { get { return 0x0001; } }
    public static UInt16 USERNAME_IN_USE { get { return 0x0002; } }

    //Client constants
    public static UInt16 LOGOUT_CODE { get { return 0x0001; } }
    public static UInt16 FORCED_DISCONNECT { get { return 0x0002; } }

    //Disconect numbers
    public static UInt16 LOGOUT_NORMAL { get { return 0x0000; } }
    public static UInt16 LOGOUT_FORCED { get { return 0x0001; } }
    public static UInt16 LOGOUT_WARNING { get { return 0x0002; } }

    public static UInt16 USER_LOGGED_IN { get { return 0x00FF; } }
    public static UInt16 USER_ALREADY_IN_ROOM { get { return 0x01FF; } }

    public static string CONNECTION_FAILED { get { return "Failed to connect!"; } }
    public static string CONNECTION_ATTEMPT { get { return "Attempting to connect! ..."; } }
    public static string CONNECTION_SUCCESS { get { return "Connected!"; } }
    public static string CONNECTION_LOST { get { return "Disconnected from server!"; } }
    public static string CONNECTION_FORCED_CLOSURE { get { return "Kicked from the server!";  } }

    public static int LOOP_MAX = 10;

    //Constants
    public static UInt16 USERNAME_LENGTH_MIN { get { return 3; } }
    public static UInt16 USERNAME_LENGTH_MAX { get { return 16; } }

    //Room Constants
    public static UInt16 ROOM_NAME_LENGTH_MIN { get { return 3; } }
    public static UInt16 ROOM_NAME_LENGTH_MAX { get { return 16; } }

    public static UInt16 ROOM_PASSWORD_LENGTH_MIN { get { return 3; } }
    public static UInt16 ROOM_PASSWORD_LENGTH_MAX { get { return 16; } }

    public static UInt16 ROOM_TYPE_PUBLIC { get { return 0x0000; } }
    public static UInt16 ROOM_TYPE_PRIVATE { get { return 0x0001; } }
    
    //Room error codes
    public static UInt16 ROOM_NAME_BAD { get { return 0x0000; } }
    public static UInt16 ROOM_PASSWORD_BAD { get { return 0x0001; } }
    public static UInt16 ROOM_TYPE_BAD { get { return 0x0003; } }

    //Room error messages
    public static string ROOM_NAME_BAD_ERROR_MSG { get { return "Bad room name, must be between " + ROOM_NAME_LENGTH_MIN + " and " + ROOM_NAME_LENGTH_MAX + " 16 characters, and cannot contain special characters!"; } }
    public static string ROOM_NAME_BAD_PASSWORD_MSG { get { return "Bad password, must be 'blank' or between " + ROOM_PASSWORD_LENGTH_MIN + " and " + ROOM_PASSWORD_LENGTH_MAX + " characters long!"; } }

}