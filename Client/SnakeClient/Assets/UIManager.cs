using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject connectionPanel;
    public GameObject mainPanel;
    public GameObject submitNamePanel;

    public InputField nameInputField;
    public Button connectButton;
    public Text statusTextConnection;


    public void ConnectUI()
    {
        connectButton.interactable = false;
    }

    public void CheckConnectionUI()
    {
        if (NetworkManager.Instance.attempts == Constants.LOOP_MAX)
        {
            statusTextConnection.text = Constants.CONNECTION_FAILED;
            connectButton.interactable = true;

            return;
        }

        if (NetworkManager.Instance.GetStatusConnection())
        {
            statusTextConnection.text = Constants.CONNECTION_SUCCESS;
            connectButton.interactable = true;

            connectionPanel.SetActive(false);
        }
        else
        {
            statusTextConnection.text = Constants.CONNECTION_ATTEMPT + NetworkManager.Instance.attempts;
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

    public void DisconnectedFromServerUI()
    {
        DisplayConnectToServerPanelUI();
        statusTextConnection.text = Constants.CONNECTION_LOST;
    }


    public string GetNameFromInputField()
    {
        return nameInputField.text;
    }
}
