using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    class PacketHelper
    {
        public (UInt16, byte[]) UInt16ToBytes(UInt16 msg, UInt16 resp)
        {
            byte[] data = new byte[6];
            byte[] response = new byte[2];

            FillHeader(ref msg, 6, ref data);

            response = BitConverter.GetBytes(resp);
            data[4] = response[0];
            data[5] = response[1];

            return (6, data);
        }

        void FillHeader(ref UInt16 messageNo, UInt16 len, ref byte[] data)
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
}
