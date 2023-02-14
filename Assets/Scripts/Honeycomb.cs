using UnityEngine;
using DG.Tweening;

public class Honeycomb : MonoBehaviour
{
    public bool IsTiedWeapon;
    private int _durability = 3;

    public void TakeDamage(int strength)
    {
        if (strength < 0) strength = 0;
        _durability -= strength;

        if(_durability <= 0)
        {
            DestroyHoneycomb(2, 1);
        }
    }

    public void DestroyHoneycomb(int loopsAnimation, float timeToShake)
    {
        transform.DOShakeRotation(timeToShake, 5, 20, 90, false).OnComplete(() =>
        {
            transform.DOShakeRotation(0.15f, 5, 20).SetLoops(loopsAnimation).OnComplete(() =>
            {
                transform.DOMoveY(-20f, 3f).OnComplete(() => gameObject.SetActive(false));
            });
        });
    }
}
