using UnityEngine;

public abstract class Enemy : MonoBehaviour
{

    private Weapon _currentWeapon;
    public Transform WeaponPosition;

    protected string Name;
    protected float Strength;
    protected float Speed;

    public virtual void Init()
    {

    }

    public void GiveWeapon(Weapon weapon)
    {
        _currentWeapon = weapon;
    }

    private void FindNearestPlayer()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(_currentWeapon == null)
        {
            PlayerInfo nearest = collision.gameObject.GetComponent<PlayerInfo>();
            if (!nearest) return;

        }
    }
}
