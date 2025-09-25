using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    public float walkSpeed = 2f;

    private bool _isMoving = false;
    private Vector3 _moveTarget = Vector3.zero;

    public bool MoveTo(Vector3 destination){
        _moveTarget = destination;
        _isMoving = true;
        return true;
    }

    void Update(){
        if (!_isMoving) return;

        float step = walkSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _moveTarget, step);

        if (Vector3.SqrMagnitude(_moveTarget - transform.position) <= 0.0001f){
            _isMoving = false;
        }
    }
}
