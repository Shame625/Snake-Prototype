using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private NetworkManager networkManager;
    private GameManager gameManager;

    public GameObject connectionPanel;
    public GameObject mainPanel;
    public GameObject submitNamePanel;
    public GameObject vsPlayerPanel;
    public GameObject createRoomPanel;
    public GameObject findRoomPanel;
    public GameObject inRoomPanel;

    public InputField nameInputField;
    public Button connectButton;
    public Text statusTextConnection;
    public Text userNameTextStatus;
    public Text userNameText;

    //Room create
    public Dropdown roomTypeDropdown;
    public InputField roomName;
    public InputField roomPassword;
    public Text roomCreationStatus;

    //In room commands
    public Text player1;
    public Text player2;
    public Text roomTitle;
    public Button AbandonButton;


    //Error panel
    public GameObject errorPanel;
    public Text errorPanelMessage; 

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
    }

    public void DisplayMainMenuUI()
    {
        connectionPanel.SetActive(false);
        mainPanel.SetActive(true);
        submitNamePanel.SetActive(false);
        vsPlayerPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        inRoomPanel.SetActive(false);

        userNameText.text = "Welcome " + gameManager.player._userName;
    }

    public void DisplayVsPlayerPanelUI()
    {
        mainPanel.SetActive(false);
        vsPlayerPanel.SetActive(true);
        createRoomPanel.SetActive(false);
        inRoomPanel.SetActive(false);
    }

    public void DisplayCreateGamePanelUI()
    {
        roomCreationStatus.text = "";

        vsPlayerPanel.SetActive(false);
        createRoomPanel.SetActive(true);
        inRoomPanel.SetActive(false);
    }

    public void RoomCreatedUI()
    {
        player1.text = gameManager.player._userName;

        if(gameManager.currentRoom.roomType == Constants.ROOM_TYPE_PRIVATE)
            roomTitle.text = gameManager.enteredRoomName;
        else
            roomTitle.text = "Public room";
        AbandonButton.GetComponentInChildren<Text>().text = "Abandon Room";

        DisplayRoomPanelUI();
    }

    public void DisplayRoomPanelUI()
    {
        createRoomPanel.SetActive(false);
        inRoomPanel.SetActive(true);
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
            DisplayVsPlayerPanelUI();
        }
        else
        {
            ShowErrorPanel(Constants.ROOM_ABANDONED_FAILED_MSG);
        }
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

    public void ShowErrorPanel(string txt)
    {
        errorPanelMessage.text = txt;
        errorPanel.SetActive(true);
    }

    public void HideErrorPanel()
    {
        errorPanel.SetActive(false);
    }
}
