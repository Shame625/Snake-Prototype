using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject connectionPanel;
    public GameObject mainPanel;
    public Button connectButton;
    public Text statusText;

    //Repeator
    bool isLoopInvoked = false;


    public void Connect()
    {
        connectButton.interactable = false;
        NetworkManager.Instance.attempts = 0;

        if (!isLoopInvoked)
            InvokeRepeating("CheckConnection", 0, 1);

        isLoopInvoked = true;
    }

    public void ConnectionHeartbeat()
    {
        if (!NetworkManager.Instance.GetStatusConnection())
        {
            mainPanel.SetActive(false);
            connectionPanel.SetActive(true);

            statusText.text = Constants.CONNECTION_LOST;
            CancelInvoke("ConnectionHeartbeat");
        }
    }

    void CheckConnection()
    {
        if (NetworkManager.Instance.attempts == Constants.LOOP_MAX)
        {
            statusText.text = Constants.CONNECTION_FAILED;
            connectButton.interactable = true;
            isLoopInvoked = false;
            CancelInvoke("CheckConnection");

            return;
        }

        if (NetworkManager.Instance.GetStatusConnection())
        {
            statusText.text = Constants.CONNECTION_SUCCESS;
            connectButton.interactable = true;

            CancelInvoke("CheckConnection");
            InvokeRepeating("ConnectionHeartbeat", 0, 5);

            isLoopInvoked = false;
            connectionPanel.SetActive(false);
            mainPanel.SetActive(true);
        }
        else
        {
            NetworkManager.Instance.LoopConnect();
            NetworkManager.Instance.attempts++;
            statusText.text = Constants.CONNECTION_ATTEMPT + NetworkManager.Instance.attempts;
        }
    }
}
