
/*
Add preconditions and checks for preconditions
Add root search when there is no next utility
Utilities can be interupted in the middle. 
root utilities need scores
graph needs to be acyclic for now because we spawn the chain of behaviour components at the start 
need to preconstruct the graph. Priest should point to an instance, and not create a node on demand.


check this works: completion or failure should remove the component and delete it


Each NPC has local knowledge and global knowledge
*/

using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class Utility
{
    public abstract List<Type> availableNPCs { get; }
    public void Execute() => currentBehavior.Execute();
    protected abstract void Initialize(NPC npc);
    public Utility? nextUtility;
    

#nullable disable
    public Behavior currentBehavior;
#nullable enable

}

public class PriestRoutineStart : Utility
{
    Vector3 destination = Vector3.zero;
    public override List<Type> availableNPCs => new(){ typeof(Priest) };
    protected override void Initialize(NPC npc){
        nextUtility = new PriestRoutineSayPrayers();
        var move = npc.gameObject.AddComponent<MoveBehaviour>();
        currentBehavior = move;
        move.Initialize(this, npc);
    }
}

public class PriestRoutineSayPrayers : Utility
{
    Vector3 destination = Vector3.zero;
    public override List<Type> availableNPCs => new() { typeof(Priest) };
    
    protected override void Initialize(NPC npc)
    {
        var move = npc.gameObject.AddComponent<MoveBehaviour>();
        currentBehavior = move;
        move.Initialize(this, npc);
    }
}

public class CurePoisonUtility : Utility
{
    public override List<Type> availableNPCs => new() { typeof(NPC) };

    protected override void Initialize(NPC npc)
    {
        
        var DoNothingBehaviour = npc.gameObject.AddComponent<DoNothingBehaviour>();
        currentBehavior = DoNothingBehaviour;
        
    }
}

public class SearchScrollUtility : Utility{
    public override List<Type> availableNPCs =>  new(){ typeof(Priest) };

    protected override void Initialize(NPC npc) 
        {
            var DoNothingBehaviour = npc.gameObject.AddComponent<DoNothingBehaviour>();
            currentBehavior = DoNothingBehaviour;
        }
    
}
