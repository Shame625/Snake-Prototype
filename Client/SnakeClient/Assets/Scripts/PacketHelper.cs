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

        byte[] name_bytes = new byte[Constants.ROOM_NAME_LENGTH_MAX-2];
        byte[] pass_bytes;

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

    public UInt16 BytesToUInt16(ref byte[] data)
    {
        return BitConverter.ToUInt16(data, 0);
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

    UInt16 CalculateLength(ref byte[] data)
    {
        return Convert.ToUInt16(data.Length + 4);
    }
}
