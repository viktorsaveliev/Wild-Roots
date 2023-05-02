using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class Car : MonoBehaviour
{
    [SerializeField] private AudioClip _crash;

    private bool _isRide;
    private readonly float _impulseForce = 2500f;
    private AudioSource _audio;
    
    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    public void Go(Vector3 startPos, Vector3 endPos, float speed)
    {
        if (_isRide) return;

        transform.position = startPos;
        _audio.Play();
        transform.DOMove(endPos, speed).OnComplete(() =>
        {
            _isRide = false;
            _audio.Stop();
            gameObject.SetActive(false);
        });

        _isRide = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<TakeImpulse>(out var takeImpulse))
        {
            takeImpulse.SetImpulse(_impulseForce, transform.position, null);
            takeImpulse.SetImmunity(1f);
            AudioSource.PlayClipAtPoint(_crash, transform.position);
        }
    }
}
