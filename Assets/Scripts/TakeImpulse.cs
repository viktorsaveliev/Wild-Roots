using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class TakeImpulse : MonoBehaviour
{
    [SerializeField] private GameObject _shieldSphere;
    [SerializeField] private AudioClip _shieldSound;

    private float _immunity = 0;
    private Character _character;
    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void SetImpulse(float force, Vector3 impulsePosition, float upwardsModifier, int fromWhom, float knockoutTime = 0)
    {
        if (_immunity >= Time.time)
        {
            if (_shieldSphere != null)
            {
                _shieldSphere.transform.DOShakeScale(0.3f, 1.2f, 10, 60);
                AudioSource.PlayClipAtPoint(_shieldSound, transform.position);
            }
            return;
        }

        float increase = 0;
        if(gameObject.layer == 6)
        {
            if(_character == null)
            {
                _character = GetComponent<Character>();
            }

            if (knockoutTime != 0)
            {
                _character.Health.SetDamageStrength(0.1f);
                _character.Move.SetMoveActive(false, knockoutTime);
            }
            _character.Health.FromWhomDamage = fromWhom;
            transform.DOPunchScale(new Vector3(0.5f, 0.5f), 0.3f).OnComplete(() => _character.transform.localScale = new Vector3(2, 2, 2));

            increase = _character.Health.GetDamageMultiplie();

            EventBus.OnCharacterTakeDamage?.Invoke(_character);
        }

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddExplosionForce(force + increase, impulsePosition, 10f, upwardsModifier);
        if (_rigidbody.angularVelocity == Vector3.zero)
        {
            _rigidbody.AddTorque(Random.insideUnitSphere * 100f);
        }
    }

    public void SetImmunity(float time)
    {
        _immunity = Time.time + time;
        if(_shieldSphere != null)
        {
            _shieldSphere.SetActive(true);
            _shieldSphere.transform.DOScaleX(0.7f, 0.2f).OnComplete(() => _shieldSphere.transform.DOScaleZ(0.7f, 0.2f));
            _shieldSphere.transform.DOScaleY(1f, 0.3f);

            StartCoroutine(InactiveShield(time));
        }
    }

    private IEnumerator InactiveShield(float time)
    {
        yield return new WaitForSeconds(time);

        if (gameObject.activeSelf == false) yield break;
        _shieldSphere.transform.DOScale(0.1f, 0.5f).OnComplete(() =>
        {
            _immunity = 0;
            _shieldSphere.SetActive(false);
        });
    }
}
