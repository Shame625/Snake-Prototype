public class Player
{
    public string _userName { get; set; }

    public void SetName(ref string name)
    {
        _userName = name;
    }
}
