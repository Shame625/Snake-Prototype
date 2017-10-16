using System.Net.Sockets;

namespace SnakeServer
{
    class Client
    {
        public int _clientId { get; }
        Socket _socket { get; }

        public Client(int id, Socket socket)
        {
            _clientId = id;
            _socket = socket;
        }
    }
}