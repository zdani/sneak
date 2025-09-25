
/*
Add preconditions and checks for preconditions
Add root search when there is no next utility
Utilities can be interupted in the middle. 
root utilities need scores

use ibehaviour to call configure and execute instead of the concrete
completion or failure should remove the component and delete it
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
    public Behavior? currentBehavior = null;

    public abstract void Execute(NPC npc);
}

public class PriestRoutineStart : Utility
{
    Vector3 destination = Vector3.zero;
    public override List<Type> availableNPCs => new(){ typeof(Priest) };
    public PriestRoutineStart(){
        isRoot = true;
        nextUtility = new PriestRoutineSayPrayers();
    }

    public override void Execute (NPC npc)
    {
         var move = npc.gameObject.AddComponent<MoveBehaviour>();
         currentBehavior = move;
         move.Configure(this, destination);
         move.Execute();
    }
}

public class PriestRoutineSayPrayers : Utility
{
    Vector3 destination = Vector3.zero;
    public override List<Type> availableNPCs => new(){ typeof(Priest) };
    public PriestRoutineSayPrayers(){
    }

     public override void Execute (NPC npc)
    {
         var move = npc.gameObject.AddComponent<MoveBehaviour>();
         move.Configure(this, destination);
         move.Execute();
    }
}

public class CurePoisonUtility : Utility{
    public override List<Type> availableNPCs => new(){ typeof(NPC) };
    public CurePoisonUtility(){
        isRoot = true;
    }

    public override void Execute (NPC npc)
    {
        throw new NotImplementedException();
    }
}

public class SearchScrollUtility : Utility{
    public override List<Type> availableNPCs =>  new(){ typeof(Priest) };
    public SearchScrollUtility(){
        isRoot = true;
    }

     public override void Execute (NPC npc)
    {
        throw new NotImplementedException();
    }
}
