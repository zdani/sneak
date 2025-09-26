using UnityEngine;

public abstract class Behavior : MonoBehaviour {
    public abstract void Execute();
    public abstract void Cancel();
}