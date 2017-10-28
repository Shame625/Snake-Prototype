using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour
{
    Text myText;

    void Awake()
    {
        myText = GetComponent<Text>();
    }

    int timer = 4;

    public void StartCountDown()
    {
        timer = 4;
        gameObject.SetActive(true);
        InvokeRepeating("InvokedMethod", 0, 1);
    }

    void InvokedMethod()
    {
        timer--;
        if (timer > 0)
        {
            myText.text = timer.ToString();
        }
        else
        {
            myText.text = "GO";
            CancelInvoke("InvokedMethod");
            Invoke("HideCountDown", 1);
        }

        GetComponent<Animation>().Play();
    }

    void HideCountDown()
    {
        gameObject.SetActive(false);
    }
}
