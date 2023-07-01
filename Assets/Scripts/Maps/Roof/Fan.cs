using UnityEngine;

public class Fan : MonoBehaviour
{
    [SerializeField] private ParticleSystem _windFX;
    private readonly float _fanForce = 80f;
    private readonly float _fanForceToSide = 27f;

    private bool _isEnable = false;
    public bool IsEnable => _isEnable;

    private AudioSource _audio;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (_isEnable == false) return;
        if (other.gameObject.TryGetComponent<Rigidbody>(out var rb))
        {
            if (other.gameObject.TryGetComponent<Character>(out var character))
            {
                if (character.Move.IsCanMove() && character.transform.position.y > 1.8f)
                {
                    character.Move.SetMoveActive(false, -1);
                    if (rb.angularVelocity == Vector3.zero)
                    {
                        rb.AddTorque(Random.insideUnitSphere * 100f);
                    }
                }
            }

            rb.AddForce(transform.up * _fanForce);

            int random = Random.Range(0, 3);
            if(random == 1)
            {
                rb.AddForce(transform.forward * _fanForceToSide);
            }
            else
            {
                rb.AddForce(_fanForceToSide * -1 * transform.forward);
            }
        }
    }

    public void Enable()
    {
        if (_isEnable) return;
        _isEnable = true;
        _windFX.Play();
        _audio.Play();
    }

    public void Disable()
    {
        if (_isEnable == false) return;
        _isEnable = false;
        _windFX.Stop();
        _audio.Stop();
    }
}
