using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Camera viewCamera;
    [SerializeField] private LayerMask groundMask;

    void Reset()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!viewCamera) viewCamera = Camera.main;
        if (groundMask.value == 0) groundMask = LayerMask.GetMask("Floor");
    }

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!viewCamera) viewCamera = Camera.main;
        if (groundMask.value == 0) groundMask = LayerMask.GetMask("Floor");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TrySetDestinationFromMouse();
        }
    }

    public void SetMoveTarget(Vector3 worldPosition)
    {
        if (!agent) return;

        if (NavMesh.SamplePosition(worldPosition, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
        else
        {
            agent.SetDestination(worldPosition);
        }
    }

    private void TrySetDestinationFromMouse()
    {
        if (!viewCamera || !agent) return;

        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        bool hitSomething;
        RaycastHit hit;

        if (groundMask.value != 0)
        {
            hitSomething = Physics.Raycast(ray, out hit, 500f, groundMask);
        }
        else
        {
            hitSomething = Physics.Raycast(ray, out hit, 500f);
        }

        if (!hitSomething) return;

        Vector3 target = hit.point;
        if (NavMesh.SamplePosition(target, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
        {
            target = navHit.position;
        }

        agent.SetDestination(target);
    }
}
