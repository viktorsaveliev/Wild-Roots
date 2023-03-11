using UnityEngine;

public class InterfaceSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] _click;
    private AudioSource _audio;

    private void Awake()
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

    public void PlayClickSound(int id)
    {
        if (id < 0 || id > _click.Length - 1) id = _click.Length - 1;

        StringBus stringBus= new();
        int checkSoundSettings = PlayerPrefs.GetInt(stringBus.SettingsSoundFX);
        if (checkSoundSettings == 0) 
        {
            _audio.clip = _click[id];
            _audio.Play();
        }
    }
}
