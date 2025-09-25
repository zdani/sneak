using UnityEngine;

public class Priest :  NPC {
    Utility utility;

    void Awake(){
        utility = new PriestRoutineStart();
    }

    void Start(){
        utility.Execute(this);
    }

    void OnEnable(){
        EventManager.Instance.OnUtilityComplete += HandleUtilityComplete;
    }

    void OnDisable(){
        EventManager.Instance.OnUtilityComplete -= HandleUtilityComplete;
    }

    private void HandleUtilityComplete(Utility u, NPC npc){
        if (npc != this) return;
        if (u.nextUtility != null){
            utility = u.nextUtility;
            utility.Execute(this);
        }
    }
}