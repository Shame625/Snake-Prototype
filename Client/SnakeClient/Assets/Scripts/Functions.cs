using System;
using System.Text;
using UnityEngine;

public class Functions : MonoBehaviour
{
    private NetworkManager networkManager;
    private UIManager uiManager;
    private PacketHelper packetHelper;
    private GameManager gameManager;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        uiManager = GetComponent<UIManager>();
        packetHelper = new PacketHelper();
        gameManager = GetComponent<GameManager>();
    }

    public void SubmitUserName()
    {
        string name = "";
        name = uiManager.GetNameFromInputField();

        byte[] dataToSend = packetHelper.StringToBytes(Messages.SET_NAME_REQUEST, ref name);
        networkManager.SendPacket(ref dataToSend);
        Invoke("IdRequest", 0.2f);
    }

    public void IdRequest()
    {
        byte[] dataToSend = packetHelper.BlankMessage(Messages.USER_ID_REQUEST);
        networkManager.SendPacket(ref dataToSend);
    }

    public void DebugCheatFunction()
    {
        byte[] dataToSend = new byte[3];
        networkManager.SendPacket(ref dataToSend);
    }

    public void CreateGame()
    {
        UInt16 gameType = uiManager.GetGameType();
        byte[] dataToSend;

        if (gameType == Constants.ROOM_TYPE_PUBLIC)
        {
            dataToSend = packetHelper.UInt16ToBytes(Messages.ROOM_CREATE_REQUEST, Constants.ROOM_TYPE_PUBLIC);
        }
        else
        {
            dataToSend = packetHelper.PrivateRoomToBytes(Messages.ROOM_CREATE_REQUEST, Constants.ROOM_TYPE_PRIVATE, uiManager.GetRoomName(), uiManager.GetRoomPassword());
        }

        if(dataToSend.Length >= 4)
            networkManager.SendPacket(ref dataToSend);
    }

    public void AbandonGame()
    {
        if (gameManager.player._inRoom)
        {
            if (gameManager.currentRoom.isAdmin)
            {
                byte[] dataToSend = packetHelper.IntToBytes(Messages.ROOM_ABANDON_REQUEST, gameManager.currentRoom.roomId);
                networkManager.SendPacket(ref dataToSend);
            }
            else
                LeaveRoom();
        }
    }

    public void LeaveRoom()
    {
            byte[] dataToSend = new byte[4];
            Debug.Log("Called leave room!");
            packetHelper.FillHeaderBlankData(Messages.ROOM_LEAVE_REQUEST, ref dataToSend);
            networkManager.SendPacket(ref dataToSend);
    }

    public void FindPublicGame()
    {
        if (!gameManager.player._inRoom)
        {
            byte[] dataToSend = new byte[4];
            packetHelper.FillHeaderBlankData(Messages.ROOM_JOIN_PUBLIC_ROOM_REQUEST, ref dataToSend);
            networkManager.SendPacket(ref dataToSend);
        }
    }

    public void FindPrivateGame()
    {
        string name = uiManager.GetPrivateRoomName();
        string password = uiManager.GetPrivateRoomPassword();

        if (!gameManager.player._inRoom)
        {
            byte[] dataToSend = packetHelper.JoinPrivateRoomToBytes(Messages.ROOM_JOIN_PRIVATE_ROOM_REQUEST, name, password);
            networkManager.SendPacket(ref dataToSend);
        }
    }

    public void CancelFindingPublicGame()
    {
        byte[] dataToSend = new byte[4];
        packetHelper.FillHeaderBlankData(Messages.ROOM_CANCEL_FINDING_REQUEST, ref dataToSend);
        networkManager.SendPacket(ref dataToSend);
    }

    public void Logout()
    {
        byte[] dataToSend = packetHelper.UInt16ToBytes(Messages.LOGOUT, Constants.LOGOUT_CODE);
        networkManager.SendPacket(ref dataToSend);
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
}