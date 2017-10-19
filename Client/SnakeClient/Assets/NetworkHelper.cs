using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
    bool isLoopInvoked = false;
    UIManager uiManager;

    private void Awake()
    {
        uiManager = GetComponent<UIManager>();    
    }

    public void Connect()
    {
        uiManager.ConnectUI();

        NetworkManager.Instance.attempts = 0;

        if (!isLoopInvoked)
            InvokeRepeating("CheckConnection", 0, 1);

        isLoopInvoked = true;
    }

    public void ConnectionHeartbeat()
    {
        if (!NetworkManager.Instance.GetStatusConnection())
        {
            uiManager.DisconnectedFromServerUI();
            CancelInvoke("ConnectionHeartbeat");
        }
    }

    void CheckConnection()
    {
        if (NetworkManager.Instance.attempts == Constants.LOOP_MAX)
        {
            uiManager.CheckConnectionUI();
            isLoopInvoked = false;
            CancelInvoke("CheckConnection");

            return;
        }

        if (NetworkManager.Instance.GetStatusConnection())
        {
            CancelInvoke("CheckConnection");
            InvokeRepeating("ConnectionHeartbeat", 0, 5);

            uiManager.DisplaySubmitUserNamePanelUI();

            isLoopInvoked = false;
        }
        else
        {
            NetworkManager.Instance.LoopConnect();
            NetworkManager.Instance.attempts++;

            if (NetworkManager.Instance.attempts == 1)
            {
                uiManager.DisplayConnectToServerPanelUI();
            }
        }

        uiManager.CheckConnectionUI();
    }
}