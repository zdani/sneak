using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    protected Utility utility;

    public void MoveTo(Vector3 destination, Utility utility)
    {
        targetPosition = destination;
        isMoving = true;
        this.utility = utility;
    }

    private void Update()
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
            EventManager.Instance.TriggerUtilityComplete(utility, this);
        }
    }

    public void CancelMove()
    {
        if (isMoving)
        {
            isMoving = false;
            EventManager.Instance.TriggerUtilityFailure(utility, this);
        }
    }
}
