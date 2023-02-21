using UnityEngine;

[RequireComponent(typeof(Character), typeof(Rigidbody))]
public class TakeImpulse : MonoBehaviour
{
    private Character _character;

    private void Start()
    {
        _character = GetComponent<Character>();
    }

    public void SetImpulse(float force, Vector3 impulsePosition, Weapon fromWhom, bool knockout = true)
    {
        _character.Health.FromWhomDamage = fromWhom;
        _character.Rigidbody.AddExplosionForce(force + _character.Health.GetDamageMultiplie(), impulsePosition, 10f);

        if(knockout)
        {
            _character.Health.SetDamageStrength(0.1f);
            _character.Move.SetMoveActive(false);
        }

        EventBus.OnCharacterTakeDamage?.Invoke(_character);
    }
}
