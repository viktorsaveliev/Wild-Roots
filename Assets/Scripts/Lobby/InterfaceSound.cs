using UnityEngine;

public class InterfaceSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] _click;
    private AudioSource _audio;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();    
    }

    private void OnEnable()
    {
        EventBus.OnPlayerClickUI += PlayClickSound;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerClickUI -= PlayClickSound;
    }

    private void PlayClickSound(int id)
    {
        StringBus stringBus= new();
        int checkSoundSettings = PlayerPrefs.GetInt(stringBus.SettingsSoundFX);
        if (checkSoundSettings == 0) 
        {
            _audio.clip = _click[id];
            _audio.Play();
        }
    }
}
