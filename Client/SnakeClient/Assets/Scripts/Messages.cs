using System;

public static class Messages
{
    public const UInt16 SET_NAME_REQUEST = 0x0000;
    public const UInt16 SET_NAME_RESPONSE = 0x0001;
    public const UInt16 LOGOUT = 0x0002;
    public const UInt16 USER_DISCONNECT = 0x0003;
    public const UInt16 BAD_PACKET = 0x00F0;

    public const UInt16 USER_ID_REQUEST = 0x0004;
    public const UInt16 USER_ID_RESPONSE = 0x0005;

    //Room messages
    public const UInt16 ROOM_CREATE_REQUEST = 0xAA00;
    public const UInt16 ROOM_CREATE_RESPONSE = 0xAA01;

    public const UInt16 ROOM_ABANDON_REQUEST = 0xAA02;
    public const UInt16 ROOM_ABANDON_RESPONSE = 0xAA03;

    public const UInt16 ROOM_JOIN_PUBLIC_ROOM_REQUEST = 0xAA04;
    public const UInt16 ROOM_JOIN_PUBLIC_ROOM_RESPONSE = 0xAA05;

    public const UInt16 ROOM_JOIN_PRIVATE_ROOM_REQUEST = 0xAA06;
    public const UInt16 ROOM_JOIN_PRIVATE_ROOM_RESPONSE = 0xAA07;

    public const UInt16 ROOM_LEAVE_REQUEST = 0xAA08;
    public const UInt16 ROOM_LEAVE_RESPONSE = 0xAA09;

    public const UInt16 ROOM_CANCEL_FINDING_REQUEST = 0xAA0A;
    public const UInt16 ROOM_CANCEL_FINDING_RESPONSE = 0xAA0B;

    public const UInt16 ROOM_JOINED_PUBLIC_ROOM = 0xAA0C;
    public const UInt16 ROOM_JOINED_MY_ROOM = 0xAA0D;
    public const UInt16 ROOM_PLAYER_LEFT_MY_ROOM = 0xAA0E;
    public const UInt16 ROOM_CLOSED = 0xAA0F;

    public const UInt16 ROOM_CHANGE_DIFFICULTY_REQUEST = 0xAB00;
    public const UInt16 ROOM_CHANGE_DIFFICULTY_RESPONSE = 0xAB01;

    public const UInt16 ROOM_CHANGE_MAP_REQUEST = 0xAB02;
    public const UInt16 ROOM_CHANGE_MAP_RESPONSE = 0xAB03;
    public const UInt16 ROOM_MAP_CHANGED = 0xAB04;
    public const UInt16 ROOM_DIFFICULTY_CHANGED = 0xAB05;

    public const UInt16 ROOM_GAME_START_REQUEST = 0xAB06;
    public const UInt16 ROOM_GAME_START_RESPONSE = 0xAB06;

    public const UInt16 ROOM_GAME_INITIATED = 0xAB07;
    public const UInt16 ROOM_GAME_STARTED = 0xAB08;

    //Ingame messages
    public const UInt16 GAME_MOVEMENT = 0xAB09;
    public const UInt16 GAME_ENDED = 0xAB10;
    public const UInt16 GAME_PLAYER_DIRECTION_CHANGE_REQUEST = 0xAB11;
    public const UInt16 GAME_PLAYER_DIRECTION_CHANGE_RESPONSE = 0xAB12;


}