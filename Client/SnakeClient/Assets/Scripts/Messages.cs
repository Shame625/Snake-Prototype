using System;

public static class Messages
{
    public const UInt16 SET_NAME_REQUEST = 0x0000;
    public const UInt16 SET_NAME_RESPONSE = 0x0001;
    public const UInt16 LOGOUT = 0x0002;
    public const UInt16 USER_DISCONNECT = 0x0003;

    public const UInt16 USER_ID_REQUEST = 0x0004;
    public const UInt16 USER_ID_RESPONSE = 0x0005;

    //Room messages
    public const UInt16 ROOM_CREATE_REQUEST = 0xAA00;
    public const UInt16 ROOM_CREATE_RESPONSE = 0xAA01;

    public const UInt16 ROOM_ABANDON_REQUEST = 0xAA02;
    public const UInt16 ROOM_ABANDON_RESPONSE = 0xAA03;

    public const UInt16 ROOM_JOIN_REQUEST = 0xAA04;
    public const UInt16 ROOM_JOIN_RESPONSE = 0xAA05;

    public const UInt16 ROOM_LEAVE_REQUEST = 0xAA06;
    public const UInt16 ROOM_LEAVE_RESPONSE = 0xAA07;

}