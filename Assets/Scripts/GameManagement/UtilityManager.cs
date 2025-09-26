using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;

public class UtilityManager{

    List<Utility> rootUtilities = new List<Utility>();
    public UtilityManager(){
        EventManager.Instance.OnUtilityComplete += HandleUtilityComplete;
        EventManager.Instance.OnUtilityFailure += HandleUtilityFailure;
    }


    public void CreateGraph(){
        rootUtilities.Add(new PriestRoutineStart());
        rootUtilities.Add(new CurePoisonUtility());
        rootUtilities.Add(new SearchScrollUtility());
    }

    private void HandleUtilityFailure(Utility utility, NPC npc)
    {
        CleanupBehaviors(utility);
        FindBestRootUtility(npc).Execute();
    }

    private void HandleUtilityComplete(Utility utility, NPC npc)
    {
        CleanupBehaviors(utility);
        if (utility.nextUtility != null){
            utility.nextUtility.Execute();
        }
        else{
            FindBestRootUtility(npc).Execute();
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