using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
    bool isLoopInvoked = false;

    NetworkManager networkManager;
    GameManager gameManager;
    UIManager uiManager;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        gameManager = GetComponent<GameManager>();
        uiManager = GetComponent<UIManager>();    
    }

    public void Connect()
    {
        uiManager.ConnectUI();

        networkManager.attempts = 0;

        if (!isLoopInvoked)
            InvokeRepeating("CheckConnection", 0, 1);

        isLoopInvoked = true;
    }

    public void ConnectionHeartbeat()
    {
        if (!networkManager.GetStatusConnection())
        {
            uiManager.DisconnectedFromServerUI(Constants.LOGOUT_FORCED);
            CancelInvoke("ConnectionHeartbeat");
        }
    }

    void CheckConnection()
    {
        if (networkManager.attempts == Constants.LOOP_MAX)
        {
            uiManager.CheckConnectionUI();
            isLoopInvoked = false;
            CancelInvoke("CheckConnection");

            return;
        }

        if (networkManager.GetStatusConnection())
        {
            CancelInvoke("CheckConnection");
            InvokeRepeating("ConnectionHeartbeat", 0, 5);

            uiManager.DisplaySubmitUserNamePanelUI();

            isLoopInvoked = false;
        }
        else
        {
            networkManager.LoopConnect();
            networkManager.attempts++;

            if (networkManager.attempts == 1)
            {
                uiManager.DisplayConnectToServerPanelUI();
            }
        }

        uiManager.CheckConnectionUI();
    }

    public void idRecieved(int id)
    {
        gameManager.player._id = id;
    }

    public void userNameOK()
    {
        gameManager.player.SetName(ref gameManager.enteredName);
        uiManager.DisplayMainMenuUI();
    }

    public void userNameBAD(UInt16 err)
    {
        if(err == Constants.USER_LOGGED_IN)
            uiManager.DisplayMainMenuUI();
        uiManager.UserNameErrorUI(ref err);
    }

    public void loggedOut(UInt16 code)
    {
        CancelInvoke("ConnectionHeartbeat");
        if(code == Constants.LOGOUT_NORMAL)
            Application.Quit();

        uiManager.DisconnectedFromServerUI(code);
    }

    public void roomCreatedSuccessfull()
    {
        MapManager.Reset();
        gameManager.currentRoom.game.ClearGame();

        gameManager.player._inRoom = true;

        gameManager.currentRoom.roomId = gameManager.player._id;
        gameManager.currentRoom.roomType = gameManager.enteredroomType;
        gameManager.currentRoom.isAdmin = true;

        if (gameManager.currentRoom.roomType == Constants.ROOM_TYPE_PRIVATE)
            gameManager.currentRoom.roomName = gameManager.enteredRoomName;

        uiManager.ClearUI();
        uiManager.EnableElementsAdminUI();
        uiManager.RoomCreatedUI();
    }

    public void MapSetSuccess()
    {
        gameManager.currentRoom.game.SetMap(MapManager.GetCurrentMapIndex());
        uiManager.SetSelectedMapUI();
    }

    public void MapSetFail()
    {
        MapManager.FailedToChangeMap();
        uiManager.ShowErrorPanel(Constants.ROOM_FAILED_SET_MAP_MSG);
    }

    public void MapChanged(UInt16 mapId)
    {
        gameManager.currentRoom.game.SetMap(mapId);
        MapManager.MapChanged(mapId);
        uiManager.SetSelectedMapUI();
    }

    public void playerJoinedMyRoom(string n)
    {
        gameManager.opponent._userName = n;
        uiManager.PlayerJoinedMyRoomUI();
        uiManager.SetSelectedMapUI();
    }

    public void playerLeftMyRoom()
    {
        gameManager.opponent._userName = "";

        uiManager.ShowErrorPanel(Constants.ROOM_PLAYER_LEFT_MY_ROOM);
        uiManager.PlayerLeftMyRoomUI();
    }

    public void joinedRoom(int id, string name, UInt16 mapId, UInt16 difficulty)
    {
        gameManager.opponent._userName = name;
        gameManager.currentRoom.isAdmin = false;
        gameManager.player._findingRoom = false;
        gameManager.player._inRoom = true;

        MapManager.MapChanged(mapId);
        gameManager.currentRoom.game.SetMap(mapId);
        gameManager.currentRoom.game._difficulty = difficulty;

        uiManager.DisableElementsNotAdminUI();
        uiManager.SetSelectedMapUI();
        uiManager.SetDifficultyUI(difficulty);

        uiManager.JoinedRoomUI();
    }

    public void roomClosed()
    {
        gameManager.opponent._userName = "";
        gameManager.currentRoom.roomId = 0;
        gameManager.currentRoom.isAdmin = false;
        gameManager.player._inRoom = false;
        gameManager.player._findingRoom = false;

        MapManager.Reset();
        uiManager.ResetClockUI();
        gameManager.currentRoom.game.ClearGame();

        uiManager.RoomUI();
        uiManager.ClearUI();
        uiManager.ShowErrorPanel(Constants.ROOM_ABANDONED_MSG);
        uiManager.DisplayFindGamePanelUI();
    }

    public void roomJoinSuccess()
    {
        gameManager.player._findingRoom = true;
        uiManager.ShowFindingPublicGamePanelUI();
    }

    public void roomJoinFailed()
    {
        uiManager.ShowErrorPanel(Constants.ROOM_JOIN_FAILURE_MSG);
    }

    public void roomCancelFindingSuccess()
    {
        gameManager.player._findingRoom = false;
        uiManager.DisplayFindGamePanelUI();
    }

    public void roomCancelFindingFailed()
    {
        uiManager.ShowErrorPanel(Constants.ROOM_CANCEL_FINDING_FAILURE_MSG);
        uiManager.DisplayFindGamePanelUI();
    }

    public void roomCreationFailed(ref UInt16 errorCode)
    {
        uiManager.RoomCreationFailedUI(errorCode);
    }

    public void roomPrivateJoinedSuccess(string roomName, string opponentName, UInt16 mapId, UInt16 difficulty)
    {
        gameManager.currentRoom.roomName = roomName;
        gameManager.currentRoom.isAdmin = false;

        MapManager.MapChanged(mapId);
        gameManager.currentRoom.game.SetMap(mapId);
        gameManager.currentRoom.game._difficulty = difficulty;

        gameManager.opponent._userName = opponentName;
        gameManager.currentRoom.roomType = Constants.ROOM_TYPE_PRIVATE;

        gameManager.player._findingRoom = false;
        gameManager.player._inRoom = true;

        uiManager.DisableElementsNotAdminUI();
        uiManager.SetSelectedMapUI();
        uiManager.SetDifficultyUI(difficulty);
        uiManager.DisplayRoomPanelUI(true);
    }

    public void roomPrivateJoinedFailure(ref UInt16 errorCode)
    {
        uiManager.RoomPrivateErrorUI(errorCode);
    }

    public void roomAbandonedCheck(ref UInt16 errorCode)
    {
        if(errorCode == Constants.ROOM_ABANDONED_SUCCESS)
        {
            gameManager.currentRoom.roomId = 0;
            gameManager.player._inRoom = false;

            MapManager.Reset();
            gameManager.currentRoom.game.ClearGame();
            uiManager.ResetClockUI();
        }
        uiManager.AbandonRoomUI(errorCode);
    }

    public void leaveRoomSuccessful()
    {
        gameManager.player._inRoom = false;

        gameManager.currentRoom.game.ClearGame();
        MapManager.Reset();

        uiManager.ShowErrorPanel(Constants.ROOM_LEAVE_SUCCESS_MSG);
        uiManager.ResetClockUI();
        uiManager.ClearUI();
        uiManager.DisplayVsPlayerPanelUI();
    }

    public void leaveRoomFailed()
    {
        uiManager.ShowErrorPanel(Constants.ROOM_LEAVE_FAILED_MSG);
    }

    public void DifficultyChanged(UInt16 id)
    {
        gameManager.currentRoom.game._difficulty = id;
        uiManager.SetDifficultyUI(id);
    }

    public void DifficultySetFail()
    {
        gameManager.currentRoom.game._difficulty = Constants.ROOM_DIFFICULTY_EASY;
        uiManager.SetDifficultyUI(Constants.ROOM_DIFFICULTY_EASY);
        uiManager.ShowErrorPanel(Constants.ROOM_FAILED_SET_DIFFICULTY_MSG);
    }

    public void GameInitiated()
    {
        uiManager.GameInitiatedUI();
    }


    internal void GameFailedToStart()
    {
        uiManager.ShowErrorPanel(Constants.ROOM_FAILED_TO_START_MSG);
    }

    public void GameStarted(UInt16 spawnIndex)
    {
        //do something with it

        MapManager.Position pos = MapManager.getCoordinates(spawnIndex);

        gameManager.StartGame();
        uiManager.GameStartedUI();
    }

    public void MovementOld(ref byte direction, ref byte iExpanded, ref byte opponentExpaded, ref byte myNewBug, ref UInt16 myIndexBug, ref byte opponentNewBug, ref UInt16 opponentIndexBug)
    {
        //do this after block loops
        gameManager.currentRoom.game._currentLocationP2 = gameManager.currentRoom.game.ReturnNewIndex(gameManager.currentRoom.game.P2Direction, gameManager.currentRoom.game._currentLocationP2);

        gameManager.currentRoom.game.P2Direction = direction;
        //Gotta implement bug things and expansions

        //Call GameLoop
        gameManager.GameLoop();
    }

    public void Movement(List<UInt16> p1Blocks, List<UInt16> p2Blocks, ref UInt16 bugLocationP1, ref UInt16 bugLocationP2)
    {
        gameManager.currentRoom.game._P1blocks = p1Blocks;
        gameManager.currentRoom.game._P2blocks = p2Blocks;

        gameManager.currentRoom.game.bugLocationP1 = bugLocationP1;
        gameManager.currentRoom.game.bugLocationP2 = bugLocationP2;

        gameManager.prevDirection = gameManager.currentRoom.game.P1Direction;

        gameManager.GameLoop();
    }

    public void ChangeMyDirection(ref byte direction)
    {
        gameManager.currentRoom.game.P1Direction = direction;
        Debug.Log(gameManager.currentRoom.game.P1Direction);
    }

    public void GameEnded(ref byte response)
    {
        if(response == Constants.GAME_DRAW)
        {
            uiManager.ShowErrorPanel(Constants.GAME_DRAW_MSG);
        }
        else if (response == Constants.GAME_WON_P1)
        {
            uiManager.ShowErrorPanel(Constants.GAME_WON_MSG);
        }
        else if (response == Constants.GAME_LOST_P1)
        {
            uiManager.ShowErrorPanel(Constants.GAME_LOST_MSG);
        }
        else if (response == Constants.GAME_ENDED)
        {
            uiManager.ShowErrorPanel(Constants.GAME_ENDED_MSG);
        }

        //clean up
        gameManager.player._findingRoom = false;
        gameManager.player._inRoom = false;
        uiManager.DisplayVsPlayerPanelUI();
        gameManager.currentRoom.game.ClearGame();
    }

    public string PrintBytes(ref byte[] byteArray)
    {
        var sb = new StringBuilder("{ ");
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

    public void SetPacketDisplay(string s, bool t)
    {
        uiManager.AddPacketToHistory(t, s);

        if (t)
        {
            uiManager.LastPacketSentUI(s);
        }
        uiManager.LastPacketRecievedUI(s);
    }

    public void BytesToMessageLength(ref byte[] msg, ref byte[] len, ref UInt16 msgNo, ref UInt16 length)
    {
        msgNo = BitConverter.ToUInt16(msg, 0);
        length = BitConverter.ToUInt16(len, 0);
    }
}