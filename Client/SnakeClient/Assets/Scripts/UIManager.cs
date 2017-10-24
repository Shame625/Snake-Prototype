using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private NetworkManager networkManager;
    private GameManager gameManager;

    //Debug crap
    public Text debugPacketRecieved;
    public Text debugPacketSent;
    public GameObject packetHistory;
    public GameObject packetHistoryItemPrefab;
    public Scrollbar packetHistoryScrollbar;
    public Text debugNameText;

    public GameObject connectionPanel;
    public GameObject mainPanel;
    public GameObject submitNamePanel;
    public GameObject vsPlayerPanel;
    public GameObject createRoomPanel;
    public GameObject inRoomPanel;
    public GameObject findGamePanel;
    public GameObject findingPublicGamePanel;
    public GameObject findPrivateGamePanel;

    public InputField nameInputField;
    public Button connectButton;
    public Text statusTextConnection;
    public Text userNameTextStatus;
    public Text userNameText;

    //Find private room
    public InputField privateRoomNameInput;
    public InputField privateRoomPasswordInput;
    public Text privateRoomTextStatus;

    //Room create
    public Dropdown roomTypeDropdown;
    public InputField roomName;
    public InputField roomPassword;
    public Text roomCreationStatus;

    //In room commands
    public Text player1;
    public Text player2;
    public GameObject player2LoadingBar;
    public Text roomTitle;
    public Button AbandonButton;
    public GameObject mapButton;

    public GameObject roomButtonLeft;
    public GameObject roomButtonRight;

    //Error panel
    public GameObject errorPanel;
    public Text errorPanelMessage;


    //Map Colors
    Color Air = Color.white;
    Color Wall = Color.black;
    Color Spawn = Color.green;


    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        gameManager = GetComponent<GameManager>();

        SetUpDropDownTypes();
    }

    public void ConnectUI()
    {
        connectButton.interactable = false;
    }

    public void CheckConnectionUI()
    {
        statusTextConnection.color = Color.white;
        if (networkManager.attempts == Constants.LOOP_MAX)
        {
            statusTextConnection.text = Constants.CONNECTION_FAILED;
            connectButton.interactable = true;

            return;
        }

        if (networkManager.GetStatusConnection())
        {
            statusTextConnection.text = Constants.CONNECTION_SUCCESS;
            connectButton.interactable = true;

            connectionPanel.SetActive(false);
        }
        else
        {
            statusTextConnection.text = Constants.CONNECTION_ATTEMPT + networkManager.attempts;
        }
    }

    public void DisplayConnectToServerPanelUI()
    {
        connectionPanel.SetActive(true);
        mainPanel.SetActive(false);
        submitNamePanel.SetActive(false);
        vsPlayerPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        inRoomPanel.SetActive(false);
        findGamePanel.SetActive(false);
        findingPublicGamePanel.SetActive(false);
        HideDisplayFindPrivateGamePanelUI();
    }

    public void DisplaySubmitUserNamePanelUI()
    {
        userNameTextStatus.text = "";

        connectionPanel.SetActive(false);
        mainPanel.SetActive(false);
        submitNamePanel.SetActive(true);
        vsPlayerPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        inRoomPanel.SetActive(false);
        findGamePanel.SetActive(false);
        HideDisplayFindPrivateGamePanelUI();
    }

    public void DisplayMainMenuUI()
    {
        connectionPanel.SetActive(false);
        mainPanel.SetActive(true);
        submitNamePanel.SetActive(false);
        vsPlayerPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        inRoomPanel.SetActive(false);
        findGamePanel.SetActive(false);
        HideDisplayFindPrivateGamePanelUI();

        userNameText.text = "Welcome " + gameManager.player._userName;
        debugNameText.text = gameManager.player._userName;
    }

    public void DisplayVsPlayerPanelUI()
    {
        mainPanel.SetActive(false);
        vsPlayerPanel.SetActive(true);
        createRoomPanel.SetActive(false);
        inRoomPanel.SetActive(false);
        findGamePanel.SetActive(false);
        HideDisplayFindPrivateGamePanelUI();
    }

    public void DisplayCreateGamePanelUI()
    {
        roomCreationStatus.text = "";

        vsPlayerPanel.SetActive(false);
        createRoomPanel.SetActive(true);
        inRoomPanel.SetActive(false);
        HideDisplayFindPrivateGamePanelUI();
    }

    public void DisplayFindGamePanelUI()
    {
        findGamePanel.SetActive(true);
        vsPlayerPanel.SetActive(false);
        findingPublicGamePanel.SetActive(false);
        inRoomPanel.SetActive(false);
        HideDisplayFindPrivateGamePanelUI();
    }

    public void DisplayFindPrivateGamePanelUI()
    {
        findPrivateGamePanel.SetActive(true);
    }

    public void HideDisplayFindPrivateGamePanelUI()
    {
        findPrivateGamePanel.SetActive(false);
    }

    public void RoomCreatedUI()
    {
        roomButtonLeft.SetActive(false);

        player1.text = gameManager.player._userName;
        player2.text = "";

        player2LoadingBar.SetActive(true);

        bool isPrivate = false;

        if (gameManager.currentRoom.roomType == Constants.ROOM_TYPE_PRIVATE)
        {
            roomTitle.text = gameManager.enteredRoomName;
            isPrivate = true;
        }
        else
            roomTitle.text = "Public room";

        SetSelectedMap(0);
        DisplayRoomPanelUI(isPrivate);
    }

    public void DisplayRoomPanelUI(bool isPrivate)
    {
        player1.text = gameManager.player._userName;
        player2.text = gameManager.opponent._userName;
        player2LoadingBar.SetActive(false);

        if(gameManager.currentRoom.isAdmin)
        {
            AbandonButton.GetComponentInChildren<Text>().text = "Abandon Room";
        }
        else
        {
            AbandonButton.GetComponentInChildren<Text>().text = "Leave Room";
        }

        if(isPrivate)
        {
            roomTitle.text = gameManager.currentRoom.roomName;
        }
        else
        {
            roomTitle.text = "Public Room";
        }

        createRoomPanel.SetActive(false);
        inRoomPanel.SetActive(true);
        findGamePanel.SetActive(false);
        findingPublicGamePanel.SetActive(false);
        HideDisplayFindPrivateGamePanelUI();
    }

    public void PlayerLeftMyRoomUI()
    {
        player2.text = "";
        player2LoadingBar.SetActive(true);
    }

    public void RoomTypeChangedUI()
    {
        Debug.Log(roomTypeDropdown.value);
        if(roomTypeDropdown.value == Constants.ROOM_TYPE_PRIVATE)
        {
            roomName.interactable = true;
            roomPassword.interactable = true;
            gameManager.enteredroomType = Constants.ROOM_TYPE_PRIVATE;
        }
        else
        {
            roomName.interactable = false;
            roomPassword.interactable = false;
            gameManager.enteredroomType = Constants.ROOM_TYPE_PUBLIC;
        }
    }

    public void DisconnectedFromServerUI(UInt16 code)
    {
        connectButton.interactable = false;
        Invoke("WaiterBeforeConnection", 5);

        DisplayConnectToServerPanelUI();

        if (code == Constants.LOGOUT_FORCED)
            statusTextConnection.text = Constants.CONNECTION_LOST;
        else if (code == Constants.LOGOUT_WARNING)
        {
            statusTextConnection.color = Color.red;
            statusTextConnection.text = Constants.CONNECTION_FORCED_CLOSURE;
        }
    }

    void WaiterBeforeConnection()
    {
        connectButton.interactable = true;
    }

    public void PlayerJoinedMyRoomUI()
    {
        player2.text = gameManager.opponent._userName;
        player2LoadingBar.SetActive(false);
    }

    public void JoinedRoomUI()
    {
        player2.text = gameManager.opponent._userName;
        player2LoadingBar.SetActive(false);

        HideDisplayFindPrivateGamePanelUI();

        bool isPrivate = (gameManager.currentRoom.roomType == Constants.ROOM_TYPE_PRIVATE ? true : false);

        DisplayRoomPanelUI(isPrivate);
    }

    public void RoomPrivateErrorUI(UInt16 errorCode)
    {
        if(errorCode == Constants.ROOM_NAME_BAD)
        {
            privateRoomTextStatus.text = Constants.ROOM_PRIVATE_NAME_BAD_MSG;
        }
        else if (errorCode == Constants.ROOM_PASSWORD_BAD)
        {
            privateRoomTextStatus.text = Constants.ROOM_PRIVATE_PASSWORD_BAD_MSG;
        }
        else
        {
            privateRoomTextStatus.text = Constants.ROOM_FULL_MSG;
        }
    }

    public void UserNameErrorUI(ref UInt16 err)
    {
        if (err == Constants.USERNAME_BAD)
            userNameTextStatus.text = "User Name must be between " + Constants.USERNAME_LENGTH_MIN + " and " +
                                      Constants.USERNAME_LENGTH_MAX + " characters long and cannot contain special characters!";

        else if (err == Constants.USERNAME_IN_USE)
            userNameTextStatus.text = "User Name already in use!";
    }

    void SetUpDropDownTypes()
    {
        roomTypeDropdown.options.Clear();

        roomTypeDropdown.options.Add(new Dropdown.OptionData("Public"));
        roomTypeDropdown.options.Add(new Dropdown.OptionData("Private"));
        

        roomTypeDropdown.value = 0;
    }

    public string GetNameFromInputField()
    {
        gameManager.enteredName = nameInputField.text;
        return nameInputField.text;
    }

    public void RoomCreationFailedUI(UInt16 errorCode)
    {
        if(errorCode == Constants.ROOM_CREATE_FAILURE)
        {
            roomCreationStatus.text = Constants.ROOM_UNKNOWN_ERROR;
        }
        else if(errorCode == Constants.ROOM_NAME_BAD)
        {
            roomCreationStatus.text = Constants.ROOM_NAME_BAD_MSG;
        }
        else if(errorCode == Constants.ROOM_NAME_IN_USE)
        {
            roomCreationStatus.text = Constants.ROOM_NAME_IN_USE_MSG;
        }
        else if (errorCode == Constants.ROOM_PASSWORD_BAD)
        {
            roomCreationStatus.text = Constants.ROOM_NAME_BAD_PASSWORD_MSG;
        }
        else if(errorCode == Constants.USER_ALREADY_IN_ROOM)
        {
            roomCreationStatus.text = Constants.ROOM_ALREADY_IN;
        }
    }

    public void AbandonRoomUI(UInt16 errorCode)
    {
        if(errorCode == Constants.ROOM_ABANDONED_SUCCESS)
        {
            ShowErrorPanel(Constants.ROOM_ABANDONED_SUCCESS_MSG);
            ClearUI();
            DisplayVsPlayerPanelUI();
        }
        else
        {
            ShowErrorPanel(Constants.ROOM_ABANDONED_FAILED_MSG);
        }
    }

    public void SelectMapLeftButtonUI()
    {
        roomButtonRight.SetActive(true);

        if (MapManager.currentMapIndex > 0 )
        {
            MapManager.currentMapIndex--;

            //Will be called from functions, jsut testing now
            SetSelectedMap(MapManager.currentMapIndex);
        }
        if(MapManager.currentMapIndex == 0)
        {
            roomButtonLeft.SetActive(false);
        }
    }

    public void SelectMapRightButtonUI()
    {
        roomButtonLeft.SetActive(true);

        if(MapManager.currentMapIndex < MapManager._NumberOfMaps - 1)
        {
            MapManager.currentMapIndex++;

            //Will be called from functions, jsut testing now
            SetSelectedMap(MapManager.currentMapIndex);
        }
        if(MapManager.currentMapIndex == MapManager._NumberOfMaps - 1)
        {
            roomButtonRight.SetActive(false);
        }
    }

    public void SetSelectedMap(int id)
    {
        Map map = MapManager._Maps[0];
        try
        {
            map = MapManager._Maps[id];
        }
        catch
        {
            Debug.Log("Map does not exist");
        }
        DrawMapOnMapElement(ref map);
        return;
    }

    void DrawMapOnMapElement(ref Map map)
    {
        RawImage image = mapButton.GetComponent<RawImage>();
        mapButton.GetComponent<RectTransform>().sizeDelta = new Vector2(map._x, map._y);
        Texture2D myTexture = new Texture2D(map._x, map._y);

        myTexture.filterMode = FilterMode.Point;
        for (int y = 0; y < myTexture.height; y++)
        {
            for (int x = 0; x < myTexture.width; x++)
            {
                Color color = ((x & y) != 0 ? Color.white : Color.gray);

                if (map._grid[x, y] == (int)MapManager.MapObjects.AIR)
                    myTexture.SetPixel(x, y, Air);

                else if (map._grid[x, y] == (int)MapManager.MapObjects.WALL)
                    myTexture.SetPixel(x, y, Wall);

                else if (map._grid[x, y] == (int)MapManager.MapObjects.SPAWN_POINT)
                    myTexture.SetPixel(x, y, Spawn);
            }
        }

        myTexture.Apply();
        image.texture = myTexture;
    }

    public UInt16 GetGameType()
    {
        return (UInt16)roomTypeDropdown.value;
    }

    public string GetRoomName()
    {
        gameManager.enteredRoomName = roomName.text;
        return roomName.text;
    }

    public string GetRoomPassword()
    {
        return roomPassword.text;
    }

    public string GetPrivateRoomName()
    {
        return privateRoomNameInput.text;
    }

    public string GetPrivateRoomPassword()
    {
        return privateRoomPasswordInput.text;
    }

    public void ShowFindingPublicGamePanelUI()
    {
        findingPublicGamePanel.SetActive(true);
    }

    public void HideFindingPublicGamePanelUI()
    {
        findingPublicGamePanel.SetActive(false);
    }

    public void ShowErrorPanel(string txt)
    {
        errorPanelMessage.text = txt;
        errorPanel.SetActive(true);
    }

    public void HideErrorPanel()
    {
        errorPanel.SetActive(false);
    }

    public void LastPacketRecievedUI(string s)
    {
        debugPacketRecieved.text = s;
    }

    public void LastPacketSentUI(string s)
    {
        debugPacketSent.text = s;
    }

    public void AddPacketToHistory(bool t, string s)
    {
        GameObject temp = Instantiate(packetHistoryItemPrefab, packetHistory.transform, false);
        temp.GetComponent<PacketUI>().SetPrefab(t, s);

        packetHistoryScrollbar.value = 0.0f ;
    }

    public void ClearUI()
    {
        statusTextConnection.text = "";
        userNameTextStatus.text = "";
        roomCreationStatus.text = "";
    }
}
