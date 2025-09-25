using UnityEngine;

public class MoveBehaviour : MonoBehaviour, IBehavior
{
    public float speed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Utility currentUtility;
    private NPC npc;

    void Awake()
    {
        npc = GetComponent<NPC>();
    }

    public void Configure(Utility utility, Vector3 destination)
    {
        currentUtility = utility;
        targetPosition = destination;
    }

    void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            if (currentUtility != null && npc != null)
            {
                EventManager.Instance.TriggerUtilityComplete(currentUtility, npc);
            }
        }
    }

    public void Execute()
    {
        isMoving = true;
    }

    public void Cancel()
    {
        if (isMoving)
        {
            isMoving = false;
            EventManager.Instance.TriggerUtilityFailure(currentUtility, npc);
        }
    }
}