using System;

public class Room
{
    public int roomId { get; set; }
    public string roomName { get; set; }
    public bool isAdmin { get; set; }
    public UInt16 roomType { get; set; }
    public Game game { get; set; }

    public Room()
    {
        game = new Game();
    }

    public Room(bool admin)
    {
        isAdmin = true;
        game = new Game();
    }
}
    