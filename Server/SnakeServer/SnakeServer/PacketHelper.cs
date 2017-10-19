using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    class PacketHelper
    {
        public (UInt16, byte[]) sendUInt16(UInt16 msg, UInt16 resp)
        {
            byte[] data = new byte[6];

            byte[] message = new byte[2];
            byte[] response = new byte[2];

            message = BitConverter.GetBytes(msg);
            response = BitConverter.GetBytes(resp);

            data[0] = message[0];
            data[1] = message[1];
            data[2] = 0x06;
            data[3] = 0x00;
            data[4] = response[0];
            data[5] = response[1];

            return (6, data);
        }
    }
}
