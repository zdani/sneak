using System.Collections.Generic;
using UnityEngine;

public class ScoringManager : IScoringManager
{
    private readonly Dictionary<Player, int> playerToScore = new Dictionary<Player, int>();

    public void AddToScore(int amount)
    {
        
    }

    public int GetScore()
    {
        return 0;
    }
}


