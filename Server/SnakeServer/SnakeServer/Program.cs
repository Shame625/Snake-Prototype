﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SnakeServer
{
    class Program
    {
        public static int counter = 0;

        private static byte[] _buffer = new byte[4096];

        public static Dictionary<int, Client> _connectedClients = new Dictionary<int, Client>();
        public static Dictionary<string, int> _usedNames = new Dictionary<string, int>();

        public static Dictionary<string, Room> _privateRooms = new Dictionary<string, Room>();
        public static Dictionary<int, Room> _publicRooms = new Dictionary<int, Room>();

        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static ServerHelper serverHelper = new ServerHelper();
        private static PacketHelper packetHelper = new PacketHelper();
        private static RoomHelper roomHelper = new RoomHelper();

        public static List<int> _clientQueue = new List<int>();

        static void Main(string[] args)
        {
            SetupServer();
            Console.ReadLine();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(5);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            Console.WriteLine("Server is running!");
            Console.WriteLine("Frequency of queue dumps: " + Constants.QUEUE_TIMER_TICK_MILISECONDS + "ms.");

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMilliseconds(Constants.QUEUE_TIMER_TICK_MILISECONDS);

            var timer = new System.Threading.Timer((e) =>
            {
                QueueSystem();
            }, null, startTimeSpan, periodTimeSpan);
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);
            int id = socket.GetHashCode();

            if (!_connectedClients.ContainsKey(id))
            {
                _connectedClients.Add(socket.GetHashCode(), new Client(id, socket));
                Console.WriteLine("Client connected Socket hash: " + socket.GetHashCode() + " Client ID: " + _connectedClients[socket.GetHashCode()]._clientId);
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            }

            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void RecieveCallback(IAsyncResult AR)
        {
            try
            {

                Socket socket = (Socket)AR.AsyncState;

                int recieved = socket.EndReceive(AR);

                if (recieved == 0)
                {
                    RemoveClient(ref socket, Constants.LOGOUT_FORCED);

                    return;
                }
                else if (recieved > 1024)
                {
                    DisconnectUser(ref socket);
                    return;
                }

                counter++;

                UInt16 message_number = Messages.BAD_PACKET, packet_length = UInt16.MaxValue;
                int clientId = socket.GetHashCode();

                //Packet handling recieving/sending
                byte[] dataBuff = new byte[recieved];
                Array.Copy(_buffer, dataBuff, recieved);

                //Breaking down buffer into segments
                byte[] message_number_bytes = new byte[2];
                byte[] packet_length_bytes = new byte[2];
                byte[] data = new byte[512];

                try
                {
                    data = new byte[recieved - Constants.MESSAGE_BASE];
                    if (recieved >= Constants.MESSAGE_BASE)
                    {
                        Array.Copy(dataBuff, message_number_bytes, 2);
                        Array.Copy(dataBuff, 2, packet_length_bytes, 0, 2);
                        Array.Copy(dataBuff, 4, data, 0, dataBuff.Length - 4);

                        //Turn bytes to usuable data
                        (message_number, packet_length) = serverHelper.BytesToMessageLength(message_number_bytes, packet_length_bytes);
                    }

                    //Print to console recieved data
                    serverHelper.PrintRecievedData(clientId, message_number, packet_length, ref data, ref dataBuff);
                }
                catch
                {
                    Console.WriteLine(clientId + " sent bad packet!");
                }
               
                UInt16 message_number_send = UInt16.MaxValue;
                UInt16 lengthToSend = 0;
                byte[] dataToSendTemp = new byte[512];

#region MESSAGE HANDLING
                switch (message_number)
                {
                    case Messages.SET_NAME_REQUEST:
                        {
                            string temp_name = Encoding.ASCII.GetString(data);
                            UInt16 response = _connectedClients[clientId].SetName(ref temp_name);

                            //Generating send data
                            {
                                message_number_send = Messages.SET_NAME_RESPONSE;
                                (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, response);
                            }
                            break;
                        }

                    case Messages.LOGOUT:
                        {
                            RemoveClient(ref socket, Constants.LOGOUT_NORMAL);
                            break;
                        }

                    case Messages.USER_ID_REQUEST:
                        {
                            message_number_send = Messages.USER_ID_RESPONSE;
                            (lengthToSend, dataToSendTemp) = packetHelper.IntToBytes(message_number_send, clientId);
                            break;
                        }

#region ROOM_MESSAGES
                    case Messages.ROOM_CREATE_REQUEST:
                        {
                            if (data.Length <= Constants.MESSAGE_BASE + Constants.ROOM_NAME_LENGTH_MAX + Constants.ROOM_PASSWORD_LENGTH_MAX + 1)
                            {
                                (UInt16 errorCode, RoomStruct room) = packetHelper.BytesToRoomStruct(ref data);

                                message_number_send = Messages.ROOM_CREATE_RESPONSE;

                                if (_connectedClients[clientId]._isInRoom || _connectedClients[clientId]._findingRoom)
                                    errorCode = Constants.ROOM_CREATE_FAILURE;
                                else
                                {
                                    if (errorCode == Constants.ROOM_CREATE_FAILURE || errorCode == Constants.ROOM_NAME_BAD || errorCode == Constants.ROOM_PASSWORD_BAD)
                                    {
                                        Console.WriteLine("Room creation failed!");
                                        (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, errorCode);
                                    }

                                    //Proceed
                                    else if (errorCode == Constants.ROOM_CREATE_SUCCESS)
                                    {
                                        //reusing errorCode
                                        errorCode = roomHelper.CheckRoomForErrors(ref room, _connectedClients[clientId]);

                                        //if everything went good
                                        if (errorCode == Constants.ROOM_CREATE_SUCCESS)
                                        {
                                            message_number_send = Messages.ROOM_CREATE_RESPONSE;
                                            //Dont forget to check if user is already in a room, or owns one
                                            Console.WriteLine("------------------------------------------------------------");
                                            Console.WriteLine(clientId + " has created a room id: " + clientId);
                                            Console.WriteLine("Room Type: " + room.roomType + "\nRoom Name: " + room.roomName + "\nRoom Password: " + room.roomPassword);
                                            Console.WriteLine("------------------------------------------------------------");

                                            Room newRoom;
                                            
                                            if (room.roomType == Constants.ROOM_TYPE_PUBLIC)
                                            {
                                                newRoom = roomHelper.CreatePublicRoom(_connectedClients[clientId]);
                                                _connectedClients[clientId].EnteredRoom(ref newRoom);

                                                _publicRooms.Add(clientId, newRoom);
                                                roomHelper.PublicGamesTick(newRoom);
                                            }
                                            else
                                            {
                                                newRoom = roomHelper.CreatePrivateRoom(_connectedClients[clientId], ref room);
                                                _connectedClients[clientId].EnteredRoom(ref newRoom);

                                                _privateRooms.Add(room.roomName, newRoom);
                                            }

                                        }
                                    }
                                    (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, errorCode);
                                }

                                
                            }
                            else
                            {
                                Console.WriteLine("Invalid packet for room creation!");
                            }
                            break;
                        }

                        //gotta kick people off too
                    case Messages.ROOM_ABANDON_REQUEST:
                        {
                            Console.WriteLine("------------------------------------------------------------");
                            Console.WriteLine(clientId + " attempting to abandon room ID: " + clientId);
                            Console.WriteLine("------------------------------------------------------------");

                            message_number_send = Messages.ROOM_ABANDON_RESPONSE;
                            int recieved_id = BitConverter.ToInt32(data, 0);

                            int p2id = 0;

                            if(_connectedClients[clientId]._currentRoom.refClients[1] != null)
                            {
                                p2id = _connectedClients[clientId]._currentRoom.refClients[1]._clientId;
                            }

                            UInt16 errorCode = roomHelper.AbandonRoom(_connectedClients[clientId], ref recieved_id);

                            (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, errorCode);

                            if(errorCode == Constants.ROOM_ABANDONED_SUCCESS)
                            {
                                //telling player 2 that room has closed
                                if (p2id != 0)
                                {
                                    try
                                    {
                                        SendDataRoomAbandoned(_connectedClients[p2id]._socket, p2id);
                                    }
                                    catch
                                    {

                                    }
                                }

                                Console.WriteLine("------------------------------------------------------------");
                                Console.WriteLine(clientId + " succesffully abandoned room ID: " + clientId);
                                Console.WriteLine("------------------------------------------------------------");
                            }
                            break;
                        }

                    case Messages.ROOM_JOIN_PUBLIC_ROOM_REQUEST:
                        {
                            message_number_send = Messages.ROOM_JOIN_PUBLIC_ROOM_RESPONSE;
                            if (_connectedClients[clientId]._isInRoom)
                            {
                                (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_JOIN_FAILURE);
                            }
                            else
                            {
                                Console.WriteLine("People in queue: " + _clientQueue.Count);
                                Console.WriteLine(clientId + " added to queue. PUBLIC_ROOM");
                                _connectedClients[clientId]._findingRoom = true;

                                _clientQueue.Add(clientId);

                                (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_JOIN_SUCCESS);
                            }
                        }
                        break;

                    case Messages.ROOM_CANCEL_FINDING_REQUEST:
                        {
                            message_number_send = Messages.ROOM_CANCEL_FINDING_RESPONSE;
                            if (_connectedClients[clientId]._findingRoom)
                            {
                                Console.WriteLine(clientId + " removed from queue.");

                                _clientQueue.Remove(clientId);
                                _connectedClients[clientId].ClearStatus();

                                (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_CANCEL_FINDING_SUCCESS);
                            }
                            else
                            {
                                _connectedClients[clientId].ClearStatus();
                                (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_CANCEL_FINDING_FAILURE);
                            }
                        }
                        break;
                    #endregion
                    case Messages.BAD_PACKET:
                        {
                            lengthToSend = packetHelper.FillHeaderBlankData(Messages.BAD_PACKET, ref dataToSendTemp);
                            break;
                        }

                    default:
                            lengthToSend = packetHelper.FillHeaderBlankData(Messages.BAD_PACKET, ref dataToSendTemp);
                        break;
                }
#endregion

#region DATA_SEND
                byte[] dataToSend = new byte[lengthToSend];
                Array.Copy(dataToSendTemp, dataToSend, lengthToSend);

                if (lengthToSend > 0)
                {
                    serverHelper.PrintSendingData(clientId, message_number_send, lengthToSend, ref dataToSend);
                    socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }
#endregion
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            }

            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void DisconnectUser(ref Socket socket)
        {
            byte[] dataToSend = packetHelper.UInt16ToBytesNoLen(Messages.USER_DISCONNECT, Constants.FORCED_DISCONNECT);

            if (_connectedClients[socket.GetHashCode()]._currentRoom != null)
            {
                _connectedClients[socket.GetHashCode()]._currentRoom.RemovePlayer2();

                //Callback to admin of a room needed here
                SendDataPlayerLeftRoom();
            }
            try
            {
                socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
            RemoveClient(ref socket, Constants.LOGOUT_WARNING);
        }

        public static void SendDataJoinedRoom(Room r, Client c)
        {
            byte[] dataToSendP1 = packetHelper.StringToBytes(Messages.ROOM_JOINED_MY_ROOM , c._userName);
            byte[] dataToSendP2 = packetHelper.JoinedRoomDataToBytes(Messages.ROOM_JOINED_PUBLIC_ROOM, r._roomId, r._roomAdmin._userName);

            Console.WriteLine("Sending ROOM data to players!");
            Console.WriteLine("Player 1" + r._roomAdmin._userName);
            serverHelper.PrintSendingData(c._clientId, Messages.ROOM_JOINED_MY_ROOM, Convert.ToUInt16(dataToSendP1.Length), ref dataToSendP1);
            Console.WriteLine("Player 2" + c._userName);
            serverHelper.PrintSendingData(c._clientId, Messages.ROOM_JOINED_MY_ROOM, Convert.ToUInt16(dataToSendP2.Length), ref dataToSendP2);
            //player 1
            r._roomAdmin._socket.BeginSend(dataToSendP1, 0, dataToSendP1.Length, SocketFlags.None, new AsyncCallback(SendCallback), r._roomAdmin._socket);
            
            //player 2
            c.EnteredRoom(ref r);

            r.AddPlayer(ref c);

            c._socket.BeginSend(dataToSendP2, 0, dataToSendP2.Length, SocketFlags.None, new AsyncCallback(SendCallback), c._socket);
        }

        public static void SendDataRoomAbandoned(Socket s, int id)
        {
            byte[] dataToSend = new byte[Constants.MESSAGE_BASE];

            _connectedClients[id].ClearStatus();
            packetHelper.FillHeaderBlankData(Messages.ROOM_CLOSED, ref dataToSend);
            s.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), s);
        }

        public static void SendDataPlayerLeftRoom()
        {

        }

        //Queue system
        public static void QueueSystem()
        {
            //Console.WriteLine("Dumping Queue Number of people waiting: " + _clientQueue.Count + " Number of public rooms: " + _publicRooms.Count);
            if (_clientQueue.Count != 0)
            {
                Room roomRef = null;
                foreach (KeyValuePair<int, Room> entry in _publicRooms)
                {
                    if (entry.Value._isEmpty) {

                        roomRef = entry.Value;
                        break;
                    }
                }
                
                if (roomRef != null)
                {

                    Client clientRef = _connectedClients[_clientQueue[0]];
                    _clientQueue.RemoveAt(0);

                    roomRef.AddPlayer(ref clientRef);
                    clientRef.EnteredRoom(ref roomRef);

                    SendDataJoinedRoom(roomRef, clientRef);
                }
            }
        }

        private static void SendCallback(IAsyncResult AR)
        {
            try
            {
                Socket clientSocket = (Socket)AR.AsyncState;
                clientSocket.EndSend(AR);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void RemoveClient(ref Socket socket, UInt16 logout_type)
        {
            Console.WriteLine("Date / Time: " + DateTime.Now);

            if(_connectedClients[socket.GetHashCode()]._currentRoom != null)
            {
                _connectedClients[socket.GetHashCode()]._currentRoom.RemovePlayer2();

                //Callback to admin needed here
                SendDataPlayerLeftRoom();
            }

            if (_connectedClients.ContainsKey(socket.GetHashCode()))
            {
                _connectedClients[socket.GetHashCode()]._socket.Shutdown(SocketShutdown.Both);
                _connectedClients[socket.GetHashCode()]._socket.Close();
            }

            if (logout_type == Constants.LOGOUT_FORCED)
            {
                Console.WriteLine("User ID: " + socket.GetHashCode() + " forcibly disconnected!");
            }
            else if(logout_type == Constants.LOGOUT_NORMAL)
            {
                Console.WriteLine("User ID: " + socket.GetHashCode() + " gracefully disconnected!");
            }
            else if(logout_type == Constants.LOGOUT_WARNING)
            {
                Console.WriteLine("User ID: " + socket.GetHashCode() + " attempted to overflow buffer! Closing socket.");
            }

            if (_connectedClients.ContainsKey(socket.GetHashCode()))
            {
                if (!string.IsNullOrEmpty(_connectedClients[socket.GetHashCode()]._userName))
                {
                    if (_usedNames.ContainsKey(_connectedClients[socket.GetHashCode()]._userName))
                        _usedNames.Remove(_connectedClients[socket.GetHashCode()]._userName);
                }
                _connectedClients.Remove(socket.GetHashCode());
            }
        }
    }

}