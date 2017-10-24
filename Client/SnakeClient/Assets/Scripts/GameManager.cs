using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public NetworkManager networkManager;
    private NetworkHelper networkHelper;

    public Player player;
    public Player opponent;

    public Room currentRoom;

    public string enteredName;
    public UInt16 enteredroomType;
    public string enteredRoomName;

    private void Awake()
    {
        MapManager.LoadMaps();

        Application.runInBackground = true;
        networkManager = GetComponent<NetworkManager>();
        networkHelper = GetComponent<NetworkHelper>();
        player = new Player();
        opponent = new Player();
        currentRoom = new Room();
        opponent._userName = "";
    }

    private void Start()
    {
        //Try to connect to server
        networkHelper.Connect();
    }

    public void SetRoom(ref Room newRoom)
    {
        currentRoom = newRoom;
    }
}
