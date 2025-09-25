
/*
Add preconditions and checks for preconditions
Add root search when there is no next utility
Utilities can be interupted in the middle. 
root utilities need scores

Each NPC has local knowledge and global knowledge
*/



using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;


public abstract class Utility{
    public abstract List<Type>availableNPCs {get;}
    public bool isRoot = false;
    public Utility? nextUtility = null;

    protected abstract bool ExecuteInternal(NPC npc);
    public void Execute(NPC npc){
        var result = ExecuteInternal(npc);
        if (result){
            EventManager.Instance.TriggerUtilityComplete(this, npc);
        }
        else{
            EventManager.Instance.TriggerUtilityFailure(this, npc);
        }
    }
}

public class PriestRoutineStart : Utility
{
    Vector3 destination = Vector3.zero;
    public override List<Type> availableNPCs => new(){ typeof(Priest) };
    public PriestRoutineStart(){
        isRoot = true;
        nextUtility = new PriestRoutineSayPrayers();
    }

    protected override bool ExecuteInternal (NPC npc)
    {
        return npc.MoveTo(destination);
    }
}

public class PriestRoutineSayPrayers : Utility
{
    Vector3 destination = Vector3.zero;
    public override List<Type> availableNPCs => new(){ typeof(Priest) };
    public PriestRoutineSayPrayers(){
    }

    protected override bool ExecuteInternal (NPC npc)
    {
        return npc.MoveTo(destination);
    }
}

public class CurePoisonUtility : Utility{
    public override List<Type> availableNPCs => new(){ typeof(NPC) };
    public CurePoisonUtility(){
        isRoot = true;
    }

    protected override bool ExecuteInternal(NPC npc)
    {
        throw new NotImplementedException();
    }
}

public class SearchScrollUtility : Utility{
    public override List<Type> availableNPCs =>  new(){ typeof(Priest) };
    public SearchScrollUtility(){
        isRoot = true;
    }

    protected override bool ExecuteInternal(NPC npc)
    {
        throw new NotImplementedException();
    }
}
