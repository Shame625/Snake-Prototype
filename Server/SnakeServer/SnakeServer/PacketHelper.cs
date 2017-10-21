using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    public struct RoomStruct
    {
        public UInt16 roomType { get; set; }
        public string roomName { get; set; }
        public string roomPassword { get; set; }
    };

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

        public (UInt16, RoomStruct) BytesToRoomStruct(ref byte[] data)
        {
            RoomStruct room = new RoomStruct();
            UInt16 errorCode = Constants.ROOM_CREATE_SUCCESS;

            if (data.Length > 36)
                return (Constants.ROOM_CREATE_FAILURE, room);

            byte[] roomTypeBytes = new byte[2];
            byte[] roomNameBytes = new byte[15];
            byte[] roomPasswordBytes = new byte[15];

            Array.Copy(data, roomTypeBytes, 2);

            room.roomType = BitConverter.ToUInt16(roomTypeBytes, 0);

            if (room.roomType == Constants.ROOM_TYPE_PRIVATE)
            {
                Array.Copy(data, 2, roomNameBytes, 0, 15);

                try
                {
                    Array.Copy(data, 17, roomPasswordBytes, 0, data.Length - 17);
                }
                catch
                {
                    return (Constants.ROOM_CREATE_FAILURE, room);
                }
            }

            if (room.roomType == Constants.ROOM_TYPE_PRIVATE)
            {
                room.roomName = Encoding.ASCII.GetString(roomNameBytes);
                room.roomPassword = Encoding.ASCII.GetString(roomPasswordBytes);
            }

            return (errorCode, room);
        }

        public byte[] UInt16ToBytesNoLen(UInt16 msg, UInt16 resp)
        {
            byte[] data = new byte[6];
            byte[] response = new byte[2];

            FillHeader(ref msg, 6, ref data);

            response = BitConverter.GetBytes(resp);
            data[4] = response[0];
            data[5] = response[1];

            return data;
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
