using UnityEngine;

public class CharacterVoice : MonoBehaviour
{
    [SerializeField] private AudioClip[] _voiceForTakeDamage;
    private AudioSource _audio;
    
    private Character _character;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        _character = GetComponent<Character>();
    }

    private void OnEnable()
    {
        EventBus.OnCharacterTakeDamage += EnableDamageVoice;
    }

    private void OnDisable()
    {
        EventBus.OnCharacterTakeDamage -= EnableDamageVoice;
    }

    private void EnableDamageVoice(Character character)
    {
        if (_character != character) return;
        _audio.clip = _voiceForTakeDamage[Random.Range(0, _voiceForTakeDamage.Length)];
        _audio.Play();
    }

}
