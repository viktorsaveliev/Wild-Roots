using UnityEngine;

public interface IExplodable
{
    public void Explode(int[] viewID, Vector3 position, float force);
}