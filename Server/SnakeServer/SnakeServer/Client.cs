using System;
using System.Linq;
using System.Net.Sockets;

namespace SnakeServer
{
    
    public class Client
    {
        public int _clientId { get; }
        public Socket _socket { get; set; }
        public string _userName { get; set; }
        public bool _isAllowed{ get; set; }

        public Client(int id, Socket socket)
        {
            _clientId = id;
            _socket = socket;
            _isAllowed = false;
        }

        public UInt16 SetName(ref string name)
        {
            if (name.Length <= Constants.USERNAME_LENGTH_MIN || name.Length >= Constants.USERNAME_LENGTH_MAX || name.Any(ch => !Char.IsLetterOrDigit(ch)))
                return Constants.USERNAME_BAD;
            else if (_isAllowed)
                return Constants.USER_LOGGED_IN;
            else if (ServerHelper.CheckIfUserNameInUse(ref name, ref Program._usedNames))
                return Constants.USERNAME_IN_USE;

            _isAllowed = true;
            _userName = name;
            Program._usedNames.Add(name, true);

            return Constants.USERNAME_OK;
        }
    }
}