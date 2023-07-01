using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class Car : MonoBehaviour
{
    [SerializeField] private AudioClip _crash;

    private bool _isRide;
    private readonly float _impulseForce = 1200f;

    public void Go()
    {
        if (_isRide) return;
        transform.DOScale(3f, 1f);
        GetComponent<AudioSource>().Play();
        _isRide = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<TakeImpulse>(out var takeImpulse))
        {
            takeImpulse.SetImpulse(_impulseForce, transform.position, 0, -1, -1);
            takeImpulse.SetImmunity(1f);
            AudioSource.PlayClipAtPoint(_crash, transform.position);
        }
    }
}