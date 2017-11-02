using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public NetworkManager networkManager;
    private NetworkHelper networkHelper;
    private UIManager uiManager;
    Functions functions;

    public Player player;
    public Player opponent;

    public Room currentRoom;

    public string enteredName;
    public UInt16 enteredroomType;
    public string enteredRoomName;

    public GameObject MapParent;

    public GameObject PlayerPrefab;
    public GameObject OpponentPrefab;

    public GameObject FloorTile;
    public GameObject FloorTileTransparent;
    public GameObject MapWallPrefab;
    public Texture playerWallTexture;
    public Texture enemyWallTexture;

    public GameObject enemyRef;
    public GameObject playerRef;

    List<GameObject> player1Blocks = new List<GameObject>();
    List<GameObject> player2Blocks = new List<GameObject>();

    public GameObject bugPrefabPlayer;
    public GameObject bugPrefabOpponent;

    GameObject player1Bug;
    GameObject player2Bug;

    private void Awake()
    {
        MapManager.LoadMaps();

        Application.runInBackground = true;
        networkManager = GetComponent<NetworkManager>();
        networkHelper = GetComponent<NetworkHelper>();
        uiManager = GetComponent<UIManager>();
        functions = GetComponent<Functions>();

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
        Invoke("setGameInProgress", 3);
        currentRoom.game.SetGame();
    }

    void setGameInProgress()
    {
        currentRoom.game._gameInProgress = true;
    }


    public byte prevDirection = Constants.GAME_DIRECTION_LEFT;

    private void Update()
    {
            //This must be sent to server!
            if (currentRoom.game._gameInProgress)
            {
                if (Input.GetKeyDown(KeyCode.W) && (prevDirection != Constants.GAME_DIRECTION_DOWN))
                {
                    functions.DirectionChanged(Constants.GAME_DIRECTION_UP);
                }
                else if (Input.GetKeyDown(KeyCode.S) && (prevDirection != Constants.GAME_DIRECTION_UP))
                {
                    functions.DirectionChanged(Constants.GAME_DIRECTION_DOWN );
                }
                else if (Input.GetKeyDown(KeyCode.A) && (prevDirection != Constants.GAME_DIRECTION_RIGHT ))
                {
                    functions.DirectionChanged(Constants.GAME_DIRECTION_LEFT);
                }
                else if (Input.GetKeyDown(KeyCode.D) && (prevDirection != Constants.GAME_DIRECTION_LEFT))
                {
                    functions.DirectionChanged(Constants.GAME_DIRECTION_RIGHT);
                }
            }
    }

    public void GameLoop()
    {
        /*
        MapManager.Position P2Position =  MapManager.getCoordinates(Convert.ToUInt16(currentRoom.game._currentLocationP2));

        currentRoom.game._currentLocationP1 = currentRoom.game.ReturnNewIndex(currentRoom.game.P1Direction, currentRoom.game._currentLocationP1);
        MapManager.Position P1Position = MapManager.getCoordinates(Convert.ToUInt16(currentRoom.game._currentLocationP1));

        playerRef.transform.position = new Vector3(P1Position.x, playerRef.transform.position.y, -P1Position.y);
        enemyRef.transform.position = new Vector3(P2Position.x, enemyRef.transform.position.y, -P2Position.y);
        */

        if(player1Blocks.Count != currentRoom.game._P1blocks.Count)
        {
            GameObject temp = (GameObject)Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
            player1Blocks.Add(temp);
        }
        if (player2Blocks.Count != currentRoom.game._P2blocks.Count)
        {
            GameObject temp = (GameObject)Instantiate(OpponentPrefab, Vector3.zero, Quaternion.identity);
            player2Blocks.Add(temp);
        }

        for (int i = 0; i < currentRoom.game._P1blocks.Count; i++)
        {
            MapManager.Position P1Position = MapManager.getCoordinates(currentRoom.game._P1blocks[i]);
            player1Blocks[i].transform.position = new Vector3(P1Position.x, player1Blocks[0].transform.position.y, -P1Position.y);
        }

        for (int i = 0; i < currentRoom.game._P2blocks.Count; i++)
        {
            MapManager.Position P2Position = MapManager.getCoordinates(currentRoom.game._P2blocks[i]);
            player2Blocks[i].transform.position = new Vector3(P2Position.x, player2Blocks[0].transform.position.y, -P2Position.y);
        }

        MapManager.Position bugP1Position = MapManager.getCoordinates(currentRoom.game.bugLocationP1);
        player1Bug.transform.position = new Vector3(bugP1Position.x, player1Blocks[0].transform.position.y, - bugP1Position.y);

        MapManager.Position bugP2Position = MapManager.getCoordinates(currentRoom.game.bugLocationP2);
        player2Bug.transform.position = new Vector3(bugP2Position.x, player2Blocks[0].transform.position.y, -bugP2Position.y);
    }

    public void InitializeMap()
    {
        foreach(Transform children in MapParent.transform)
        {
            Destroy(children.gameObject);
        }

        foreach(GameObject go in player1Blocks)
        {
            Destroy(go);
        }
        foreach (GameObject go in player2Blocks)
        {
            Destroy(go);
        }

        player1Blocks.Clear();
        player2Blocks.Clear();

        if (player1Bug != null)
            Destroy(player1Bug);

        if(player2Bug != null)
            Destroy(player2Bug);

        //Spawning map

        //Floor tile
        GameObject temp = (GameObject)Instantiate(FloorTile, new Vector3(currentRoom.game._selectedMap._xSize / 2, -0.5f, - (currentRoom.game._selectedMap._ySize / 2)), FloorTile.transform.rotation);
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
                SpawnBlock(new Vector3(x, 0, -y), currentRoom.game._selectedMap._grid[x, y], false);
                SpawnBlock(new Vector3(x, 1, -y), currentRoom.game._selectedMap._grid[x, y], true);
            }
        }

        //SpawnPlayer
        playerRef = (GameObject)Instantiate(PlayerPrefab, new Vector3(currentRoom.game._selectedMap.spawnPoint.x , 1, -currentRoom.game._selectedMap.spawnPoint.y), Quaternion.identity);
        player1Blocks.Add(playerRef);
        //Spawn Enemy
        enemyRef = (GameObject)Instantiate(OpponentPrefab, new Vector3(currentRoom.game._selectedMap.spawnPoint.x, 0, -currentRoom.game._selectedMap.spawnPoint.y), Quaternion.identity);
        player2Blocks.Add(enemyRef);

        player1Bug = (GameObject)Instantiate(bugPrefabPlayer, new Vector3(currentRoom.game._selectedMap.spawnPoint.x, -20, -currentRoom.game._selectedMap.spawnPoint.y), Quaternion.identity);
        player2Bug = (GameObject)Instantiate(bugPrefabOpponent, new Vector3(currentRoom.game._selectedMap.spawnPoint.x, -20, -currentRoom.game._selectedMap.spawnPoint.y), Quaternion.identity);

        prevDirection = Constants.GAME_DIRECTION_LEFT;

        Camera.main.transform.position = new Vector3(currentRoom.game._selectedMap._xSize / 2, Camera.main.transform.position.y, - currentRoom.game._selectedMap._ySize / 2);
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
