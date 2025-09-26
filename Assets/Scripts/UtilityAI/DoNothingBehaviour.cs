using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNothingBehaviour : Behavior
{
    public float speed = 5f;
  
    #nullable disable
    private Utility currentUtility;
    private NPC npc;
    #nullable enable
    
    
    void Start()
    {
        // Example sequence
       
    }

    public void Initialize(Utility utility, NPC npc)
    {
        this.currentUtility = utility;
        this.npc = npc;
    }

    public override void Execute()
    {
        
    }

    public override void Cancel()
    {
        EventManager.Instance.TriggerUtilityFailure(currentUtility, npc);
    }
}