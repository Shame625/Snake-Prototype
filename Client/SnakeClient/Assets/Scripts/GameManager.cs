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

    public GameObject MapParent;

    public GameObject PlayerPrefab;
    public GameObject FloorTile;
    public GameObject MapWallPrefab;


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

    public void StartGame()
    {
        //Initialize map
        InitializeMap();

    }

    public void InitializeMap()
    {
        foreach(GameObject children in MapParent.transform)
        {
            Destroy(children);
        }

        //Spawning map

        //Floor tile
        GameObject temp = (GameObject)Instantiate(FloorTile, new Vector3(currentRoom.game._selectedMap._xSize / 2, -0.5f, currentRoom.game._selectedMap._ySize / 2), FloorTile.transform.rotation);

        temp.transform.localScale = new Vector3(currentRoom.game._selectedMap._xSize, currentRoom.game._selectedMap._ySize, 1);
        temp.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(currentRoom.game._selectedMap._xSize, currentRoom.game._selectedMap._ySize);
        temp.transform.SetParent(MapParent.transform);

        //Objects
        for (int y = 0; y < currentRoom.game._selectedMap._ySize; y++)
        {
            for (int x = 0; x < currentRoom.game._selectedMap._xSize; x++)
            {
                SpawnBlock(new Vector2(x, y), currentRoom.game._selectedMap._grid[x, y]);
            }
        }

        //SpawnPlayer
        temp = (GameObject)Instantiate(PlayerPrefab, new Vector3(currentRoom.game._selectedMap.spawnPoint.x , 0, currentRoom.game._selectedMap.spawnPoint.y), Quaternion.identity);

        Camera.main.transform.position = new Vector3(currentRoom.game._selectedMap._xSize / 2, Camera.main.transform.position.y, currentRoom.game._selectedMap._ySize / 2);
    }

    void SpawnBlock(Vector2 pos, UInt16 type)
    {
        if(type != (UInt16)MapManager.MapObjects.AIR && type != (UInt16)MapManager.MapObjects.SPAWN_POINT)
        {
            if(type == (UInt16)MapManager.MapObjects.WALL)
            {
                GameObject temp = (GameObject)Instantiate(MapWallPrefab, new Vector3(pos.x, 0, pos.y), Quaternion.identity);
                temp.transform.SetParent(MapParent.transform);
            }
        }
        return;
    }
}
