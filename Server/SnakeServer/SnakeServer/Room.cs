using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{

    public class Room
    {
        public UInt16 _type { get; }

        private int _roomId;
        public string _roomName { get; }
        string _roomPassword;

        Client[] refClients = new Client[2];

        public Client _roomAdmin { get; set; }

        public Room(Client c, UInt16 t, int id, string name, string password)
        {
            _type = t;
            _roomId = id;
            _roomAdmin = c;

            if (_type == Constants.ROOM_TYPE_PRIVATE)
            {
                _roomName = name;
                _roomPassword = password;
            }
        }

        public Room(Client c, UInt16 t, int id)
        {
            _type = t;
            _roomId = id;
            _roomAdmin = c;
        }

        public void AddPlayer(ref Client c)
        {
            if(c._isAllowed)
            {
                if(!c._isInRoom)
                {
                    if(refClients[0] == null)
                    {
                        refClients[0] = c;
                    }
                    else if(refClients[1] == null)
                    {
                        refClients[1] = c;
                    }
                }
            }
        }

        public void SetAdmin(Client c)
        {

        }

    }
}
