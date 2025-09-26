using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "StatusChange", story: "[NPC] Status Changed", category: "Conditions", id: "707d16f9538a20dabbe5d6866df13787")]
public partial class StatusChangeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> NPC;

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
