using UnityEngine;

public class FallArea : MonoBehaviour
{
    [SerializeField] private AudioClip[] _fallSounds;
    private AudioSource _audio;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponent<Character>();
        if(character)
        {
            character.Health.PlayerFell();
            _audio.clip = _fallSounds[character.Health.Value];
            _audio.Play();
        }
        else if(other.TryGetComponent(out TakeImpulse _))
        {
            other.gameObject.SetActive(false);
        }
    }
}
