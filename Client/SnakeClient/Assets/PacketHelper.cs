using System;
using System.Text;

public class PacketHelper
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
