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
                if (_type == Constants.ROOM_TYPE_PUBLIC)
                    StartGame();
            }
            _isEmpty = false;
        }

        public void RemovePlayer2()
        {
            if(refClients[1] != null)
            {
                refClients[1] = null;
                game._gameInProgress = false;
            }
            _isEmpty = true;

            StopGameStart();
        }

        System.Timers.Timer gameTimer = new System.Timers.Timer();
        bool firstTick = false;

        public void DisposeOfTimer()
        {
            gameTimer.Stop();
            gameTimer.Dispose();
        }

        public UInt16 StartGame()
        {
            if (!_isEmpty && !game._gameInProgress)
            {
                Console.WriteLine("Game ID: " + _roomId + " started!");

                //start timer here
                gameTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.GameLoop);
                gameTimer.Interval = Constants.ROOM_GAME_TIME_TO_START;
                gameTimer.Start();

                Program.SendDataGameInitiated(this);
                return Constants.ROOM_GAME_STARTED_SUCCESS;
            }
            return Constants.ROOM_GAME_STARTED_FAILURE;
        }

        void StopGameStart()
        {
            gameTimer.Stop();
            Console.WriteLine("Game ID: " + _roomId + " stopped!");
        }

        public void EndGame()
        {
            gameTimer.Stop();
            Console.WriteLine("Game ID:" + _roomId + "ended");
        }

        void GameLoop(object src, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Loop called from Room ID:" + _roomId);
            //Logic packets
            if (!firstTick)
            {
                firstTick = true;
                gameTimer.Interval = ReturnTickSpeed();
                game._gameInProgress = true;
                Program.SendDataGameStarted(this);
            }
            //Loop
            else
            {
                Console.WriteLine(DateTime.Now.ToLongTimeString());
            }
        }

        int ReturnTickSpeed()
        {
            if (game._difficulty == Constants.ROOM_DIFFICULTY_EASY)
                return Constants.ROOM_GAME_TICKS_EASY;

            else if (game._difficulty == Constants.ROOM_DIFFICULTY_NORMAL)
                return Constants.ROOM_GAME_TICKS_NORMAL;

            else
                return Constants.ROOM_GAME_TICKS_HARD;
        }

    }
}
