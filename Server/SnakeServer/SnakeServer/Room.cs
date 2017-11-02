using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{

    public class Room
    {
        PacketHelper packetHelper = new PacketHelper();

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
            }
            _isEmpty = true;
            
            StopGameStart();
        }

        System.Timers.Timer gameTimer = new System.Timers.Timer();
        bool firstTick = false;

        float currentTime;
        float gameTimeout;
        float tickSeconds;
        bool prepare = true;

        public UInt16 StartGame()
        {
            if (!_isEmpty && !game._gameInProgress)
            {
                Console.WriteLine("Game ID: " + _roomId + " started!");

                prepare = true;
                currentTime = 0;
                gameTimeout = Constants.ROOM_GAME_TIME_LIMIT / ReturnTickSpeed();
                tickSeconds = ReturnTickSpeed() / 1000;

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
            EndGame(0xFF);
        }

        public void EndGame(byte status)
        {
            gameTimer.Stop();
            gameTimer.Dispose();

            game.Reset();
            Console.WriteLine("game status: " + status);

            byte[] dataToSendP1;
            byte[] dataToSendP2;

            //send data of game status to players
            if (status == Constants.GAME_DRAW)
            {
                Console.WriteLine("Game ID:" + _roomId + " ended. DRAW!");

                (_, dataToSendP1) = packetHelper.ByteToBytes(Messages.GAME_ENDED, Constants.GAME_DRAW);
                Program.GameEnded(ref dataToSendP1, ref dataToSendP1, this);

            }
            else if (status == Constants.GAME_WON_P1)
            {
                Console.WriteLine("Game ID:" + _roomId + " ended. P1 Won!");
                (_, dataToSendP1) = packetHelper.ByteToBytes(Messages.GAME_ENDED, Constants.GAME_WON_P1);
                (_, dataToSendP2) = packetHelper.ByteToBytes(Messages.GAME_ENDED, Constants.GAME_LOST_P1);

                Program.GameEnded(ref dataToSendP1, ref dataToSendP2, this);
            }
            else if (status == Constants.GAME_LOST_P1)
            {
                Console.WriteLine("Game ID:" + _roomId + " ended. P2 Won!");
                (_, dataToSendP1) = packetHelper.ByteToBytes(Messages.GAME_ENDED, Constants.GAME_LOST_P1);
                (_, dataToSendP2) = packetHelper.ByteToBytes(Messages.GAME_ENDED, Constants.GAME_WON_P1);

                Program.GameEnded(ref dataToSendP1, ref dataToSendP2, this);
            }
            else
            {
                Console.WriteLine("Game ID:" + _roomId + " ended.");

                (_, dataToSendP1) = packetHelper.ByteToBytes(Messages.GAME_ENDED, Constants.GAME_ENDED);
                Program.GameEnded(ref dataToSendP1, ref dataToSendP1, this);
            } 
        }

        int preparation = 4;

        void GameLoop(object src, System.Timers.ElapsedEventArgs e)
        {
            //Logic packets
            if (!firstTick)
            {
                Console.WriteLine("Loop called from Room ID:" + _roomId + " First Tick done!");
                firstTick = true;
                gameTimer.Interval = 1000;
                game._gameInProgress = true;
                Program.SendDataGameStarted(this);
            }
            //Loop
            else if(prepare == true)
            {
                preparation--;
                Console.WriteLine("Loop called from Room ID:" + _roomId + " Preparation: " + preparation + " .");
                if (preparation == 1)
                {
                    prepare = false;
                    gameTimer.Interval = ReturnTickSpeed();
                }
            }
            else if(game._gameInProgress)
            {
                currentTime += tickSeconds;
                Console.WriteLine(DateTime.Now.ToLongTimeString());

                byte status = game.Mover();


                if(currentTime >= gameTimeout || status != 0xFF)
                {
                    //Send game ended packet from EndGame()
                    EndGame(status);
                }
                else
                {
                    //Send location packets
                    /*
                    byte[] p1Data = packetHelper.MovementPacketOld(Messages.GAME_MOVEMENT, game.P2Direction, 0, 0, 0, 0, 0, 0);
                    byte[] p2Data = packetHelper.MovementPacketOld(Messages.GAME_MOVEMENT, game.P1Direction, 0, 0, 0, 0, 0, 0);
                    */
                    byte[] movementPacket = packetHelper.MovementPacket(Messages.GAME_MOVEMENT, game._P1blocks, game._P2blocks, game.bugLocationP1, game.bugLocationP2);
                    Program.SendLocationData(ref movementPacket, this);
                }
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
