
/*
Each utility has a next action to perform. Check for preconditoins. If we cant perform it, evaluate all top level nodes to find the best one.
Exectute should be run async. Once it completes, trigger an event. The event will run the next utility. If there is non, find the next best root utility.
Utilities can be interupted in the middle. 
Each NPC has local knowledge and global knowledge

Add runner class that searches top level nodes appropriate for the current NPC
*/



using System.Collections.Generic;
using System;
using UnityEngine;


public abstract class Utility{
    public abstract List<Type>availableNPCs {get;}
    public abstract void Execute(NPC npc);
    public bool isRoot = false;
    public Utility? nextUtility = null;
}

public class PriestRoutineStart : Utility
{
    Vector3 destination = Vector3.zero;
    public override List<Type> availableNPCs => new(){ typeof(Priest) };
    public PriestRoutineStart(){
        isRoot = true;
        nextUtility = new PriestRoutineSayPrayers();
    }

    public override void Execute(NPC npc)
    {
        var currentPosition = npc.transform.position;
        if (currentPosition != destination)
        {
            float moveSpeed = 2f;
            var newPosition = Vector3.MoveTowards(
                currentPosition,
                destination,
                moveSpeed * Time.deltaTime
            );
            npc.transform.position = newPosition;
        }
    }
}

public class PriestRoutineSayPrayers : Utility
{
    Vector3 destination = Vector3.zero;
    public override List<Type> availableNPCs => new(){ typeof(Priest) };
    public PriestRoutineSayPrayers(){
    }

    public override void Execute(NPC npc)
    {
        
    }
}

public class CurePoisonUtility : Utility{
    public override List<Type> availableNPCs => new(){ typeof(NPC) };
    public CurePoisonUtility(){
        isRoot = true;
    }

    public override void Execute(NPC npc)
    {
        throw new System.NotImplementedException();
    }
}

public class SearchScrollUtility : Utility{
    public override List<Type> availableNPCs =>  new(){ typeof(Priest) };
    public SearchScrollUtility(){
        isRoot = true;
    }

    public override void Execute(NPC npc)
    {
        throw new System.NotImplementedException();
    }
}
