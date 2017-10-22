using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PacketUI : MonoBehaviour
{
    public Color colorSent = new Color(0.357f, 0.549f, 0.353f, 1);
    public Color colorRec = new Color(0.91f, 0.365f, 0.459f, 1);

    public void SetPrefab(bool type, string packet)
    {
        if(type)
        {
            GetComponent<Image>().color = colorSent;
        }
        else
        {
            GetComponent<Image>().color = colorRec;
        }

        transform.GetChild(0).GetComponent<Text>().text = (type == true ? "SENT" : "REC");
        transform.GetChild(1).GetComponent<Text>().text = System.DateTime.Now.ToLongTimeString();
        transform.GetChild(2).GetComponent<Text>().text = packet;
    }
}
