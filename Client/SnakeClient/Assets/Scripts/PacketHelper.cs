using System;
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

    public int BytesToJoinedRoomData(ref byte[] data, ref string opponentName)
    {
        int nameLen = data.Length - 4;
        byte[] id_bytes = new byte[4];
        byte[] name_bytes = new byte[nameLen];

        Array.Copy(data, 0, id_bytes, 0, 4);
        Array.Copy(data, 4, name_bytes, 0, nameLen);

        opponentName = Encoding.ASCII.GetString(name_bytes);

        return BitConverter.ToInt32(id_bytes, 0);
    }

    public string BytesToJoinedPrivateRoomData(ref byte[] data, ref string opponentName)
    {
        //base + roomMax
        int nameLen = data.Length - 17;
        byte[] roomNameBytes = new byte[Constants.ROOM_NAME_LENGTH_MAX-1];
        byte[] nameBytes = new byte[nameLen];

        Array.Copy(data, 2, roomNameBytes, 0, Constants.ROOM_NAME_LENGTH_MAX - 1);
        Array.Copy(data, (Constants.ROOM_NAME_LENGTH_MAX - 1) + 2, nameBytes, 0, nameLen);

        opponentName = Encoding.ASCII.GetString(nameBytes);

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

    public UInt16 BytesToUInt16(ref byte[] data)
    {
        return BitConverter.ToUInt16(data, 0);
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
