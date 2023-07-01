using UnityEngine;

public interface IMoveable
{
    public void Move(Vector3 direction);
    public void Stop();
    public void Rotate(Vector3 direction);
    public void SetMoveActive(bool active, float time = 2, bool activateRoots = false, bool freezeRotate = false);
    public bool IsCanMove();
}
