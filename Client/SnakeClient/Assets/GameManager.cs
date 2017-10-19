using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private NetworkHelper networkHelper;

    private void Awake()
    {
        networkHelper = GetComponent<NetworkHelper>();
    }

    private void Start()
    {
        //Try to connect to server
        networkHelper.Connect();
    }
}
