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
        public bool _findingRoom { get; set; }

        public bool _isInRoom { get; set; }
        public Room _currentRoom { get; set; }

        public Client(int id, Socket socket)
        {
            _clientId = id;
            _socket = socket;
            _isAllowed = false;
            _isInRoom = false;
            _findingRoom = false;
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
            Program._usedNames.Add(name, _clientId);

            return Constants.USERNAME_OK;
        }

        public void EnteredRoom(ref Room r)
        {
            _currentRoom = r;
            _isInRoom = true;
            _findingRoom = false;
        }
        public void LeftRoom()
        {
            _currentRoom = null;
            _isInRoom = false;
            _findingRoom = false;
        }


        public void ClearStatus()
        {
            _currentRoom = null;
            _isInRoom = false;
            _findingRoom = false;
        }
    }
}