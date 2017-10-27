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
    public GameObject FloorTileTransparent;
    public GameObject MapWallPrefab;
    public Texture playerWallTexture;
    public Texture enemyWallTexture;

    GameObject enemyRef;
    GameObject playerRef;

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

        //Floor tile transparent
        /*
        temp = (GameObject)Instantiate(FloorTileTransparent, new Vector3(currentRoom.game._selectedMap._xSize / 2, 0.5f, currentRoom.game._selectedMap._ySize / 2), FloorTile.transform.rotation);
        temp.transform.localScale = new Vector3(currentRoom.game._selectedMap._xSize, currentRoom.game._selectedMap._ySize, 1);
        temp.transform.SetParent(MapParent.transform);
        */

        //Objects
        for (int y = 0; y < currentRoom.game._selectedMap._ySize; y++)
        {
            for (int x = 0; x < currentRoom.game._selectedMap._xSize; x++)
            {
                SpawnBlock(new Vector3(x, 0, y), currentRoom.game._selectedMap._grid[x, y], false);
                SpawnBlock(new Vector3(x, 1, y), currentRoom.game._selectedMap._grid[x, y], true);
            }
        }

        //SpawnPlayer
        playerRef = (GameObject)Instantiate(PlayerPrefab, new Vector3(currentRoom.game._selectedMap.spawnPoint.x , 1, currentRoom.game._selectedMap.spawnPoint.y), Quaternion.identity);

        //Spawn Enemy
        enemyRef = (GameObject)Instantiate(PlayerPrefab, new Vector3(currentRoom.game._selectedMap.spawnPoint.x, 0, currentRoom.game._selectedMap.spawnPoint.y), Quaternion.identity);

        Camera.main.transform.position = new Vector3(currentRoom.game._selectedMap._xSize / 2, Camera.main.transform.position.y, currentRoom.game._selectedMap._ySize / 2);
    }

    void SpawnBlock(Vector3 pos, UInt16 type, bool player)
    {
        if(type != (UInt16)MapManager.MapObjects.AIR && type != (UInt16)MapManager.MapObjects.SPAWN_POINT)
        {
            if(type == (UInt16)MapManager.MapObjects.WALL)
            {
                GameObject temp = (GameObject)Instantiate(MapWallPrefab, pos, Quaternion.identity);

                if (player)
                    temp.GetComponent<MeshRenderer>().material.mainTexture = playerWallTexture;

                temp.transform.SetParent(MapParent.transform);
            }
        }
        return;
    }
}
