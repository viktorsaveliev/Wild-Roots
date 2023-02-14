using UnityEngine;

[RequireComponent(typeof(PlayerInfo), typeof(Rigidbody))]
public class TakeImpulse : MonoBehaviour
{
    private PlayerInfo _player;

    private void Start()
    {
        _player = GetComponent<PlayerInfo>();
    }

    public void SetImpulse(float force, Vector3 impulsePosition, Weapon fromWhom, bool knockout = true)
    {
        _player.Health.FromWhomDamage = fromWhom;
        _player.Rigidbody.AddExplosionForce(force + _player.Health.GetDamageDamageMultiplie(), impulsePosition, 10f);

        if(knockout)
        {
            _player.Health.SetDamageStrength(0.1f);
            if(_player.Move) _player.Move.SetMoveActive(false);
        }

        EventBus.OnPlayerTakeDamage?.Invoke(_player);
    }
}
