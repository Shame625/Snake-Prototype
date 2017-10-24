using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin_Remote_Connect
{
    public static class PacketHelper
    {
        public static byte[] LoginData(UInt16 message, string password)
        {
            byte[] password_bytes = Encoding.ASCII.GetBytes(password);
            byte[] data = new byte[password_bytes.Length + 4];
            UInt16 length = Convert.ToUInt16(data.Length);

            FillHeader(message, length, ref data);
            Array.Copy(password_bytes, 0, data, 4, password_bytes.Length);

            return data;
        }


        static void FillHeader(UInt16 messageNo, UInt16 len, ref byte[] data)
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

        public static void BytesToMessageLength(ref byte[] msg, ref byte[] len, ref UInt16 msgNo, ref UInt16 length)
        {
            msgNo = BitConverter.ToUInt16(msg, 0);
            length = BitConverter.ToUInt16(len, 0);
        }
    }
}
