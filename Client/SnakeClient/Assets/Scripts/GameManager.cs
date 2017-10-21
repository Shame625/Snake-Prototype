using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public NetworkManager networkManager;
    private NetworkHelper networkHelper;

    public Player player;

    public string enteredName;

    private void Awake()
    {
        Application.runInBackground = true;
        networkManager = GetComponent<NetworkManager>();
        networkHelper = GetComponent<NetworkHelper>();
        player = new Player();
    }

    private void Start()
    {
        //Try to connect to server
        networkHelper.Connect();
    }
}
