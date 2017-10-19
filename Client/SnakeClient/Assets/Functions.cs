using System;
using System.Text;
using UnityEngine;

public class Functions : MonoBehaviour
{
    private UIManager uiManager;
    private PacketHelper packetHelper;

    private void Awake()
    {
        uiManager = GetComponent<UIManager>();
        packetHelper = new PacketHelper();
    }

    public void SubmitUserName()
    {
            string name = "";
            name = uiManager.GetNameFromInputField();

            byte[] dataToSend = packetHelper.StringToBytes(Messages.SET_NAME_REQUEST, ref name);

            byte[] temp = new byte[2];
            temp[0] = dataToSend[2];
            temp[1] = dataToSend[3];

            Debug.Log(BitConverter.ToUInt16(temp, 0));

            Debug.Log(PrintBytes(ref dataToSend));

            NetworkManager.Instance.SendPacket(ref dataToSend);
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