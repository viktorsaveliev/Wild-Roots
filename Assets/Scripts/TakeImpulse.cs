using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Character), typeof(Rigidbody))]
public class TakeImpulse : MonoBehaviour
{
    [SerializeField] private GameObject _shieldSphere;
    [SerializeField] private AudioClip _shieldSound;

    private Character _character;
    private float _immunity;

    private void Start()
    {
        _character = GetComponent<Character>();
        _immunity = 0;
    }

    public void SetImpulse(float force, Vector3 impulsePosition, Weapon fromWhom, float knockoutTime = 2)
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

        _character.Rigidbody.velocity = Vector3.zero;
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
