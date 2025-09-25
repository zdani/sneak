using System;
using UnityEngine;

public class UtilityManager{
    public UtilityManager(){
        EventManager.Instance.OnUtilityComplete += HandleUtilityComplete;
        EventManager.Instance.OnUtilityFailure += HandleUtilityFailure;
    }

    private void HandleUtilityFailure(Utility utility, NPC npc)
    {
        CleanupBehaviors(utility);
        FindBestRootUtility(npc).Execute(npc);
    }

    private void HandleUtilityComplete(Utility utility, NPC npc)
    {
        CleanupBehaviors(utility);
        if (utility.nextUtility != null){
            utility.nextUtility.Execute(npc);
        }
        else{
            FindBestRootUtility(npc).Execute(npc);
        }
    }

    private Utility FindBestRootUtility(NPC npc){
        throw new NotImplementedException();
    }

    private void CleanupBehaviors(Utility utility){
        
        var behavior = utility.currentBehavior;
        UnityEngine.Object.Destroy(behavior);      
    }
}