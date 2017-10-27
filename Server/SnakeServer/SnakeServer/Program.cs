using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SnakeServer
{
    class Program
    {
        public static int counter = 0;

        private static byte[] _buffer = new byte[Constants.RECEIVE_BUFFER_SIZE];

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
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("Property of Yee Studios Inc.\nDeveloped by: Yee Studios Inc.");
            Console.WriteLine("---------------------------------------------------------\n");
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("Setting up server...");

            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(5);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

            Console.WriteLine("Frequency of queue dumps set to " + Constants.QUEUE_TIMER_TICK_MILISECONDS + " ms.");
            Console.WriteLine("Receive buffer size set to " + Constants.RECEIVE_BUFFER_SIZE + " bytes.");
            Console.WriteLine("Send buffer size set to " + Constants.SEND_BUFFER_SIZE + " bytes.");
            Console.WriteLine("Public game starts after " + Constants.ROOM_GAME_TIME_TO_START + " ms.");
            Console.WriteLine("Loading maps...");
            Console.WriteLine("Server is running!");
            Console.WriteLine("---------------------------------------------------------");

            MapManager.LoadMaps();
            ClearLogs();

            System.Timers.Timer queueTimer = new System.Timers.Timer();
            queueTimer.Elapsed += new System.Timers.ElapsedEventHandler(QueueSystem);
            queueTimer.Interval = Constants.QUEUE_TIMER_TICK_MILISECONDS;
            queueTimer.Start();

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
                else if (recieved > 512)
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
                byte[] data = new byte[Constants.SEND_BUFFER_SIZE];

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

                    case Messages.ROOM_JOIN_PRIVATE_ROOM_REQUEST:
                        {
                            message_number_send = Messages.ROOM_JOIN_PRIVATE_ROOM_RESPONSE;
                            UInt16 errorCode;

                            if (_connectedClients[clientId]._isInRoom)
                            {
                                (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_PRIVATE_JOIN_FAILURE);
                            }
                            else
                            {
                                RoomStruct room = new RoomStruct();
                                //decode
                                {
                                    errorCode = Constants.ROOM_PRIVATE_JOIN_SUCCESS;
                                    (room, errorCode) = packetHelper.DecodeJoinPrivateRoom(ref data);
                                    room.roomName = room.roomName.Trim('\0');
                                    room.roomPassword = room.roomPassword.Trim('\0');
                                    if (!_privateRooms.ContainsKey(room.roomName))
                                        errorCode = Constants.ROOM_NAME_BAD;
                                    else
                                    {
                                        if (_privateRooms[room.roomName]._roomPassword != room.roomPassword)
                                            errorCode = Constants.ROOM_PASSWORD_BAD;
                                        else if (!_privateRooms[room.roomName]._isEmpty)
                                            errorCode = Constants.ROOM_FULL;
                                    }

                                }

                                //send more data
                                if(errorCode == Constants.ROOM_PRIVATE_JOIN_SUCCESS)
                                {
                                    //generate nice big msg
                                    (lengthToSend, dataToSendTemp) = SendDataJoinedPrivateRoom(_privateRooms[room.roomName], _connectedClients[clientId]);
                                }
                                else
                                    (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, errorCode);
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

                    case Messages.ROOM_LEAVE_REQUEST:
                        {
                            message_number_send = Messages.ROOM_LEAVE_RESPONSE;
                            
                            {
                                if(_connectedClients[clientId]._isInRoom)
                                {
                                    Console.WriteLine(clientId + " has left the room id: " + _connectedClients[clientId]._currentRoom._roomId);

                                    int p1id = _connectedClients[clientId]._currentRoom._roomAdmin._clientId;

                                    _connectedClients[clientId]._currentRoom.RemovePlayer2();
                                    _connectedClients[clientId].ClearStatus();
                                    (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_LEAVE_SUCCESS);

                                    //Tell other player of room that player has left.
                                    SendDataPlayerLeftRoom(p1id);
                                }
                                else
                                {
                                    (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_LEAVE_FAILURE);
                                }
                            }

                            break;
                        }

                    case Messages.ROOM_CHANGE_MAP_REQUEST:
                        {
                            UInt16 request = BitConverter.ToUInt16(data, 0);
                            message_number_send = Messages.ROOM_CHANGE_MAP_RESPONSE;

                            //checking if user is admin of a room
                            if (_connectedClients[clientId]._currentRoom._roomAdmin._clientId == clientId)
                            {
                                if (request <= MapManager._NumberOfMaps - 1)
                                {
                                    (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_MAP_CHANGE_SUCCESS);
                                    _connectedClients[clientId]._currentRoom.game.SetMap(request);
                                    SendMapChangedData(_connectedClients[clientId]._currentRoom);
                                }
                                else
                                {
                                    (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_MAP_CHANGE_FAILURE);
                                }
                            }
                            break;
                        }

                    case Messages.ROOM_CHANGE_DIFFICULTY_REQUEST:
                        {
                            UInt16 request = BitConverter.ToUInt16(data, 0);
                            message_number_send = Messages.ROOM_CHANGE_DIFFICULTY_RESPONSE;

                            //check if user requesting change is admin
                            try
                            {
                                if (_connectedClients[clientId]._currentRoom._roomAdmin._clientId == clientId)
                                {
                                    if (request <= Constants.ROOM_DIFFICULTY_HARD)
                                    {
                                        _connectedClients[clientId]._currentRoom.game.SetDifficulty(request);
                                        (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_DIFFICULTY_CHANGE_SUCCESS);
                                    }
                                    else
                                    {
                                        _connectedClients[clientId]._currentRoom.game.SetDifficulty(Constants.ROOM_DIFFICULTY_EASY);
                                        (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(message_number_send, Constants.ROOM_DIFFICULTY_CHANGE_FAILURE);
                                    }
                                    SendDifficultyChangedData(_connectedClients[clientId]._currentRoom);
                                }
                            }
                            catch
                            {
                                Console.WriteLine("Welp, nice checking...");
                            }
                            break;
                        }
                        //Used in private rooms
                    case Messages.ROOM_GAME_START_REQUEST:
                        {
                            if(_connectedClients[clientId]._currentRoom._roomAdmin._clientId == clientId)
                            {
                                if(!_connectedClients[clientId]._currentRoom._isEmpty)
                                {
                                    _connectedClients[clientId]._currentRoom.StartGame();
                                }
                                else
                                {
                                    (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(Messages.ROOM_GAME_START_RESPONSE, Constants.ROOM_GAME_STARTED_FAILURE);
                                }
                            }
                            else
                            {
                                (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(Messages.ROOM_GAME_START_RESPONSE, Constants.ROOM_GAME_STARTED_FAILURE);
                            }
                            break;
                        }

                    #endregion
                    #region ADMIN_STUFF
                    case Messages.ADMIN_LOGIN_REQUEST:
                        {
                            string password = Encoding.ASCII.GetString(data);
                            
                            if(password == Constants.ADMIN_LOGIN_PASSWORD)
                            {
                                (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(Messages.ADMIN_LOGIN_RESPONSE, Constants.ADMIN_LOGIN_SUCCESS);
                                _connectedClients[clientId]._isAdmin = true;
                            }
                            else
                            {
                                (lengthToSend, dataToSendTemp) = packetHelper.UInt16ToBytes(Messages.ADMIN_LOGIN_RESPONSE, Constants.ADMIN_LOGIN_FAILURE);
                            }

                            break;
                        }

                    case Messages.ADMIN_DUMP_USERS_TO_FILE:
                        {
                            if (!_connectedClients[clientId]._isAdmin)
                                break;

                            Console.WriteLine("[ADMIN]Dump users to file.");

                            using (StreamWriter file = new StreamWriter("connected_clients.txt"))
                                foreach (var entry in _connectedClients)
                                    file.WriteLine("[ ClientId: {0} User Name: {1} IP: {2} ]", entry.Key, entry.Value._userName, entry.Value._socket.RemoteEndPoint);

                            break;
                        }
                    case Messages.ADMIN_DUMP_GAMES_TO_FILE:
                        {
                            if (!_connectedClients[clientId]._isAdmin)
                                break;

                            Console.WriteLine("[ADMIN]Dump games to file.");
                            Console.WriteLine("Public game count: " + _publicRooms.Count);
                            Console.WriteLine("Private game count: " + _privateRooms.Count);
                            using (StreamWriter file = new StreamWriter("public_games.txt"))
                                foreach (var entry in _publicRooms)
                                    file.WriteLine("[ GameID: {0}\n Game Name: {1}]", entry.Key, entry.Value._roomName);

                            using (StreamWriter file = new StreamWriter("private_games.txt"))
                                foreach (var entry in _privateRooms)
                                file.WriteLine("[ GameID: {0}\n Game Name: {1}]", entry.Key, entry.Value._roomName);

                            break;
                        }

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
                    try
                    {
                        socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                    }
                    catch
                    {

                    }
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
                int id = _connectedClients[socket.GetHashCode()]._currentRoom._roomAdmin._clientId;

                //if user is admin, kick other guy
                if(socket.GetHashCode() == id)
                {
                    Room r = _connectedClients[socket.GetHashCode()]._currentRoom;
                    try
                    {
                        SendDataRoomAbandoned(_connectedClients[socket.GetHashCode()]._currentRoom.refClients[1]._socket, _connectedClients[socket.GetHashCode()]._currentRoom.refClients[1]._clientId);
                    }
                    catch
                    { }
                    roomHelper.DestroyRoom(r);
                }
                else
                {
                    SendDataPlayerLeftRoom(id);
                    _connectedClients[socket.GetHashCode()]._currentRoom.RemovePlayer2();
                }
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
            byte[] dataToSendP2 = packetHelper.JoinedRoomDataToBytes(Messages.ROOM_JOINED_PUBLIC_ROOM, r._roomId, r.game._mapId, r.game._difficulty, r._roomAdmin._userName);

            Console.WriteLine("Sending ROOM data to players!");
            Console.WriteLine("Player 1" + r._roomAdmin._userName);
            serverHelper.PrintSendingData(c._clientId, Messages.ROOM_JOINED_MY_ROOM, Convert.ToUInt16(dataToSendP1.Length), ref dataToSendP1);
            Console.WriteLine("Player 2" + c._userName);
            serverHelper.PrintSendingData(c._clientId, Messages.ROOM_JOINED_PUBLIC_ROOM, Convert.ToUInt16(dataToSendP2.Length), ref dataToSendP2);

            c.EnteredRoom(ref r);
            r.AddPlayer(ref c);
            //player 1
            try
            {
                r._roomAdmin._socket.BeginSend(dataToSendP1, 0, dataToSendP1.Length, SocketFlags.None, new AsyncCallback(SendCallback), r._roomAdmin._socket);
            }
            catch { }
            //player 2
            try
            {
                c._socket.BeginSend(dataToSendP2, 0, dataToSendP2.Length, SocketFlags.None, new AsyncCallback(SendCallback), c._socket);
            }
            catch { }
           
        }

        public static (UInt16, byte[]) SendDataJoinedPrivateRoom(Room r, Client c)
        {
            byte[] dataToSendP1 = packetHelper.StringToBytes(Messages.ROOM_JOINED_MY_ROOM, c._userName);

            byte[] dataToSendP2 = packetHelper.JoinedPrivateRoomDataToBytes(Messages.ROOM_JOIN_PRIVATE_ROOM_RESPONSE, Constants.ROOM_PRIVATE_JOIN_SUCCESS, r.game._mapId, r.game._difficulty, r._roomName, r._roomAdmin._userName);

            Console.WriteLine("Sending ROOM data to players!");
            Console.WriteLine("Player 1" + r._roomAdmin._userName);
            serverHelper.PrintSendingData(c._clientId, Messages.ROOM_JOINED_MY_ROOM, Convert.ToUInt16(dataToSendP1.Length), ref dataToSendP1);

            //player 1
            try
            {
                r._roomAdmin._socket.BeginSend(dataToSendP1, 0, dataToSendP1.Length, SocketFlags.None, new AsyncCallback(SendCallback), r._roomAdmin._socket);
            }
            catch { }

            //player 2
            c.EnteredRoom(ref r);
            r.AddPlayer(ref c);

            return (Convert.ToUInt16(dataToSendP2.Length), dataToSendP2);
        }

        public static void SendDataGameInitiated(Room r)
        {
            byte[] dataToSend = new byte[4];
            packetHelper.FillHeaderBlankData(Messages.ROOM_GAME_INITIATED, ref dataToSend);
            try
            {
                r.refClients[0]._socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), r.refClients[0]._socket);
                r.refClients[1]._socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), r.refClients[1]._socket);
            }
            catch
            {

            }
        }

        public static void SendDataGameStarted(Room r)
        {
            
            byte[] dataToSend = packetHelper.UInt16ToBytesNoLen(Messages.ROOM_GAME_STARTED, r.game._selectedMap.GetSpawnPointIndex());
            try
            {
                r.refClients[0]._socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), r.refClients[0]._socket);
                r.refClients[1]._socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), r.refClients[1]._socket);
            }
            catch
            {

            }
        }

        public static void SendDataRoomAbandoned(Socket s, int id)
        {
            byte[] dataToSend = new byte[Constants.MESSAGE_BASE];

            _connectedClients[id].ClearStatus();
            packetHelper.FillHeaderBlankData(Messages.ROOM_CLOSED, ref dataToSend);
            s.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), s);
        }

        public static void SendDataPlayerLeftRoom(int id)
        {
            byte[] dataToSend = new byte[Constants.MESSAGE_BASE];

            packetHelper.FillHeaderBlankData(Messages.ROOM_PLAYER_LEFT_MY_ROOM, ref dataToSend);
            if (_connectedClients.ContainsKey(id))
            {
                _connectedClients[id]._socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), _connectedClients[id]._socket);
            }
        }

        public static void SendMapChangedData(Room r)
        {
            byte[] dataToSend = packetHelper.UInt16ToBytesNoLen(Messages.ROOM_MAP_CHANGED, r.game._mapId);

            if(r.refClients[1] != null)
            {
                try
                {
                    r.refClients[1]._socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), r.refClients[1]._socket);
                }
                catch
                {
                    Console.WriteLine("Socket closed");
                }
            }
        }

        public static void SendDifficultyChangedData(Room r)
        {
            byte[] dataToSend = packetHelper.UInt16ToBytesNoLen(Messages.ROOM_DIFFICULTY_CHANGED, r.game._difficulty);
            
            if (r.refClients[1] != null)
            {
                try
                {
                    r.refClients[1]._socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), r.refClients[1]._socket);
                }
                catch
                {
                    Console.WriteLine("Socket closed");
                }
            }
        }

        //Queue system
        private static void QueueSystem(Object src, System.Timers.ElapsedEventArgs e)
        {
            //Console.WriteLine("Dumping Queue Number of people waiting: " + _clientQueue.Count + " Number of public rooms: " + _publicRooms.Count);
            if (_clientQueue.Count > 0)
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
                int id = _connectedClients[socket.GetHashCode()]._currentRoom._roomAdmin._clientId;

                //if user is admin, kick other guy
                if (socket.GetHashCode() == id)
                {
                    Room r = _connectedClients[socket.GetHashCode()]._currentRoom;
                    try
                    {
                        SendDataRoomAbandoned(_connectedClients[socket.GetHashCode()]._currentRoom.refClients[1]._socket, _connectedClients[socket.GetHashCode()]._currentRoom.refClients[1]._clientId);
                    }
                    catch
                    {

                    }
                    roomHelper.DestroyRoom(r);
                }
                else
                {
                    SendDataPlayerLeftRoom(id);
                    _connectedClients[socket.GetHashCode()]._currentRoom.RemovePlayer2();
                }
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

        private static void ClearLogs()
        {
            if (File.Exists("connected_clients.txt"))
            {
                File.Delete("connected_clients.txt");
            }

            if (File.Exists("public_games.txt"))
            {
                File.Delete("public_games.txt");
            }

            if (File.Exists("private_games.txt"))
            {
                File.Delete("private_games.txt");
            }
        }
    }

}