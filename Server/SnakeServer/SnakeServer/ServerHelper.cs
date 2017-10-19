using System;
using System.Text;

namespace SnakeServer
{
    public class ServerHelper
    {
        public (UInt16, UInt16) BytesToMessageLength(byte[] msg, byte[] len)
        {
            return (BitConverter.ToUInt16(msg, 0), BitConverter.ToUInt16(len, 0));
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

        public void PrintRecievedData(int clientId, UInt16 messageNO, UInt16 packetLength, ref byte[] data)
        {
            Console.WriteLine("------------------------------------------------------------------------------");
            Console.WriteLine("Counter: " + Program.counter + " Date / Time: " + DateTime.Now);
            Console.WriteLine("------------------------------------------------------------------------------");
            Console.WriteLine(string.Format("Recieved data from client: {0}", clientId));
            Console.WriteLine(string.Format("Message number: {0} Packet Length: {1}\n", messageNO, packetLength));
            Console.WriteLine(string.Format("Data\nHex: {0}\nString: {1}\n", PrintBytes(ref data), Encoding.ASCII.GetString(data)));
        }

        public void PrintSendingData(int clientId, UInt16 messageNO, UInt16 packetLength, ref byte[] data)
        {
            Console.WriteLine(string.Format("Sending data to client: {0}", clientId));
            Console.WriteLine(string.Format("Message number: {0} Packet Length: {1}\n", messageNO, packetLength));
            Console.WriteLine(string.Format("Data\nHex: {0}\nString: {1}\n", PrintBytes(ref data), Encoding.ASCII.GetString(data)));
            Console.WriteLine("------------------------------------------------------------------------------");
        }
    }
}
