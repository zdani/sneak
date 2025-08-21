public class Player
{
    public string PlayerId { get; private set; }
    public string DisplayName { get; private set; }
   

    public Player(string displayName)
    {
        PlayerId = System.Guid.NewGuid().ToString();
        DisplayName = displayName;
    }
}


