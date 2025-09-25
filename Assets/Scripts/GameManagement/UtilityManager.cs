using System;

public class UtilityManager{
    public UtilityManager(){
        EventManager.Instance.OnUtilityComplete += HandleUtilityComplete;
        EventManager.Instance.OnUtilityFailure += HandleUtilityFailure;
    }

    private void HandleUtilityFailure(Utility utility, NPC npc)
    {
        FindBestRootUtility(npc).Execute(npc);
    }

    private void HandleUtilityComplete(Utility utility, NPC npc)
    {
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
}