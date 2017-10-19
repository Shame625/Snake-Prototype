using System;
using System.Text;
using UnityEngine;

public class Functions : MonoBehaviour
{
    private NetworkManager networkManager;
    private UIManager uiManager;
    private PacketHelper packetHelper;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        uiManager = GetComponent<UIManager>();
        packetHelper = new PacketHelper();
    }

    public void SubmitUserName()
    {
            string name = "";
            name = uiManager.GetNameFromInputField();

            byte[] dataToSend = packetHelper.StringToBytes(Messages.SET_NAME_REQUEST, ref name);
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