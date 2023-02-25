using UnityEngine;

[RequireComponent(typeof(Character), typeof(Rigidbody))]
public class TakeImpulse : MonoBehaviour
{
    private Character _character;
    private float _immunity;

    private void Start()
    {
        _character = GetComponent<Character>();
        _immunity = 0;
    }

    public void SetImpulse(float force, Vector3 impulsePosition, Weapon fromWhom, float knockoutTime = 2)
    {
        if (_immunity >= Time.time) return;

        _character.Health.FromWhomDamage = fromWhom;
        _character.Rigidbody.AddExplosionForce(force + _character.Health.GetDamageMultiplie(), impulsePosition, 10f);

        if(knockoutTime != 0)
        {
            _character.Health.SetDamageStrength(0.1f);
            _character.Move.SetMoveActive(false, knockoutTime);
        }

        EventBus.OnCharacterTakeDamage?.Invoke(_character);
    }

    public void SetImmunity(float time)
    {
        _immunity = Time.time + time;
    }
}
