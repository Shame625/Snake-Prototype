using System;
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

    public void userNameOK()
    {
        gameManager.player.SetName(ref gameManager.enteredName);
        uiManager.DisplayMainMenuUI();
    }

    public void userNameBAD(UInt16 err)
    {
        uiManager.UserNameErrorUI(ref err);
    }

    public void loggedOut(UInt16 code)
    {
        CancelInvoke("ConnectionHeartbeat");
        if(code == Constants.LOGOUT_NORMAL)
            Application.Quit();

        uiManager.DisconnectedFromServerUI(code);
    }


    public string PrintBytes(ref byte[] byteArray)
    {
        var sb = new StringBuilder("new byte[] { ");
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

    public void BytesToMessageLength(ref byte[] msg, ref byte[] len, ref UInt16 msgNo, ref UInt16 length)
    {
        msgNo = BitConverter.ToUInt16(msg, 0);
        length = BitConverter.ToUInt16(len, 0);
    }
}