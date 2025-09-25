using UnityEngine;

public interface IBehavior{
    void Configure(Utility utility, Vector3 destination);
    void Execute();
    void Cancel();
}