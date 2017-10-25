using System;

class Constants
{
    //Error codes
    public static UInt16 USERNAME_BAD { get { return 0x0000; } }
    public static UInt16 USERNAME_OK { get { return 0x0001; } }
    public static UInt16 USERNAME_IN_USE { get { return 0x0002; } }
    public static UInt16 BAD_PACKET_SIZE { get { return 0x0003; } }

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
    public static UInt16 ROOM_TYPE_BAD { get { return 0x0002; } }
    public static UInt16 ROOM_NAME_OK { get { return 0x0003; } }
    public static UInt16 ROOM_PASSWORD_OK { get { return 0x0004; } }
    public static UInt16 ROOM_TYPE_OK { get { return 0x0005; } }
    public static UInt16 ROOM_NAME_IN_USE { get { return 0x0006; } }
    public static UInt16 ROOM_FULL { get { return 0x0007; } }

    public static UInt16 ROOM_PRIVATE_JOIN_SUCCESS { get { return 0x0009; } }
    public static UInt16 ROOM_PRIVATE_JOIN_FAILURE { get { return 0x0010; } }

    public static UInt16 ROOM_ABANDONED_SUCCESS { get { return 0x0000; } }
    public static UInt16 ROOM_ABANDONED_FAILURE { get { return 0x0001; } }

    public static UInt16 ROOM_LEAVE_SUCCESS { get { return 0x0002; } }
    public static UInt16 ROOM_LEAVE_FAILURE { get { return 0x0003; } }

    public static UInt16 ROOM_JOIN_SUCCESS { get { return 0x0004; } }
    public static UInt16 ROOM_JOIN_FAILURE { get { return 0x0005; } }

    public static UInt16 ROOM_CANCEL_FINDING_SUCCESS { get { return 0x0006; } }
    public static UInt16 ROOM_CANCEL_FINDING_FAILURE { get { return 0x0007; } }

    public static UInt16 ROOM_USER_JOINED { get { return 0x0008; } }
    public static UInt16 ROOM_USER_LEFT { get { return 0x0009; } }

    public static UInt16 ROOM_CREATE_FAILURE { get { return 0x00FE; } }
    public static UInt16 ROOM_CREATE_SUCCESS { get { return 0x00FF; } }

    public static UInt16 ROOM_DIFFICULTY_EASY { get { return 0x0000; } }
    public static UInt16 ROOM_DIFFICULTY_NORMAL { get { return 0x0001; } }
    public static UInt16 ROOM_DIFFICULTY_HARD { get { return 0x0002; } }

    public static UInt16 ROOM_MAP_CHANGE_SUCCESS { get { return 0x0001; } }
    public static UInt16 ROOM_MAP_CHANGE_FAILURE { get { return 0x0000; } }

    public static UInt16 ROOM_DIFFICULTY_CHANGE_SUCCESS { get { return 0x0001; } }
    public static UInt16 ROOM_DIFFICULTY_CHANGE_FAILURE { get { return 0x0000; } }

    //Room error messages
    public static string ROOM_NAME_BAD_MSG { get { return "Bad room name, must be between " + ROOM_NAME_LENGTH_MIN + " and " + ROOM_NAME_LENGTH_MAX + " characters, and cannot contain special characters!"; } }
    public static string ROOM_NAME_BAD_PASSWORD_MSG { get { return "Bad password, must be 'blank' or between " + ROOM_PASSWORD_LENGTH_MIN + " and " + ROOM_PASSWORD_LENGTH_MAX + " characters long, and cannot contain special characters!"; } }
    public static string ROOM_UNKNOWN_ERROR { get { return "Unknown error occured!"; } }
    public static string ROOM_ALREADY_IN { get { return "You are already in a room!"; } }
    public static string ROOM_NAME_IN_USE_MSG { get { return "Room with the same name already exists!"; } }
    public static string ROOM_ABANDONED_SUCCESS_MSG { get { return "You have abandoned the room!"; } }
    public static string ROOM_ABANDONED_MSG { get { return "Host has abandoned the room!"; } }
    public static string ROOM_ABANDONED_FAILED_MSG { get { return "You have failed to abandoned the room!"; } }
    public static string ROOM_LEAVE_SUCCESS_MSG { get { return "You have left the room!"; } }
    public static string ROOM_LEAVE_FAILED_MSG { get { return "You have failed to leave the room!"; } }
    public static string ROOM_JOIN_FAILURE_MSG { get { return "Failed to look for a room, are you in a room already?"; } }
    public static string ROOM_CANCEL_FINDING_FAILURE_MSG { get { return "Failed to cancel finding a room!"; } }
    public static string ROOM_PLAYER_LEFT_MY_ROOM { get { return "Player has left the group."; } }
    public static string ROOM_PRIVATE_NAME_BAD_MSG { get { return "Room does not exist!"; } }
    public static string ROOM_PRIVATE_PASSWORD_BAD_MSG { get { return "Wrong password!"; } }
    public static string ROOM_FULL_MSG { get { return "Room is full!"; } }
    public static string ROOM_FAILED_SET_MAP_MSG { get { return "Failed to set map!"; } }
    public static string ROOM_FAILED_SET_DIFFICULTY_MSG { get { return "Failed to set difficulty!"; } }
}