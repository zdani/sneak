using System;

public static class ServiceLocator
{
    private static IScoringManager? _scoringManager;

    public static void RegisterScoringManager(IScoringManager service) => _scoringManager = service;

    public static IScoringManager GetScoringManager()
    {
        if (_scoringManager == null)
        {
            throw new Exception("Scoring manager not registered");
        }
        return _scoringManager;
    }
    

    public static void ResetInstance()
    {
        _scoringManager = null;
    }
}
