using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehaviour : Behavior
{
    public float speed = 5f;
  
    #nullable disable
    private Utility currentUtility;
    private NPC npc;
    #nullable enable
    private readonly Queue<IEnumerator> actionQueue = new ();
    
    void Start()
    {
        // Example sequence
        EnqueueAction(Move(Vector3.right, 2f));
        EnqueueAction(Move(Vector3.forward, 2f));
    }

    public void Initialize(Utility utility, NPC npc)
    {
        this.currentUtility = utility;
        this.npc = npc;
    }

    public void EnqueueAction(IEnumerator action)
    {
        actionQueue.Enqueue(action);
    }

    private IEnumerator RunActions()
    {
        while (actionQueue.Count > 0)
        {
            yield return StartCoroutine(actionQueue.Dequeue());
        }

        EventManager.Instance.TriggerUtilityComplete(currentUtility, npc);
    }

    private IEnumerator Move(Vector3 direction, float distance)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + direction.normalized * distance;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
    }

    public override void Execute()
    {
        StartCoroutine(RunActions());
    }

    public override void Cancel()
    {
        EventManager.Instance.TriggerUtilityFailure(currentUtility, npc);
    }
}