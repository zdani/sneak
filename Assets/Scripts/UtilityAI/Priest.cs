using UnityEngine;

public class Priest :  NPC {
   

    void Awake(){
        utility = new PriestRoutineStart();
    }

    void Start(){
        utility.Execute(this);
    }

    
}