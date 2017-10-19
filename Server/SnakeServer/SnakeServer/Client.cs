using System;
using System.Linq;
using System.Net.Sockets;

namespace SnakeServer
{
    class Client
    {
        public int _clientId { get; }
        Socket _socket { get; }
        public string _userName { get; set; }
        private bool _isAllowed = false;

        public Client(int id, Socket socket)
        {
            _clientId = id;
            _socket = socket;
        }

        public UInt16 SetName(ref string name)
        {
            if (name.Length <= Constants.USERNAME_LENGTH_MIN || name.Length >= Constants.USERNAME_LENGTH_MAX || name.Any(ch => !Char.IsLetterOrDigit(ch)))
                return Constants.USERNAME_BAD;
            else if (_isAllowed)
                return Constants.USER_LOGGED_IN;

            _isAllowed = true;
            _userName = name;

            return Constants.USERNAME_OK;
        }
    }
}