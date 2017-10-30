using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PacketHelper : MonoBehaviour
{
    public byte[] StringToBytes(UInt16 messageNo, ref string str)
    {
        byte[] data = Encoding.ASCII.GetBytes(str);

        UInt16 length = CalculateLength(ref data);
        byte[] dataToSend = new byte[length];

        FillHeader(ref messageNo, ref length, ref dataToSend);

        Array.Copy(data, 0, dataToSend, 4, data.Length);

        return dataToSend;
    }

    public byte[] BlankMessage(UInt16 messageNo)
    {
        byte[] data = new byte[4];

        FillHeaderBlankData(ref messageNo, ref data);
        return data;
    }

    public byte[] IntToBytes(UInt16 msg, int resp)
    {
        byte[] data = new byte[8];
        byte[] response = new byte[4];

        UInt16 length = CalculateLength(ref response);
        FillHeader(ref msg, ref length, ref data);

        response = BitConverter.GetBytes(resp);
        data[4] = response[0];
        data[5] = response[1];
        data[6] = response[2];
        data[7] = response[3];

        return data;
    }

    public byte[] UInt16ToBytes(UInt16 msg, UInt16 resp)
    {
        byte[] data = new byte[6];
        byte[] response = new byte[2];

        UInt16 length = CalculateLength(ref response);
        FillHeader(ref msg, ref length, ref data);

        response = BitConverter.GetBytes(resp);
        data[4] = response[0];
        data[5] = response[1];


        return data;
    }

    public int BytesToJoinedRoomData(ref byte[] data, ref string opponentName, ref UInt16 mapId, ref UInt16 difficulty)
    {
        int nameLen = data.Length - 8;
        byte[] id_bytes = new byte[4];
        byte[] mapId_bytes = new byte[2];
        byte[] difficulty_bytes = new byte[2];

        byte[] name_bytes = new byte[nameLen];

        Array.Copy(data, 0, id_bytes, 0, 4);
        Array.Copy(data, 4, mapId_bytes, 0, 2);
        Array.Copy(data, 6, difficulty_bytes, 0, 2);

        Array.Copy(data, 8, name_bytes, 0, nameLen);

        mapId = BytesToUInt16(ref mapId_bytes);
        difficulty = BytesToUInt16(ref difficulty_bytes);

        opponentName = Encoding.ASCII.GetString(name_bytes);

        return BitConverter.ToInt32(id_bytes, 0);
    }

    public string BytesToJoinedPrivateRoomData(ref byte[] data, ref string opponentName, ref UInt16 mapId, ref UInt16 difficulty)
    {
        //base + roomMax
        int nameLen = data.Length - 21;

        byte[] roomNameBytes = new byte[Constants.ROOM_NAME_LENGTH_MAX-1];
        byte[] nameBytes = new byte[nameLen];
        byte[] mapId_bytes = new byte[2];
        byte[] difficulty_bytes = new byte[2];

        Array.Copy(data, 2, mapId_bytes, 0, 2);
        Array.Copy(data, 4, difficulty_bytes, 0, 2);

        Array.Copy(data, 6, roomNameBytes, 0, Constants.ROOM_NAME_LENGTH_MAX - 1);
        Array.Copy(data, (Constants.ROOM_NAME_LENGTH_MAX - 1) + 6, nameBytes, 0, nameLen);

        opponentName = Encoding.ASCII.GetString(nameBytes);
        mapId = BitConverter.ToUInt16(mapId_bytes, 0);
        difficulty = BitConverter.ToUInt16(difficulty_bytes, 0);

        Debug.Log(opponentName);

        return Encoding.ASCII.GetString(roomNameBytes);
    }

    public byte[] PrivateRoomToBytes(UInt16 msg, UInt16 roomType, string roomName, string roomPassword)
    {
        UInt16 pass_len = 0;

        if (!string.IsNullOrEmpty(roomPassword))
            pass_len = (UInt16)roomPassword.Length;

        //msg len, roomtype len + string max + whatever remains of password
        byte[] data = new byte[6 + (Constants.ROOM_NAME_LENGTH_MAX-1) + pass_len];

        byte[] response = new byte[(2 + (Constants.ROOM_NAME_LENGTH_MAX-1) + pass_len)];

        byte[] room_type_bytes = new byte[2];
        room_type_bytes = BitConverter.GetBytes(roomType);

        byte[] name_bytes = new byte[Constants.ROOM_NAME_LENGTH_MAX-1];
        byte[] pass_bytes;

        if (roomName.Length >= Constants.ROOM_NAME_LENGTH_MAX)
        {
            byte[] dummyData = new byte[19];
            Array.Clear(dummyData, 0, 19);
            FillHeaderBlankData(ref msg, ref dummyData);
            dummyData[4] = 0x01;
            return dummyData;
        }

        name_bytes = Encoding.ASCII.GetBytes(roomName);

        Array.Copy(room_type_bytes, response, 2);
        Array.Copy(name_bytes, 0, response, 2, name_bytes.Length);

        if (pass_len > 0)
        {
            pass_bytes = new byte[pass_len];
            pass_bytes = Encoding.ASCII.GetBytes(roomPassword);
            Array.Copy(pass_bytes, 0, response, (Constants.ROOM_NAME_LENGTH_MAX - 1) + 2, pass_len);
        }

        UInt16 length = CalculateLength(ref response);
        FillHeader(ref msg, ref length, ref data);

        Array.Copy(response, 0, data, 4, response.Length);
        return data;
    }

    public byte[] JoinPrivateRoomToBytes(UInt16 message, string name, string password)
    {
        byte[] data = new byte[4 + (Constants.ROOM_NAME_LENGTH_MAX - 1) + password.Length];

        byte[] response = new byte[((Constants.ROOM_NAME_LENGTH_MAX - 1) + password.Length)];
        UInt16 length = CalculateLength(ref response);

        FillHeader(ref message, ref length, ref data);

        byte[] name_bytes = new byte[Constants.ROOM_NAME_LENGTH_MAX - 1];
        byte[] pass_bytes;

        name_bytes = Encoding.ASCII.GetBytes(name);
        Array.Copy(name_bytes, 0, response,0, name_bytes.Length);

        if (password.Length > 0)
        {
            pass_bytes = new byte[password.Length];
            pass_bytes = Encoding.ASCII.GetBytes(password);
            Array.Copy(pass_bytes, 0, response, (Constants.ROOM_NAME_LENGTH_MAX - 1), password.Length);
        }

        Array.Copy(response, 0, data, 4, response.Length);
        return data;
    }

    public  byte[] ByteToBytes(UInt16 msg, byte resp)
    {
        byte[] data = new byte[5];
        UInt16 len = 5;

        FillHeader(ref msg, ref len, ref data);

        data[4] = resp;

        return  data;
    }

    public byte MovementBytesToDataOld(ref byte[] data, ref byte iExpanded, ref byte opponentExpaded, ref byte myNewBug, ref UInt16 myIndexBug, ref byte opponentNewBug, ref UInt16 opponentIndexBug)
    {
        byte[] myIndexBugBytes = new byte[2];
        byte[] opponentIndexBugBytes = new byte[2];

        Array.Copy(data, 4, myIndexBugBytes, 0, 2);
        Array.Copy(data, 7, opponentIndexBugBytes, 0, 2);

        iExpanded = data[1];
        opponentExpaded = data[2];
        myNewBug = data[3];
        opponentNewBug = data[6];

        return data[0];
    }

    public void MovementBytesToData(ref byte[] data, List<UInt16> p1, List<UInt16> p2, ref UInt16 bugLocationP1, ref UInt16 bugLocationP2)
    {
        byte[] p1listLen = new byte[2];
        byte[] p2listLen = new byte[2];

        UInt16 p1len;
        UInt16 p2len;

        p1listLen[0] = data[0];
        p1listLen[1] = data[1];
        p2listLen[0] = data[2];
        p2listLen[1] = data[3];

        p1len = BitConverter.ToUInt16(p1listLen, 0);
        p2len = BitConverter.ToUInt16(p2listLen, 0);

        byte[] tempBytes = new byte[2];

        for (int i = 0; i < p1len; i++)
        {
            Array.Copy(data, 4 + (i * 2), tempBytes, 0, 2);
            p1.Add(BitConverter.ToUInt16(tempBytes, 0));
        }

        for (int i = 0; i < p2len; i++)
        {
            Array.Copy(data, 4 + (p1len * 2) + (i * 2), tempBytes, 0, 2);
            p2.Add(BitConverter.ToUInt16(tempBytes, 0));
        }

        byte[] p1BugLocation = new byte[2];
        byte[] p2BugLocation = new byte[2];

        p1BugLocation[0] = data[data.Length - 4];
        p1BugLocation[1] = data[data.Length - 3];
        p2BugLocation[0] = data[data.Length - 2];
        p2BugLocation[1] = data[data.Length - 1];

        bugLocationP1 = BitConverter.ToUInt16(p1BugLocation, 0);
        bugLocationP2 = BitConverter.ToUInt16(p2BugLocation, 0);
    }

    public UInt16 BytesToUInt16(ref byte[] data)
    {
        try
        {
            return BitConverter.ToUInt16(data, 0);
        }
        catch
        {

        }
        return 0x00;
    }

    public int BytesToInt(ref byte[] data)
    {
        return BitConverter.ToInt32(data, 0);
    }

    void FillHeader(ref UInt16 messageNo, ref UInt16 len, ref byte[] data)
    {
        byte[] message_bytes = new byte[2];
        byte[] length_bytes = new byte[2];

        message_bytes = BitConverter.GetBytes(messageNo);
        length_bytes = BitConverter.GetBytes(len);

        data[0] = message_bytes[0];
        data[1] = message_bytes[1];

        data[2] = length_bytes[0];
        data[3] = length_bytes[1];
    }

    public void FillHeaderBlankData(ref UInt16 messageNo, ref byte[] data)
    {
        byte[] message_bytes = new byte[2];
        byte[] length_bytes = new byte[2];

        message_bytes = BitConverter.GetBytes(messageNo);
        length_bytes = BitConverter.GetBytes((UInt16)4);

        data[0] = message_bytes[0];
        data[1] = message_bytes[1];

        data[2] = length_bytes[0];
        data[3] = length_bytes[1];
    }

    public void FillHeaderBlankData(UInt16 messageNo, ref byte[] data)
    {
        byte[] message_bytes = new byte[2];
        byte[] length_bytes = new byte[2];

        message_bytes = BitConverter.GetBytes(messageNo);
        length_bytes = BitConverter.GetBytes((UInt16)4);

        data[0] = message_bytes[0];
        data[1] = message_bytes[1];

        data[2] = length_bytes[0];
        data[3] = length_bytes[1];
    }

    UInt16 CalculateLength(ref byte[] data)
    {
        return Convert.ToUInt16(data.Length + 4);
    }



    public string PrintBytes(ref byte[] byteArray)
    {
        var sb = new StringBuilder("new byte[] { ");
        for (var i = 0; i < byteArray.Length; i++)
        {
            var b = byteArray[i];
            sb.Append(b.ToString("X"));
            if (i < byteArray.Length - 1)
            {
                sb.Append(", ");
            }
        }
        sb.Append(" }");
        return sb.ToString();
    }
}
