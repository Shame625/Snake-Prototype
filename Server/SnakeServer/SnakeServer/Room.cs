using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{

    class Room
    {
        public UInt16 _type { get; }

        private int _roomId;
        public string _roomName { get; }
        string _roomPassword;

        int[] _players;

        int _roomAdmin;

        public Room(UInt16 t, int id, string name, string password)
        {
            _type = t;
            _roomId = id;
            _roomAdmin = id;

            if (_type == Constants.ROOM_TYPE_PRIVATE)
            {
                _roomName = name;
                _roomPassword = password;
            }
        }



    }
}
