using UnityEngine;

public class PreloadFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] _preloadExplode;

    void Start()
    {
        for(int i = 0; i < _preloadExplode.Length; i++)
        {
            _preloadExplode[i].Play();
            Destroy(_preloadExplode[i].gameObject, 1f);
        }

        LoadingUI.Hide();
    }
}
