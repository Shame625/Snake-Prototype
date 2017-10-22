public class Player
{
    public string _userName { get; set; }
    public bool _inRoom { get; set; }
    public int _id { get; set; }
    public bool _findingRoom { get; set; }

    public void SetName(ref string name)
    {
        _userName = name;
        _inRoom = false;
        _findingRoom = false;
    }

    public void SetId(ref int id)
    {
        _id = id;
    }
}
