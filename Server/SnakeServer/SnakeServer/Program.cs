using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SnakeServer
{
    class Program
    {
        public static int counter = 0;

        private static byte[] _buffer = new byte[1024];

        private static Dictionary<int, Client> _connectedClients = new Dictionary<int, Client>();
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static ServerHelper serverHelper = new ServerHelper();
        private static PacketHelper packetHelper = new PacketHelper();

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
                    return;
                }
                else if(recieved > 1024)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(false);
                }

                counter++;

                byte[] dataBuff = new byte[recieved];
                Array.Copy(_buffer, dataBuff, recieved);

                //Breaking down buffer into segments
                byte[] message_number_bytes = new byte[2];
                byte[] packet_length_bytes = new byte[2];
                byte[] data = new byte[recieved - 4];

                UInt16 message_number = UInt16.MaxValue, packet_length = UInt16.MaxValue;

                if (recieved >= 4)
                {
                    Array.Copy(dataBuff, message_number_bytes, 2);
                    Array.Copy(dataBuff, 2, packet_length_bytes, 0, 2);
                    Array.Copy(dataBuff, 4, data, 0, dataBuff.Length - 4);

                    //Turn bytes to usuable data
                    (message_number, packet_length) = serverHelper.BytesToMessageLength(message_number_bytes, packet_length_bytes);
                }

                int clientId = socket.GetHashCode();

                //Print to console recieved data
                serverHelper.PrintRecievedData(clientId, message_number, packet_length, ref data);

                //Packet handling recieving/sending
                UInt16 message_number_send = UInt16.MaxValue;
                UInt16 lengthToSend = 0;
                byte[] dataToSendTemp = new byte[512];

                #region MESSAGE HANDLING
                switch(message_number)
                {
                    case Messages.SET_NAME_REQUEST:
                        {
                            string temp_name = Encoding.ASCII.GetString(data);
                            UInt16 response = _connectedClients[clientId].SetName(ref temp_name);

                            //Send Data
                            {
                                message_number_send = Messages.SET_NAME_RESPONSE;
                                (lengthToSend, dataToSendTemp) = packetHelper.sendUInt16(message_number_send, response);
                            }
                            break;
                        }
                    default:

                        break;
                }
                #endregion


                byte[] dataToSend = new byte[lengthToSend];
                Array.Copy(dataToSendTemp, dataToSend, lengthToSend);

                if (lengthToSend > 0)
                {
                    serverHelper.PrintSendingData(clientId, message_number_send, lengthToSend, ref dataToSend);
                    socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }
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
    }
}
