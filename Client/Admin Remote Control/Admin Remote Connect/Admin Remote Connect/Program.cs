using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Admin_Remote_Connect
{
    class Program
    {
        private static readonly Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] buffer = new byte[1024];

        static IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 50000);
        private static byte[] message_number_bytes = new byte[2];
        private static byte[] packet_length_bytes = new byte[2];
        private static ushort message_number;
        private static ushort packet_length;

        static bool loggedIn = false;

        private static void ConnectToServer()
        {
            int attempts = 0;

            while (!clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    clientSocket.Connect(IPAddress.Loopback, 50000);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }

        static void Main(string[] args)
        {
            string input = "connect";
            while (!clientSocket.Connected)
            {
                
                if (input == Constants.CLIENT_CONNECT)
                    ConnectToServer();

                else if (input == Constants.CLIENT_EXIT)
                    return;

                if(!clientSocket.Connected)
                    input = Console.ReadLine();
            }


            Console.WriteLine("Welcome to the Admin RC tool");
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);

            while (input != Constants.CLIENT_EXIT)
            {
                input = Console.ReadLine();
                string to_send = "";

                if(input == Constants.CLIENT_LOGIN)
                {
                    Console.WriteLine("Enter password: ");
                    to_send = Console.ReadLine();

                    byte[] b = PacketHelper.LoginData(Messages.ADMIN_LOGIN_REQUEST, to_send);

                    clientSocket.Send(b);
                }

                if(loggedIn)
                {

                }
                else
                {
                    Console.WriteLine("You must log in.");
                }
            }
        }


        private static void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = clientSocket.EndReceive(AR);
                

                if (received == 0)
                {
                    return;
                }

                byte[] dataBuff = new byte[received];
                Array.Copy(buffer, dataBuff, received);

                byte[] data = new byte[received - 4];

                if (received >= 4)
                {
                    Array.Copy(dataBuff, message_number_bytes, 2);
                    Array.Copy(dataBuff, 2, packet_length_bytes, 0, 2);

                    Array.Copy(dataBuff, 4, data, 0, dataBuff.Length - 4);

                    //Turn bytes to usuable data
                    PacketHelper.BytesToMessageLength(ref message_number_bytes, ref packet_length_bytes, ref message_number, ref packet_length);
                }

                Console.WriteLine(PrintBytes(ref dataBuff));
                Console.Clear();

                switch (message_number)
                {
                    case Messages.ADMIN_LOGIN_RESPONSE:
                        {
                            UInt16 response = BitConverter.ToUInt16(data, 0);
                            
                            if(response == Constants.ADMIN_LOGIN_SUCCESS)
                            {
                                Console.WriteLine("Succesffuly logged in!");
                                loggedIn = true;
                            }
                            else
                            {
                                Console.WriteLine("Wrong password");
                            }

                            break;
                        }
                    default:
                        break;
                }
                

                // Start receiving data again.
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);

            }
            // Avoid Pokemon exception handling in cases like these.
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static string PrintBytes(ref byte[] byteArray)
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
    }
}
