using UnityEngine;

public class Priest :  NPC {
    Utility utility;

    public Priest(){
        utility = new PriestRoutineStart();
    }

    void Start(){
        utility.Execute(this);
    }
}