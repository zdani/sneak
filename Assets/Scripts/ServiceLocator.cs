using System;

public static class ServiceLocator
{
    #nullable enable

    private static IScoringManager? _scoringManager;
    #nullable disable


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
