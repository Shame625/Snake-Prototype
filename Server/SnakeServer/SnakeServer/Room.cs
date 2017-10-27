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

        public int _roomId { get; }
        public string _roomName { get; }
        public string _roomPassword { get; }

        public Client[] refClients = new Client[2];

        public Client _roomAdmin { get; set; }

        public bool _isEmpty { get; set; }

        public Game game;

        System.Threading.Timer gameLoopTimer;

        public Room(Client c, UInt16 t, int id, string name, string password)
        {
            _type = t;
            _roomId = id;
            _roomAdmin = c;
            _isEmpty = true;
            refClients[0] = c;

            game = new Game();

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
            _isEmpty = true;
            refClients[0] = c;

            game = new Game();
        }

        public void AddPlayer(ref Client c)
        {
            if(c._isAllowed)
            {
                refClients[1] = c;
            }
            _isEmpty = false;
        }

        public void RemovePlayer2()
        {
            if(refClients[1] != null)
            {
                refClients[1] = null;
            }
            _isEmpty = true;
        }

        public void StartGame()
        {
            Console.WriteLine("Game started");

            TimeSpan startTimeSpan = TimeSpan.Zero;
            TimeSpan periodTimeSpan = TimeSpan.FromMilliseconds(100);

            gameLoopTimer = new System.Threading.Timer((e) =>
            {
                GameLoop();
            }, null, startTimeSpan, periodTimeSpan);
        }

        public void EndGame()
        {
            Console.WriteLine("Game ended");
            gameLoopTimer.Dispose();
        }

        void GameLoop()
        {
            Program.SendDataJoinedRoom(this, refClients[0]);
        }

    }
}
