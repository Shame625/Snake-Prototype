﻿using System;

namespace Admin_Remote_Connect
{
    public static class Constants
    {
        //Server constants
        public static int QUEUE_TIMER_TICK_MILISECONDS { get { return 15; } }
        public static int RECEIVE_BUFFER_SIZE { get { return 4096; } }
        public static int SEND_BUFFER_SIZE { get { return 512; } }

        public static UInt16 MESSAGE_BASE { get { return 4; } }
        public static UInt16 LOGOUT_CODE { get { return 0x0001; } }
        public static UInt16 FORCED_DISCONNECT { get { return 0x0002; } }
        public static UInt16 BAD_PACKET_SIZE { get { return 0x0003; } }

        //Disconect numbers
        public static UInt16 LOGOUT_NORMAL { get { return 0x0000; } }
        public static UInt16 LOGOUT_FORCED { get { return 0x0001; } }
        public static UInt16 LOGOUT_WARNING { get { return 0x0002; } }

        //Server errors
        public static UInt16 USER_LOGGED_IN { get { return 0x00FF; } }
        public static UInt16 USER_ALREADY_IN_ROOM { get { return 0x01FF; } }

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

        public static UInt16 ROOM_CREATE_FAILURE { get { return 0x00FE; } }
        public static UInt16 ROOM_CREATE_SUCCESS { get { return 0x00FF; } }

        //Room abandon / leave cosnstants
        public static UInt16 ROOM_ABANDONED_SUCCESS { get { return 0x0000; } }
        public static UInt16 ROOM_ABANDONED_FAILURE { get { return 0x0001; } }

        public static UInt16 ROOM_LEAVE_SUCCESS { get { return 0x0002; } }
        public static UInt16 ROOM_LEAVE_FAILURE { get { return 0x0003; } }

        public static UInt16 ROOM_JOIN_SUCCESS { get { return 0x0004; } }
        public static UInt16 ROOM_JOIN_FAILURE { get { return 0x0005; } }

        public static UInt16 ROOM_CANCEL_FINDING_SUCCESS { get { return 0x0006; } }
        public static UInt16 ROOM_CANCEL_FINDING_FAILURE { get { return 0x0007; } }


        //Chat system
        public static UInt16 CHAT_MSG_LENGTH_MAX { get { return 100; } }

        //Admin Stuff
        public static string ADMIN_LOGIN_PASSWORD { get { return "admin"; } }
        public static UInt16 ADMIN_LOGIN_FAILURE { get { return 0x0000; } }
        public static UInt16 ADMIN_LOGIN_SUCCESS { get { return 0x0001; } }


        //Admin Client Constants
        public static string CLIENT_EXIT { get { return "exit"; } }
        public static string CLIENT_CONNECT { get { return "connect"; } }
        public static string CLIENT_HELP { get { return "help"; } }
        public static string CLIENT_LOGIN { get { return "login"; } }
        public static string CLIENT_DUMP_USERS_TO_FILE { get { return "dump_users"; } }
        public static string CLIENT_DUMP_GAMES_TO_FILE { get { return "dump_games"; } }
    }
}
