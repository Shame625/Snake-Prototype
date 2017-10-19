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

    public InputField nameInputField;
    public Button connectButton;
    public Text statusTextConnection;
    public Text userNameTextStatus;
    public Text userNameText;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        gameManager = GetComponent<GameManager>();
    }

    public void ConnectUI()
    {
        connectButton.interactable = false;
    }

    public void CheckConnectionUI()
    {
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
    }

    public void DisplaySubmitUserNamePanelUI()
    {
        connectionPanel.SetActive(false);
        mainPanel.SetActive(false);
        submitNamePanel.SetActive(true);
    }

    public void DisplayMainMenuUI()
    {
        connectionPanel.SetActive(false);
        mainPanel.SetActive(true);
        submitNamePanel.SetActive(false);

        userNameText.text = "Welcome " + gameManager.player._userName;
    }

    public void DisconnectedFromServerUI()
    {
        DisplayConnectToServerPanelUI();
        statusTextConnection.text = Constants.CONNECTION_LOST;
    }

    public void UserNameErrorUI()
    {
        userNameTextStatus.text = "User Name must be between " + Constants.USERNAME_LENGTH_MIN + " and " +
                                  Constants.USERNAME_LENGTH_MAX + " characters long and cannot contain special characters!";
    }

    public string GetNameFromInputField()
    {
        gameManager.enteredName = nameInputField.text;
        return nameInputField.text;
    }
}
