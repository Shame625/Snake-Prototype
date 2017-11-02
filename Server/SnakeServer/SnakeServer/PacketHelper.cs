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

        public (UInt16, byte[]) ByteToBytes(UInt16 msg, byte resp)
        {
            byte[] data = new byte[5];

            FillHeader(ref msg, 5, ref data);

            data[4] = resp;

            return (5, data);
        }


        public byte[] StringToBytes(UInt16 messageNo, string str)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);

            UInt16 length = CalculateLength(ref data);
            byte[] dataToSend = new byte[length];

            FillHeader(ref messageNo, length, ref dataToSend);

            Array.Copy(data, 0, dataToSend, 4, data.Length);

            return dataToSend;
        }

        public (UInt16, RoomStruct) BytesToRoomStruct(ref byte[] data)
        {
            RoomStruct room = new RoomStruct();
            UInt16 errorCode = Constants.ROOM_CREATE_SUCCESS;
             
            if (data.Length + Constants.MESSAGE_BASE < PacketSizes.ROOM_CREATE_MIN)
                return (Constants.ROOM_CREATE_FAILURE, room);


            if (data.Length + Constants.MESSAGE_BASE > (PacketSizes.ROOM_CREATE_MAX))
                return (Constants.ROOM_PASSWORD_BAD, room);

            byte[] roomTypeBytes = new byte[2];
            byte[] roomNameBytes = new byte[15];
            byte[] roomPasswordBytes = new byte[15];

            try
            {
                Array.Copy(data, roomTypeBytes, 2);
            }
            catch
            {
                return (Constants.ROOM_CREATE_FAILURE, room);
            }

            room.roomType = BitConverter.ToUInt16(roomTypeBytes, 0);
            

            if (room.roomType == Constants.ROOM_TYPE_PRIVATE)
            {
                try
                {
                    Array.Copy(data, 2, roomNameBytes, 0, 15);
                }
                catch
                {
                    return (Constants.ROOM_NAME_BAD, room);
                }
                try
                {
                    Array.Copy(data, 17, roomPasswordBytes, 0, data.Length - 17);
                }
                catch
                {
                    return (Constants.ROOM_PASSWORD_BAD, room);
                }
            }

            if (room.roomType == Constants.ROOM_TYPE_PRIVATE)
            {
                room.roomName = Encoding.ASCII.GetString(roomNameBytes);
                room.roomPassword = Encoding.ASCII.GetString(roomPasswordBytes);
            }

            return (errorCode, room);
        }

        public (RoomStruct, UInt16) DecodeJoinPrivateRoom(ref byte[] data)
        {
            byte[] roomNameBytes = new byte[Constants.ROOM_NAME_LENGTH_MAX-1];
            byte[] roomPasswordBytes;
            string password = "";

            Array.Copy(data, roomNameBytes, 15);

            string roomName = Encoding.ASCII.GetString(roomNameBytes);

            if(data.Length >= Constants.ROOM_NAME_LENGTH_MAX-1)
            {
                roomPasswordBytes = new byte[data.Length - (Constants.ROOM_NAME_LENGTH_MAX - 1)];
                Array.Copy(data, Constants.ROOM_NAME_LENGTH_MAX - 1, roomPasswordBytes, 0, roomPasswordBytes.Length);
                password = Encoding.ASCII.GetString(roomPasswordBytes);
            }

            Console.WriteLine(roomName);

            RoomStruct room = new RoomStruct();
            room.roomType = Constants.ROOM_TYPE_PRIVATE;
            room.roomName = roomName;
            room.roomPassword = password;

            return (room, Constants.ROOM_PRIVATE_JOIN_SUCCESS);
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

        public (UInt16, byte[]) IntToBytes(UInt16 msg, int resp)
        {
            byte[] data = new byte[8];
            byte[] response = new byte[4];

            UInt16 length = CalculateLength(ref response);
            FillHeader(ref msg, length, ref data);

            response = BitConverter.GetBytes(resp);

            data[4] = response[0];
            data[5] = response[1];
            data[6] = response[2];
            data[7] = response[3];

            return (length, data);
        }

        public byte[] JoinedRoomDataToBytes(UInt16 msg, int id, UInt16 map_id, UInt16 difficulty, string userName)
        {
            //base + int + UInt16 + UInt16 + string len
            byte[] data = new byte[12 + userName.Length];
            byte[] id_bytes = new byte[4];
            byte[] mapId_bytes = new byte[2];
            byte[] difficulty_bytes = new byte[2];

            byte[] string_bytes = Encoding.ASCII.GetBytes(userName);
            id_bytes = BitConverter.GetBytes(id);
            mapId_bytes = BitConverter.GetBytes(map_id);
            difficulty_bytes = BitConverter.GetBytes(difficulty);

            UInt16 length = Convert.ToUInt16(Constants.MESSAGE_BASE + id_bytes.Length + mapId_bytes.Length + mapId_bytes.Length + string_bytes.Length);
            FillHeader(ref msg, length, ref data);

            Array.Copy(id_bytes, 0, data, 4, id_bytes.Length);
            Array.Copy(mapId_bytes, 0, data, 8, mapId_bytes.Length);
            Array.Copy(difficulty_bytes, 0, data, 10, difficulty_bytes.Length);
            Array.Copy(string_bytes, 0, data, 12, string_bytes.Length);

            return data;
        }

        public byte[] JoinedPrivateRoomDataToBytes(UInt16 msg, UInt16 errorMsg, UInt16 map_id, UInt16 difficulty, string room_name, string userName)
        {
            //UInt16 + UInt16 + UInt16 + UInt16 + UInt16 + str + str
            UInt16 length = Convert.ToUInt16(10 + (Constants.ROOM_NAME_LENGTH_MAX - 1) + userName.Length);

            byte[] data = new byte[length];
            byte[] errorMsgBytes = new byte[2];

            byte[] mapId_bytes = new byte[2];
            byte[] difficulty_bytes = new byte[2];

            byte[] roomNameBytes = new byte[Constants.ROOM_NAME_LENGTH_MAX - 1];
            byte[] userNameBytes;

            errorMsgBytes = BitConverter.GetBytes(errorMsg);
            mapId_bytes = BitConverter.GetBytes(map_id);
            difficulty_bytes = BitConverter.GetBytes(difficulty);
            roomNameBytes = Encoding.ASCII.GetBytes(room_name);
            userNameBytes = Encoding.ASCII.GetBytes(userName);

            FillHeader(ref msg, length, ref data);

            Array.Copy(errorMsgBytes, 0, data, 4, errorMsgBytes.Length);
            Array.Copy(mapId_bytes, 0, data, 6, mapId_bytes.Length);
            Array.Copy(difficulty_bytes, 0, data, 8, difficulty_bytes.Length);
            Array.Copy(roomNameBytes, 0, data, 10, roomNameBytes.Length);
            Array.Copy(userNameBytes, 0, data, (Constants.ROOM_NAME_LENGTH_MAX - 1) + Constants.MESSAGE_BASE + 6, userNameBytes.Length);

            return data;
        }

        //base bytes + direction of enemy + did enemy expand + my bug new + my bug new location + enemy bug new + enemy bug new location + [reserved for later]
        public byte[] MovementPacketOld(UInt16 msg, byte opponentDirection, byte iExpanded, byte opponentExpanded, byte myNewBug, UInt16 myIndexBug, byte opponentNewBug, UInt16 opponentIndexBug)
        {
            //base + byte + byte + byte + byte + UInt16 + byte + UInt16
            UInt16 length = 13;

            byte[] data = new byte[length];
            FillHeader(ref msg, length, ref data);

            data[4] = opponentDirection;
            data[5] = iExpanded;
            data[6] = opponentExpanded;

            data[7] = myNewBug;

            byte[] bugLocation = BitConverter.GetBytes(myIndexBug);
            Array.Copy(bugLocation, 0, data, 8, 2);

            data[10] = opponentNewBug;
            bugLocation = BitConverter.GetBytes(opponentIndexBug);
            Array.Copy(bugLocation, 0, data, 11, 2);

            return data;
        }

        public byte[] MovementPacket(UInt16 msg, List<UInt16> p1Blocks, List<UInt16> p2Blocks, UInt16 p1Bug, UInt16 p2Bug)
        {
            UInt16 length = Convert.ToUInt16(Constants.MESSAGE_BASE + ((p1Blocks.Count + p2Blocks.Count) * 2) + 4 + 4);
            byte[] data = new byte[length];

            byte[] blockcount = BitConverter.GetBytes(p1Blocks.Count);

            FillHeader(ref msg, length, ref data);

            data[4] = blockcount[0];
            data[5] = blockcount[1];

            blockcount = BitConverter.GetBytes(p2Blocks.Count);
            data[6] = blockcount[0];
            data[7] = blockcount[1];

            UInt16 counter = 8;
            foreach(UInt16 index in p1Blocks)
            {
                byte[] temp = BitConverter.GetBytes(index);
                data[counter] = temp[0];
                data[counter + 1] = temp[1];
                counter += 2;
            }

            foreach (UInt16 index in p2Blocks)
            {
                byte[] temp = BitConverter.GetBytes(index);
                data[counter] = temp[0];
                data[counter + 1] = temp[1];
                counter += 2;
            }

            byte[] bugLocation = new byte[2];
            bugLocation = BitConverter.GetBytes(p1Bug);
            data[data.Length - 4] = bugLocation[0];
            data[data.Length - 3] = bugLocation[1];

            bugLocation = BitConverter.GetBytes(p2Bug);
            data[data.Length - 2] = bugLocation[0];
            data[data.Length - 1] = bugLocation[1];

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

        public UInt16 FillHeaderBlankData(UInt16 messageNo, ref byte[] data)
        {
            byte[] message_bytes = new byte[2];
            byte[] length_bytes = new byte[2];

            message_bytes = BitConverter.GetBytes(messageNo);
            length_bytes = BitConverter.GetBytes((UInt16)4);

            data[0] = message_bytes[0];
            data[1] = message_bytes[1];

            data[2] = length_bytes[0];
            data[3] = length_bytes[1];

            return 4;
        }

        UInt16 CalculateLength(ref byte[] data)
        {
            return Convert.ToUInt16(data.Length + 4);
        }
    }
}
